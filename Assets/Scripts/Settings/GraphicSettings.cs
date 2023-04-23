using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphicSettings : MonoBehaviour
{

[SerializeField]
int targetFrameRate = 60;

    public static GraphicSettings Instance;

    void Awake()
    {
        if(Instance != null) Destroy(this);
        else Instance = this;
    }

    void Start()
    {
        Application.targetFrameRate = targetFrameRate;
    }
}
