using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    [Header("Basic stats")]
    [SerializeField]
    float f_MaxSpeed = 1.0f;
    [SerializeField]
    float f_WanderStrength = 1.0f;
    [SerializeField]
    float f_SteerStrength = 1.0f;
    [SerializeField]
    float f_DetectionDistance = 3.0f;

    [Space(10)]
    [Header("Obstacles")]

    [SerializeField]
    float f_ObstacleSenseOffset = 0.3f;
    [SerializeField]
    float f_ObstacleSenseDelaySec = 1.0f;
    [SerializeField]
    float f_ObstacleSenseDistance = 1.0f;

    [Space(10)]
    [Header("Pheromones")]
    [SerializeField]
    GameObject f_Pheromone;
    [SerializeField]
    float f_PheromoneDispatchDelay = 2.0f;
    [SerializeField]
    float f_PheromoneSenseDelaySec = 1.0f;
    [SerializeField]
    float f_PheromoneSenseDistance = 2.0f;
    [SerializeField]
    float f_PheromoneSenseOffsetDeg = 35.0f;
    [SerializeField]
    float f_PheromoneSenseRadius = 0.5f;

    [Space(10)]
    [Header("Turning")]
    [SerializeField]
    float f_TurnObstacleMinDistance = 8.0f;
    [SerializeField]
    float f_MaxTurnaroundOffsetDeg = 50.0f;
    [SerializeField]
    float f_TurnaroundTime = 1.0f;

    [Space(10)]
    [Header("Steering priorities")]
    [SerializeField]
    [Range(0,1)]
    float f_WanderPriority = 0.5f;
    [SerializeField]
    [Range(0,1)]
    float f_TurnPriority = 0.5f;
    [SerializeField]
    [Range(0,1)]
    float f_PerceptionPriority = 0.5f;


    static Transform s_Pheromones = null;


    Transform r_Head;
    Transform r_Body;
    Transform r_TF;
    Rigidbody r_RB;
    NavMeshAgent r_NMA;


    //Physics stats
    Vector2 m_Velocity              = Vector2.zero;
    Vector2 m_Direction             = Vector2.zero;

    //Wandering
    bool m_Wandering                = true;

    //Turning
    Vector2 m_TurnDirection         = Vector2.zero;
    bool m_Turning                  = false;

    //Percepting
    Vector2 m_PerceptionDirection   = Vector2.zero;
    bool m_Percepting               = true;

    //Pheromones
    bool m_Dispatching              = true;
    float m_PassedDistance          = 0.0f;

    //State
    bool m_Returning                = false;
    Transform m_TargetFood          = null;
    Transform m_TargetPrey          = null;
    Transform m_TargetNest          = null;

    void Awake()
    {
        if(s_Pheromones == null) { s_Pheromones = new GameObject("Pheromones").transform; }

        r_RB = GetComponent<Rigidbody>();
        r_NMA = GetComponent<NavMeshAgent>();
        r_TF = transform;
        r_Head = r_TF.Find("Head");
        r_Body = r_TF.Find("Body");
    }

    void Start()
    {
        int seed = Random.Range(0, 1000000000);
        Debug.Log(seed);
        Random.InitState(seed);

        StartCoroutine("senseObstacle");
        // StartCoroutine("sensePheromoneEnumerator");

        StartCoroutine("navigate");
    }

    void Update()
    {
        detection();
        behavior();
    }

    /*
        Detection of Prey, Food and Nest
        Currently works only with 1 forward ray
    */
    void detection()
    {
        int layer = Layer.s_Instance.m_FoodMask | Layer.s_Instance.m_PlayerMask | Layer.s_Instance.m_NestMask;

#if UNITY_EDITOR
        DebugExtension.DebugArrow(r_TF.position, r_TF.forward * f_DetectionDistance, Color.white, 0.1f);
#endif

        RaycastHit hit;
        Ray ray = new Ray(r_TF.position, r_TF.forward);
        if(Physics.Raycast(ray, out hit, f_DetectionDistance, layer))
        {
            int hitLayer = hit.collider.gameObject.layer;
            Transform tf = hit.collider.transform;
            
            if(hitLayer == Layer.s_Instance.m_FoodLayer)
            {
                #if UNITY_EDITOR 
                    Debug.Log("FoodDetected");
                #endif

                //Either fighting, running away, collecting, carrying, storing
                if(m_TargetPrey != null || m_TargetFood != null) return; //Maybe add if returning to return too; redundant?
                m_TargetFood = tf;
            }
            else if(hitLayer == Layer.s_Instance.m_PlayerLayer)
            {
                #if UNITY_EDITOR 
                    Debug.Log("PlayerDetected");
                #endif
                
                //Either already fights or runs away from enemy
                if(m_TargetPrey != null) return;

                //Either finished running away, was collecting, was carrying, was storing, was roaming
                //Drops food, starts fight
                if(m_TargetFood)
                {
                    m_TargetFood.parent = null;
                    m_TargetFood.GetComponent<Collider>().enabled = true;
                    m_TargetFood = null;
                }
                m_Returning = false;
                m_TargetNest = null;
                m_TargetPrey = tf;
            }
            else //Nest
            {
                #if UNITY_EDITOR 
                    Debug.Log("NestDetected");
                #endif
                
                //Ignore if not returning
                if(!m_Returning) return;

                //Happens when running away or fighting -> reset to fight or wander
                if(m_TargetPrey != null)
                {
                    //Everything is now false -> turns around and starts wandering
                    m_Returning = false;
                    m_TargetPrey = null;
                    StartCoroutine("turn");
                }
                else if(m_TargetFood != null)
                {
                    //Starts navigating to nest
                    m_TargetNest = tf;
                }
                else
                {
                    //Impossible ? Frame when runaway is cooled down
#if UNITY_EDITOR
                    Debug.Log("Impossible 1");
#endif
                    m_Returning = false;
                }
            }
        }
    }

    void behavior()
    {
        /*

        ---------------------------------------------------------------------------------------------------
        |##|NEST  RETURNING   FOOD    PREY |  ACTION      PRIORITY |             DESCRIPTION              |
        |__|_______________________________|_______________________|______________________________________|
        | 0|  0       0         0      0   |  wander         0     | wander;                              |
        | 1|  0       0         0      1   |  fight          4     | fight;                               |
        | 2|  0       0         1      0   |  collect        1     | collect;                             |
        | 3|  0       0         1      1   |  ----         ----    | not happening;  fallback to 1        |
        | 4|  0       1         0      0   |  ----         ----    | not happening;  fallback to 0        |
        | 5|  0       1         0      1   |  run away       5     | run away;                            |
        | 6|  0       1         1      0   |  carry          2     | carry;                               |
        | 7|  0       1         1      1   |  ----         ----    | not happening;  fallback to 1        |
        | 8|  1       0         0      0   |  ----         ----    | not happening;  fallback to 0        |
        | 9|  1       0         0      1   |  ----         ----    | not happening;  fallback to 1        |
        |10|  1       0         1      0   |  ----         ----    | not happening;  fallback to 2        |
        |11|  1       0         1      1   |  ----         ----    | not happening;  fallback to 1        |
        |12|  1       1         0      0   |  ----         ----    | not happening;  fallback to 0        |
        |13|  1       1         0      1   |  reset          6     | reset;          transition to 0 or 1 |
        |14|  1       1         1      0   |  store          3     | store;          transition to 0      |
        |15|  1       1         1      1   |  ----         ----    | not happening;  fallback to 1        |
        ---------------------------------------------------------------------------------------------------

        VERBOSE:
        0. Default free roaming action. Moves randomly and reacts to food pheromones, food and prey
        1. Happens during or after any action with lower priority. Moves with navigation to prey
        2. Happens during or after any action with lower priority. Moves with navigation to food
        3. Doesnt happen - during state 2 (collection) it falls back to state 1 (fighting) after spotting prey
        4. Doesnt happen - after runaway cooldown it falls back to state 0 (wandering)
        5. Happens randomly when health drops. Moves randomly and reacts to nest pheromones
        6. Happens only after state 2 (collection). Moves randomly and reacts to nest pheromones and prey
        7. Doesnt happen - during state 6 (carrying) it falls back to state 1 (fighting) after spotting prey
        8. Doesnt happen - seeing nest during wandering falls again to state 0 (wandering)
        9. Doesnt happen - cannot see nest and enemy in the same frame
        10.Doesnt happen - cannot see nest and food in the same frame
        11.Doesnt happen - cannot see nest and food and prey in the same frame
        12.Doesnt happen - frame where runaway cooldown and seeing nest happen. Falls back to state 0 (wandering)
        13.~~~Happens after seeing nest while runaway is active. Falls back to state 0 (wandering) or state 1 (fighting)
        14.Happens after seeing nest while state 6 (carrying). Moves with navigation to nest
        15.Happens after seeing enemy while state 14 (storing). Falls back to state 1 (fighting)
        */

        if(m_Returning)
        {
            r_NMA.enabled = false;
            wander();
        }
        else if(m_TargetFood != null || m_TargetPrey != null || m_TargetNest != null)
        {
            r_NMA.enabled = true;
        }
        else
        {
            r_NMA.enabled = false;
            wander();
        }

        if(m_Dispatching) dispatchPheromone();
    }

