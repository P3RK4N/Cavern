using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillOnAwake : MonoBehaviour
{
    [SerializeField]
    bool f_Enabled = false;

    void Awake()
    {
        if(f_Enabled) Destroy(gameObject);
    }
}
