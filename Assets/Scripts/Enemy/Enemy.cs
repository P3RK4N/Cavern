using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    EnemyMovement r_Movement;
    Transform r_Hitbox;
    Animator r_Animator;

    bool m_Attacking = false;

    void Awake()
    {
        r_Movement = GetComponent<EnemyMovement>();
        r_Hitbox = transform.Find("Hitbox");
        r_Animator = GetComponent<Animator>();
    }

    void Update()
    {
        tryAttack();
    }

    void tryAttack()
    {
        if(!m_Attacking && Physics.CheckSphere(r_Hitbox.position, 0.7f, Layer.s_Instance.m_PlayerMask)) StartCoroutine("attack");
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
}
