using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [SerializeField]
    float f_Radius = 7.0f;
    [SerializeField]
    float f_TurnSpeedDeg = 80.0f;
    [SerializeField]
    float f_Cooldown = 1.0f;
    [SerializeField]
    float f_MaxOffsetDeg = 5.0f;
    [SerializeField]
    GameObject f_Bullet = null;

    Transform r_Head;
    Transform r_TF;
    Transform r_Target = null;

    Collider[] m_EnemyCollider = new Collider[1];
    float m_Cooldown = 0.0f;

    void Awake()
    {
        r_Head = transform.Find("Head");
        r_TF = transform;
    }

    void Start()
    {
        StartCoroutine("checkForEnemy");
    }

    void Update()
    {
        if(r_Target != null) handleTarget();
    }

    void handleTarget()
    {
        Vector3 dir = - r_Head.position + r_Target.position;
        if(Vector3.SqrMagnitude(dir) > f_Radius*f_Radius) r_Target = null;
        else
        {
            Quaternion desiredAngle = Quaternion.LookRotation(dir);
            r_Head.rotation = Quaternion.RotateTowards(r_Head.rotation, desiredAngle, f_TurnSpeedDeg * Time.deltaTime);

            m_Cooldown = Mathf.Max(0.0f, m_Cooldown - Time.deltaTime);

            if(m_Cooldown < 0.01f && Quaternion.Angle(desiredAngle, r_Head.rotation) < f_MaxOffsetDeg)
            {
                Instantiate(f_Bullet, r_Head.position + r_Head.forward, r_Head.rotation);
                m_Cooldown = f_Cooldown;
            }
        }
    }

    IEnumerator checkForEnemy()
    {
        WaitForSeconds wait = new WaitForSeconds(1.0f);
        while(true)
        {
            if(r_Target == null)
            {
                Physics.OverlapSphereNonAlloc(r_TF.position, f_Radius, m_EnemyCollider, Layer.s_Instance.m_EnemyMask, QueryTriggerInteraction.Collide);
                if(m_EnemyCollider[0])
                {
                    r_Target = m_EnemyCollider[0].transform; //Body because only it has trigger
                }
            }
            yield return wait;
        }
    }
}
