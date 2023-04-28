using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyMovement : MonoBehaviour
{
    [Header("Basic stats")]
    [SerializeField]
    float f_MaxSpeed = 1.0f;
    [SerializeField]
    float f_WanderStrength = 1.0f;
    [SerializeField]
    float f_SteerStrength = 1.0f;

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
    [SerializeField]
    [Range(0,1)]
    float f_FoodPriority = 0.5f;


    static Transform s_Pheromones = null;


    Transform r_Head;
    Transform r_TF;
    Rigidbody r_RB;


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
    float m_PassedDistance          = 0.0f;

    //State
    EnemyState m_State              = EnemyState.Searching;
    Transform m_TargetFood          = null;
    Transform m_TargetPrey          = null;

    void Awake()
    {
        if(s_Pheromones == null) { s_Pheromones = new GameObject("Pheromones").transform; }

        r_TF = transform;
        r_Head = r_TF.Find("Head");
        r_RB = GetComponent<Rigidbody>();
    }

    void Start()
    {
        int seed = Random.Range(0, 1000000000);
        Debug.Log(seed);
        Random.InitState(seed);

        StartCoroutine("senseObstacle");
        // StartCoroutine("sensePheromoneEnumerator");
    }

    // Update is called once per frame
    void Update()
    {
        moveWander();
    }

    void moveWander()
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

        dispatchPheromone();
    }

    void calculateDirection()
    {
        Vector2 newDirection = Vector2.zero;

        if(m_Wandering)     newDirection += (m_Direction + Random.insideUnitCircle * f_WanderStrength).normalized * f_WanderPriority;
        if(m_Turning)       newDirection += m_TurnDirection * f_TurnPriority;
        if(m_Percepting)    newDirection += m_PerceptionDirection * f_PerceptionPriority;
        //TODO: Add Food
        //TODO: Add enemy

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
            int layer = m_State == EnemyState.Searching ? Layer.s_Instance.m_ToFoodPheromoneMask : Layer.s_Instance.m_ToNestPheromoneMask;

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

    void sensePheromone()
    {
        Quaternion offset1 = Quaternion.Euler(0, f_PheromoneSenseOffsetDeg, 0);
        Quaternion offset2 = Quaternion.Inverse(offset1);

        int layer = m_State == EnemyState.Searching ? Layer.s_Instance.m_ToFoodPheromoneMask : Layer.s_Instance.m_ToNestPheromoneMask;

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
        m_PassedDistance += m_Velocity.magnitude;
        if(m_PassedDistance >= f_PheromoneDispatchDelay)
        {
            m_PassedDistance -= f_PheromoneDispatchDelay;
            Pheromone p = Instantiate(f_Pheromone, r_TF.position, Quaternion.identity, s_Pheromones).GetComponent<Pheromone>();
            p.setPheromoneType(m_State == EnemyState.Searching ? PheromoneType.ToNest : PheromoneType.ToFood);
        }
    }

    void OnTriggerEnter(Collider other) 
    {
        Vector3 point = other.ClosestPoint(r_TF.position);
        Vector3 normal = r_TF.position - point;

#if UNITY_EDITOR
        Debug.DrawRay(point, normal, Color.green, 5);
#endif

        r_TF.position += normal.normalized * 0.1f;
    }
}

public enum EnemyState
{
    Searching   = 0,
    Returning   = 1,
    Fighting    = 2,
}

public enum EnemyMovementType
{
    Wandering   = 0,
    Navigating  = 1,
}