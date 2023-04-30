using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.HealthSystemCM;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public HealthBarUI r_HealthUI;
    public HealthBarUI r_StaminaUI;
    PlayerMovement r_PM;

    public HealthSystem m_Health;
    public HealthSystem m_Stamina;

    void Awake()
    {
        r_PM = GetComponent<PlayerMovement>();
        r_HealthUI = GameObject.Find("Health").GetComponent<HealthBarUI>();
        r_StaminaUI = GameObject.Find("Stamina").GetComponent<HealthBarUI>();

        m_Health = new HealthSystem(100.0f);
        r_HealthUI.SetHealthSystem(m_Health);
        m_Health.OnDead += onDeath;
    }

    void OnDestroy()
    {
        m_Health.OnDead -= onDeath;
    }

    void onDeath(object sender, System.EventArgs args)
    {
        Destroy(gameObject);
        SceneManager.LoadScene(0);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == Layer.s_Instance.m_EnemyHitboxLayer)
        {
            m_Health.Damage(5.0f);
        }
    }
}
