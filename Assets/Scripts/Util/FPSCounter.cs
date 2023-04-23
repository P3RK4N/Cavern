using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class FPSCounter : MonoBehaviour
{

    TMP_Text txt;

    void Awake()
    {
        txt = GetComponent<TMP_Text>();
    }

    void Start()
    {
        InvokeRepeating("framerate", 0.0f, 1.0f);
    }

    void framerate()
    {
        txt.text = string.Format("Framerate: {0}", 1.0f / Time.deltaTime);
    }
}
