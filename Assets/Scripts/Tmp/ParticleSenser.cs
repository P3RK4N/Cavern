using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSenser : MonoBehaviour
{
    public static readonly int s_ParticleLayer  = 0b10000000000;
    public static readonly int s_EnemyLayer     = 0b00001000000;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("senseParticles");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator senseParticles()
    {
        while(true)
        {
            yield return new WaitForSeconds(1.0f);

            Collider[] hits = Physics.OverlapSphere(transform.position, 2.0f, s_ParticleLayer | s_EnemyLayer, QueryTriggerInteraction.Collide);

            DebugExtension.DebugWireSphere(transform.position, Color.blue, 2.0f, 2.0f);
            // Debug.Log(string.Format("numhits: {0}", hits.Length));
            // foreach (var item in hits)
            // {
            //     Debug.Log(string.Format("Name: {0}", item.name));
            // }
        }
    }
}
