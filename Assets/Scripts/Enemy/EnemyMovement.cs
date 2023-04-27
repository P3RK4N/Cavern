using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{

[Header("Basic stats")]
[SerializeField]
float maxSpeed = 1.0f;
[SerializeField]
float wanderStrength = 1.0f;
[SerializeField]
float steerStrength = 1.0f;

[Space(10)]
[Header("Sensing")]

[SerializeField]
float antennaOffset = 0.1f;
[SerializeField]
float obstacleSenseDelaySec = 1.0f;
[SerializeField]
float obstacleSenseDistance = 1.0f;
[SerializeField]
float obstacleAvoidDistance = 8.0f;
[SerializeField]
float maxTurnaroundOffset = 30.0f;
[SerializeField]
float turnaroundTime = 1.0f;

[Space(10)]
[Header("Steering priorities")]

[SerializeField]
[Range(0,1)]
float wanderPriority = 0.5f;
[SerializeField]
[Range(0,1)]
float turnPriority = 0.5f;
[SerializeField]
[Range(0,1)]
float perceptionPriority = 0.5f;
[SerializeField]
[Range(0,1)]
float foodPriority = 0.5f;

[Space(10)]
[Header("Pheromones")]
[SerializeField]
GameObject pheromoneCollider;
[SerializeField]
float pheromoneDelay = 2.0f;

    public static readonly int s_ObstacleLayer = 0b100000000;

    /* ENEMY STATE */

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
    bool m_Percepting               = false;

    //Pheromones
    Transform pheromones            = null;
    float passedDistance            = 0.0f;


    /* ENEMY PARTS */

    Transform m_Head;
    Rigidbody rb;

    void Awake()
    {
        m_Head = transform.Find("Head");
        rb = GetComponent<Rigidbody>();
        pheromones = new GameObject("Pheromones").transform;
    }

    void Start()
    {
        int seed = Random.Range(0, 1000000000);
        Debug.Log(seed);
        Random.InitState(seed);

        StartCoroutine("senseObstacle");
    }

    // Update is called once per frame
    void Update()
    {
        move();
    }

    void move()
    {
        calculateDirection();

        Vector2 wantedVelocity = m_Direction * maxSpeed;
        Vector2 steerVelocity = (wantedVelocity - m_Velocity) * steerStrength;
        Vector2 acceleration = Vector2.ClampMagnitude(steerVelocity, steerStrength);

        m_Velocity = Vector2.ClampMagnitude(m_Velocity + acceleration * Time.deltaTime, maxSpeed);
        Vector3 velocity3d = new Vector3(m_Velocity.x, 0, m_Velocity.y);

        transform.position += velocity3d;

        passedDistance += m_Velocity.magnitude;
        if(passedDistance >= pheromoneDelay)
        {
            passedDistance -= pheromoneDelay;
            Instantiate(pheromoneCollider, transform.position, Quaternion.identity, pheromones);
        }

        transform.rotation = Quaternion.LookRotation(velocity3d, Vector3.up);
    }

    void calculateDirection()
    {
        Vector2 newDirection = Vector2.zero;

        if(m_Wandering)     newDirection += (m_Direction + Random.insideUnitCircle * wanderStrength).normalized * wanderPriority;
        if(m_Turning)       newDirection += m_TurnDirection * turnPriority;
        if(m_Percepting)    newDirection += m_PerceptionDirection * perceptionPriority;
        //TODO: Add Food

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
                    hit |= Physics.Raycast(m_Head.position, m_Head.forward - m_Head.right * antennaOffset, obstacleSenseDistance, s_ObstacleLayer);
                    Debug.DrawRay(m_Head.position, m_Head.forward - m_Head.right * antennaOffset, hit ? Color.red : Color.green, 1.0f);
                }
                //Right
                {
                    hit |= Physics.Raycast(m_Head.position, m_Head.forward + m_Head.right * antennaOffset, obstacleSenseDistance, s_ObstacleLayer);
                    Debug.DrawRay(m_Head.position, m_Head.forward + m_Head.right * antennaOffset, hit ? Color.red : Color.green, 1.0f);
                }

                if(hit) StartCoroutine("turn");
            }
            yield return new WaitForSeconds(obstacleSenseDelaySec);
        }
    }

    IEnumerator turn()
    {
        m_Turning = true;
        steerStrength *= 1.5f;

        GetComponentInChildren<MeshRenderer>().material.color = Color.red;
        m_TurnDirection = findTurnaroudDirection();
        yield return new WaitForSeconds(turnaroundTime);

        m_Turning = false;
        steerStrength /= 1.5f;
        GetComponentInChildren<MeshRenderer>().material.color = Color.gray;
    }

    Vector2 findTurnaroudDirection(int tries = 5)
    {
        float distance = 0.0f;
        Vector3 furthestVec = Vector2.zero;

        RaycastHit hit;
        while(tries-- > 0)
        {
            Vector3 turnDir = Quaternion.Euler(0,Random.Range(-maxTurnaroundOffset, maxTurnaroundOffset), 0) * -transform.forward;
            Ray ray = new Ray(transform.position, turnDir);
            
            bool hasHit = Physics.Raycast(ray, out hit, obstacleAvoidDistance, s_ObstacleLayer);
            
            Debug.DrawRay(transform.position, turnDir * 2, Color.gray, 10.0f);
            Debug.DrawRay(transform.position, turnDir + new Vector3(0, 0.2f, 0), Color.gray, 10.0f);

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

        Debug.DrawRay(transform.position, furthestVec * 2, Color.magenta, 10.0f);
        Debug.DrawRay(transform.position, furthestVec + new Vector3(0, 1, 0), Color.magenta, 10.0f);
        return new Vector2(furthestVec.x, furthestVec.z);
    }

    void OnTriggerEnter(Collider other) 
    {
        Vector3 point = other.ClosestPoint(transform.position);
        Vector3 normal = transform.position - point;
        Debug.DrawRay(point, normal, Color.green, 5);
        transform.position += normal.normalized * 0.1f;
    }
}
