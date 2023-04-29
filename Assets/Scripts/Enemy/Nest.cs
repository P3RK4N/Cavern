using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Nest : MonoBehaviour
{
    [SerializeField]
    GameObject f_Enemy;
    [SerializeField]
    int f_EnemyAmount;

    TMP_Text r_TXT      = null;
    Transform r_Label   = null;
    Transform r_Camera  = null;
    Transform r_TF      = null;
    Transform r_Enemies = null;

    int m_Value = 0;

    void Awake()
    {
        r_TXT = GetComponentInChildren<TMP_Text>();
        r_TF = transform;
        r_Label = r_TF.GetChild(0);
        r_Camera = FindObjectOfType<Camera>().transform;
        r_Enemies = new GameObject("Enemies").transform;
    }

    void Start()
    {
        spawnEnemies();
    }

    // Update is called once per frame
    void Update()
    {
        lookToCam();
    }

    void spawnEnemies()
    {
        Transform tf = r_TF.Find("SpawnPoints");
        int num = tf.childCount;
        for(int i = 0; i < f_EnemyAmount; i++)
        {
            Instantiate(f_Enemy, tf.GetChild(Random.Range(0,num)).position,Quaternion.identity, r_Enemies);
        }
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
