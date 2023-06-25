using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.HealthSystemCM;

public class Enemy : MonoBehaviour
{
    EnemyMovement r_Movement;
    Transform r_Hitbox;
    Animator r_Animator;

    HealthSystem m_Health;
    public bool m_Attacking = false;

    void Awake()
    {
        r_Movement = GetComponent<EnemyMovement>();
        r_Hitbox = transform.Find("Hitbox");
        r_Animator = GetComponent<Animator>();
        m_Health = new HealthSystem(15.0f);
        m_Health.OnDead += onDeath;
    }

    void Update()
    {
        if(!tryAttack())
        {

        }
    }

    bool tryAttack()
    {
        if
        (
            !m_Attacking && 
            r_Movement.m_TargetPrey != null && 
            Physics.CheckSphere(r_Hitbox.position, 0.7f, Layer.s_Instance.m_PlayerMask)
        )
        {
            StartCoroutine("attack");
            return true;
        }
        return false;
    }

    WaitForSeconds wait = new WaitForSeconds(0.666f);
    IEnumerator attack()
    {
        m_Attacking = true;
        r_Movement.m_Moving = false;

        r_Animator.SetTrigger("AttackTrigger");
        yield return wait;

        m_Attacking = false;
        r_Movement.m_Moving = true;
    }

    //TODO Move to EnemyCollision
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == Layer.s_Instance.m_PlayerHitboxLayer)
        {
            Debug.Log(other.name);
            m_Health.Damage(5.0f);
        }
    }

    void onDeath(object sender, System.EventArgs args)
    {
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        m_Health.OnDead -= onDeath;
    }
}
