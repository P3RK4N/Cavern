using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    float f_Speed = 10.0f;
    [SerializeField]
    float f_Lifespan = 2.0f;

    Transform r_TF;

    void Awake()
    {
        r_TF = transform;
    }

    void FixedUpdate()
    {
        if(f_Lifespan < 0.01f) Destroy(gameObject);
        
        f_Lifespan -= Time.fixedDeltaTime;
        r_TF.position += r_TF.forward * Time.fixedDeltaTime * f_Speed;
    }
}
