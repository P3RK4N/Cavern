using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pheromone : MonoBehaviour
{
    [Header("Pheromone stats")]
    [SerializeField]
    float f_Lifespan      = 30.0f;
    [SerializeField]
    PheromoneType f_Type  = PheromoneType.ToNest;

    public static Color s_ToNestColor = new Color(0.1f, 0.85f, 0.2f, 1.0f);
    public static Color s_ToFoodColor = new Color(0.15f, 0.25f, 0.8f, 1.0f);
    public static Color s_ToEnemyColor = new Color(0.9f, 0.15f, 0.2f, 1.0f);

    Material r_Mat;

    public float m_Alive = 0.0f;

    void Awake()
    {
        r_Mat = GetComponent<MeshRenderer>().material;
    }

    void Start()
    {
#if UNITY_EDITOR
        setPheromoneType(f_Type);
#endif
    }

    void Update()
    {
        evaporate();
    }

    void evaporate()
    {
        m_Alive += Time.deltaTime;
        float left = 1.0f - m_Alive/f_Lifespan;
        if(left <= 0.0f) 
        {
            Destroy(gameObject);
            return;
        }
        Color c = r_Mat.color;
        r_Mat.color = new Color(c.r, c.g, c.b, left);
    }

    public void setPheromoneType(PheromoneType t)
    {
        f_Type = t;
        switch(t)
        {
            case PheromoneType.ToNest:
            {
                r_Mat.color = s_ToNestColor;
                gameObject.layer = Layer.s_Instance.m_ToNestPheromoneLayer;
                break;
            }
            case PheromoneType.ToFood:
            {
                r_Mat.color = s_ToFoodColor;
                gameObject.layer = Layer.s_Instance.m_ToFoodPheromoneLayer;
                break;
            }
            case PheromoneType.ToEnemy:
            {
                r_Mat.color = s_ToEnemyColor;
                gameObject.layer = Layer.s_Instance.m_ToEnemyPheromoneLayer;
                break;
            }
        }
    }
}

public enum PheromoneType
{
    ToNest  = 0,
    ToFood  = 1,
    ToEnemy = 2
}
