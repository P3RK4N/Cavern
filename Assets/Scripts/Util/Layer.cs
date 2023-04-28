using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Layer : MonoBehaviour
{
    public static Layer s_Instance;

    public int m_GroundMask             { get; private set; }
    public int m_EnemyMask              { get; private set; }
    public int m_PlayerMask             { get; private set; }
    public int m_ObstacleMask           { get; private set; }
    public int m_FoodMask               { get; private set; }
    public int m_ToNestPheromoneMask    { get; private set; }
    public int m_ToFoodPheromoneMask    { get; private set; }
    public int m_ToEnemyPheromoneMask   { get; private set; }

    public int m_GroundLayer             { get; private set; }
    public int m_EnemyLayer              { get; private set; }
    public int m_PlayerLayer             { get; private set; }
    public int m_ObstacleLayer           { get; private set; }
    public int m_FoodLayer               { get; private set; }
    public int m_ToNestPheromoneLayer    { get; private set; }
    public int m_ToFoodPheromoneLayer    { get; private set; }
    public int m_ToEnemyPheromoneLayer   { get; private set; }

    void Awake()
    {
        if(s_Instance != null) Destroy(this);
        else
        {
            s_Instance = this;
            queryMasks();
        }
    }

    void queryMasks()
    {
        m_GroundMask = LayerMask.GetMask("Ground");
        m_EnemyMask = LayerMask.GetMask("Enemy");
        m_PlayerMask = LayerMask.GetMask("Player");
        m_ObstacleMask = LayerMask.GetMask("Obstacle");
        m_FoodMask = LayerMask.GetMask("Food");
        m_ToNestPheromoneMask = LayerMask.GetMask("NestPheromone");
        m_ToFoodPheromoneMask = LayerMask.GetMask("FoodPheromone");
        m_ToEnemyPheromoneMask = LayerMask.GetMask("EnemyPheromone");

        m_GroundLayer = LayerMask.NameToLayer("Ground");
        m_EnemyLayer = LayerMask.NameToLayer("Enemy");
        m_PlayerLayer = LayerMask.NameToLayer("Player");
        m_ObstacleLayer = LayerMask.NameToLayer("Obstacle");
        m_FoodLayer = LayerMask.NameToLayer("Food");
        m_ToNestPheromoneLayer = LayerMask.NameToLayer("NestPheromone");
        m_ToFoodPheromoneLayer = LayerMask.NameToLayer("FoodPheromone");
        m_ToEnemyPheromoneLayer = LayerMask.NameToLayer("EnemyPheromone");
    }
}