#region NAVIGATING

    IEnumerator navigate()
    {
        WaitForSeconds wait = new WaitForSeconds(1.0f);
        while(true)
        {
            yield return wait;
            if(r_NMA.enabled) 
            {
                if(m_TargetPrey != null) r_NMA.SetDestination(m_TargetPrey.position);
                else if(m_TargetFood != null) r_NMA.SetDestination(m_TargetFood.position);
                else r_NMA.SetDestination(m_TargetNest.position); //Nest
            }
        }
    }

#endregion

#region WANDERING

    //Regular wandering for now
    void wander()
    {
        sensePheromone();

        calculateDirection();

        Vector2 wantedVelocity = m_Direction * f_MaxSpeed;
        Vector2 steerVelocity = (wantedVelocity - m_Velocity) * f_SteerStrength;
        Vector2 acceleration = Vector2.ClampMagnitude(steerVelocity, f_SteerStrength);

        m_Velocity = Vector2.ClampMagnitude(m_Velocity + acceleration * Time.deltaTime, f_MaxSpeed);
        Vector3 velocity3d = new Vector3(m_Velocity.x, 0, m_Velocity.y);

        r_TF.position += velocity3d;
        r_TF.rotation = Quaternion.LookRotation(velocity3d, Vector3.up);
    }

    void calculateDirection()
    {
        Vector2 newDirection = Vector2.zero;

        if(m_Wandering)     newDirection += (m_Direction + Random.insideUnitCircle * f_WanderStrength).normalized * f_WanderPriority;
        if(m_Turning)       newDirection += m_TurnDirection * f_TurnPriority;
        if(m_Percepting)    newDirection += m_PerceptionDirection * f_PerceptionPriority;

        m_Direction = newDirection.normalized;
    }

    IEnumerator senseObstacle()
    {
        while(true)
        {
            if(!m_Turning)
            {
                bool hit = false;

                //Left
                {
                    hit |= Physics.Raycast(r_Head.position, r_Head.forward - r_Head.right * f_ObstacleSenseOffset, f_ObstacleSenseDistance, Layer.s_Instance.m_ObstacleMask);
#if UNITY_EDITOR
                    Debug.DrawRay(r_Head.position, r_Head.forward - r_Head.right * f_ObstacleSenseOffset, hit ? Color.red : Color.green, 1.0f);
#endif
                }
                //Right
                {
                    hit |= Physics.Raycast(r_Head.position, r_Head.forward + r_Head.right * f_ObstacleSenseOffset, f_ObstacleSenseDistance, Layer.s_Instance.m_ObstacleMask);
#if UNITY_EDITOR
                    Debug.DrawRay(r_Head.position, r_Head.forward + r_Head.right * f_ObstacleSenseOffset, hit ? Color.red : Color.green, 1.0f);
#endif
                }

                if(hit) StartCoroutine("turn");
            }
            yield return new WaitForSeconds(f_ObstacleSenseDelaySec);
        }
    }

    IEnumerator sensePheromoneEnumerator()
    {
        Quaternion offset1 = Quaternion.Euler(0, f_PheromoneSenseOffsetDeg, 0);
        Quaternion offset2 = Quaternion.Inverse(offset1);

        while(true)
        {
            int layer = !m_Returning ? Layer.s_Instance.m_ToFoodPheromoneMask : Layer.s_Instance.m_ToNestPheromoneMask;

            Vector3 fwd = r_TF.forward * f_PheromoneSenseDistance;
            m_PerceptionDirection = Vector2.zero;
            
            int maxVisited = 0;
            int curr = 0;
            
            //Left
            Vector3 vec = offset1 * fwd;
            if((curr = Physics.OverlapSphere(r_TF.position + vec, f_PheromoneSenseRadius, layer).Length) > maxVisited)
            {
                maxVisited = curr;
                m_PerceptionDirection = new Vector2(vec.x, vec.z);
            }

            //Forward
            vec = fwd;
            if((curr = Physics.OverlapSphere(r_TF.position + vec, f_PheromoneSenseRadius, layer).Length) > maxVisited)
            {
                maxVisited = curr;
                m_PerceptionDirection = new Vector2(vec.x, vec.z);
            }

            //Right
            vec = offset2 * fwd;
            if((curr = Physics.OverlapSphere(r_TF.position + vec, f_PheromoneSenseRadius, layer).Length) > maxVisited)
            {
                maxVisited = curr;
                m_PerceptionDirection = new Vector2(vec.x, vec.z);
            }
            
            m_PerceptionDirection.Normalize();

#if UNITY_EDITOR
            DebugExtension.DebugWireSphere(r_TF.position + offset1 * fwd, Color.magenta, f_PheromoneSenseRadius, 1.0f);
            DebugExtension.DebugWireSphere(r_TF.position + offset2 * fwd, Color.magenta, f_PheromoneSenseRadius, 1.0f);
            DebugExtension.DebugWireSphere(r_TF.position + fwd, Color.magenta, f_PheromoneSenseRadius, 1.0f);
            vec = new Vector3(m_PerceptionDirection.x, 0, m_PerceptionDirection.y);
            DebugExtension.DebugArrow(r_TF.position, vec * 2, Color.cyan, 3.0f);
#endif

            yield return new WaitForSeconds(f_PheromoneSenseDelaySec);
        }
    }

    //Handles both wandering and returning
    void sensePheromone()
    {
        Quaternion offset1 = Quaternion.Euler(0, f_PheromoneSenseOffsetDeg, 0);
        Quaternion offset2 = Quaternion.Inverse(offset1);

        int layer = !m_Returning ? Layer.s_Instance.m_ToFoodPheromoneMask : Layer.s_Instance.m_ToNestPheromoneMask;

        Vector3 fwd = r_TF.forward * f_PheromoneSenseDistance;
        m_PerceptionDirection = Vector2.zero;
        
        int maxVisited = 0;
        int curr = 0;
        
        //Left
        Vector3 vec = offset1 * fwd;
        if((curr = Physics.OverlapSphere(r_TF.position + vec, f_PheromoneSenseRadius, layer).Length) > maxVisited)
        {
            maxVisited = curr;
            m_PerceptionDirection = new Vector2(vec.x, vec.z);
        }

        //Forward
        vec = fwd;
        if((curr = Physics.OverlapSphere(r_TF.position + vec, f_PheromoneSenseRadius, layer).Length) > maxVisited)
        {
            maxVisited = curr;
            m_PerceptionDirection = new Vector2(vec.x, vec.z);
        }

        //Right
        vec = offset2 * fwd;
        if((curr = Physics.OverlapSphere(r_TF.position + vec, f_PheromoneSenseRadius, layer).Length) > maxVisited)
        {
            maxVisited = curr;
            m_PerceptionDirection = new Vector2(vec.x, vec.z);
        }
        
        m_PerceptionDirection.Normalize();

#if UNITY_EDITOR
        vec = new Vector3(m_PerceptionDirection.x, 0, m_PerceptionDirection.y);
        DebugExtension.DebugArrow(r_TF.position, vec * 2, Color.cyan, 0.2f);
#endif
    }

    IEnumerator turn()
    {
        if(m_Turning) yield break;

        m_Turning = true;
        f_SteerStrength *= 1.5f;

        GetComponentInChildren<MeshRenderer>().material.color = Color.red;
        m_TurnDirection = findTurnaroudDirection();
        yield return new WaitForSeconds(f_TurnaroundTime);

        m_Turning = false;
        f_SteerStrength /= 1.5f;
        GetComponentInChildren<MeshRenderer>().material.color = Color.gray;
    }

    Vector2 findTurnaroudDirection(int tries = 5)
    {
        float distance = 0.0f;
        Vector3 furthestVec = Vector2.zero;

        RaycastHit hit;
        while(tries-- > 0)
        {
            Vector3 turnDir = Quaternion.Euler(0,Random.Range(-f_MaxTurnaroundOffsetDeg, f_MaxTurnaroundOffsetDeg), 0) * -r_TF.forward;
            Ray ray = new Ray(r_TF.position, turnDir);
            
            bool hasHit = Physics.Raycast(ray, out hit, f_TurnObstacleMinDistance, Layer.s_Instance.m_ObstacleMask);
            
#if UNITY_EDITOR
            DebugExtension.DebugArrow(r_TF.position, turnDir * 2, Color.gray, 5.0f);
#endif

            if(!hasHit)
            {
                furthestVec = turnDir;
                break;
            }
            else if(distance < hit.distance)
            {
                distance = hit.distance;
                furthestVec = turnDir;
            }
        }

#if UNITY_EDITOR
        DebugExtension.DebugArrow(r_TF.position, furthestVec * 2, Color.magenta, 5.0f);
#endif

        return new Vector2(furthestVec.x, furthestVec.z);
    }

    void dispatchPheromone()
    {
        m_PassedDistance += !r_NMA.enabled ? m_Velocity.magnitude : r_NMA.velocity.magnitude*Time.deltaTime;
        if(m_PassedDistance >= f_PheromoneDispatchDelay)
        {
            m_PassedDistance -= f_PheromoneDispatchDelay;
            Pheromone p = Instantiate(f_Pheromone, r_TF.position, Quaternion.identity, s_Pheromones).GetComponent<Pheromone>();
            p.setPheromoneType(!m_Returning ? PheromoneType.ToNest : PheromoneType.ToFood);
        }
    }

#endregion

    void OnTriggerEnter(Collider other) 
    {
        int layer = other.gameObject.layer;
        Transform tf = other.transform;

        if(layer == Layer.s_Instance.m_ObstacleLayer)
        {
            #if UNITY_EDITOR 
                Debug.Log("Obstacle hit");
            #endif

            Vector3 point = other.ClosestPoint(r_TF.position);
            Vector3 normal = r_TF.position - point;
            r_TF.position += normal.normalized * 0.1f;
        }
        else if(layer == Layer.s_Instance.m_FoodLayer)
        {
            #if UNITY_EDITOR 
                Debug.Log("Food hit");
            #endif

            if(m_Returning || m_TargetFood != tf) return;

            tf.parent = r_TF;
            tf.GetComponent<Collider>().enabled = false;
            StartCoroutine("turn");
            m_Returning = true;
        }
        else if(layer == Layer.s_Instance.m_NestLayer)
        {
            #if UNITY_EDITOR 
                Debug.Log("Nest hit");
            #endif
        }
        else if(layer == Layer.s_Instance.m_PlayerLayer)
        {
            #if UNITY_EDITOR 
                Debug.Log("Player hit");
            #endif

            m_Dispatching = false;
        }
    }
}