using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pheromone : MonoBehaviour
{

[Header("Pheromone stats")]
[SerializeField]
float lifespan      = 30.0f;
[SerializeField]
PheromoneType type  = PheromoneType.ToNest;

    Material mat;

    public float alive = 0.0f;

    void Awake()
    {
        mat = GetComponent<MeshRenderer>().material;
    }

    void Update()
    {
        evaporate();
    }

    void evaporate()
    {
        alive += Time.deltaTime;
        float left = 1.0f - alive/lifespan;
        if(left <= 0.0f) 
        {
            Destroy(gameObject);
            return;
        }
        Color c = mat.color;
        mat.color = new Color(c.r, c.g, c.b, left);
    }
}

public enum PheromoneType
{
    ToNest  = 0,
    ToFood  = 1,
    ToEnemy = 2
}
