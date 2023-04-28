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

#if UNITY_EDITOR
            DebugExtension.DebugWireSphere(transform.position, Color.blue, 2.0f, 2.0f);
            Debug.Log(string.Format("numhits: {0}", hits.Length));
#endif
        }
    }
}
