using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillOnAwake : MonoBehaviour
{
    void Awake()
    {
        Destroy(gameObject);
    }
}
