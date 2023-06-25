using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    [SerializeField]
    float f_WalkSpeedFac = 15.0f;
    [SerializeField]
    float f_WalkSFXVolumeFac = 1.0f;
    [SerializeField]
    float f_WalkSFXPitchFac = 1.0f;

    Transform r_TF;
    Rigidbody r_RB;
    Animator r_Animator;
    AudioSource r_AudioSource;

    Enemy r_Enemy;
    EnemyMovement r_EnemyMovement;

    void Awake()
    {
        r_TF = transform;
        r_RB = GetComponent<Rigidbody>();
        r_Animator = transform.Find("Parts/Body").GetComponent<Animator>();
        r_AudioSource = GetComponent<AudioSource>();
        r_Enemy = GetComponent<Enemy>();
        r_EnemyMovement = GetComponent<EnemyMovement>();
    }

    void Update()
    {
        float enemyMoveSpeed = r_EnemyMovement.moveSpeed();
        handleWalkAnimation(enemyMoveSpeed);
        handleWalkSFX(enemyMoveSpeed);
    }

    void handleWalkAnimation(float moveSpeed)
    {
        if(!r_Enemy.m_Attacking)
        {
            r_Animator.SetFloat("WalkSpeed", moveSpeed * f_WalkSpeedFac);
        }
        else
        {
            r_Animator.SetFloat("WalkSpeed", 0.0f);
        }
    }

    // TEMP
    void handleWalkSFX(float moveSpeed)
    {
        float audioStart = 0.08f;
        float audioEnd = 1.57f;

        // r_AudioSource.pitch = moveSpeed * f_WalkSFXPitchFac;
        r_AudioSource.volume = moveSpeed * f_WalkSFXVolumeFac;

        if(r_AudioSource.time > audioEnd) r_AudioSource.time = audioStart;
    }
}
