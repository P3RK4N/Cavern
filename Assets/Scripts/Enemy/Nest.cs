using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Nest : MonoBehaviour
{
    TMP_Text r_TXT;
    Transform r_Label;
    Transform r_Camera;
    Transform r_TF;

    int m_Value = 0;

    void Awake()
    {
        r_TXT = GetComponentInChildren<TMP_Text>();
        r_TF = transform;
        r_Label = r_TF.GetChild(0);
        r_Camera = FindObjectOfType<Camera>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        lookToCam();
    }

    void lookToCam()
    {
        r_Label.rotation = Quaternion.LookRotation(- r_Camera.position + r_TF.position, Vector3.up);
    }

    public void increment()
    {
        m_Value++;
        r_TXT.text = m_Value + "";
    }
}
