using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSenser : MonoBehaviour
{
    void Start()
    {
        StartCoroutine("senseParticles");
    }

    IEnumerator senseParticles()
    {
        while(true)
        {
            yield return new WaitForSeconds(1.0f);

            Collider[] hits = Physics.OverlapSphere(transform.position, 2.0f, Layer.s_Instance.m_ToFoodPheromoneMask, QueryTriggerInteraction.Collide);
        }
    }
}
