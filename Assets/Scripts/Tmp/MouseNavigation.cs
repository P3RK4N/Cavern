using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MouseNavigation : MonoBehaviour
{
    Camera r_CAM;
    NavMeshAgent r_NMA;
 
    void Awake()
    {
        r_CAM = FindObjectOfType<Camera>();
        r_NMA = GetComponent<NavMeshAgent>();
    }
 
    void Update()
    {
        navigate();
        if(Input.GetKey(KeyCode.E)) r_NMA.enabled = !r_NMA.enabled;
        Debug.Log("vel " + r_NMA.velocity.sqrMagnitude);
    }

    void navigate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            int layerMask = Layer.s_Instance.m_GroundMask;
            Ray ray = r_CAM.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000.0f, layerMask))
            {
                r_NMA.SetDestination(hit.point);
            }
        }
    }
}
