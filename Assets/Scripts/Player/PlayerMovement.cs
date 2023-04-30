using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{

//############################
[Header("Movement")]
[SerializeField]
float f_WalkSpeed = 3.5f;
[SerializeField]
float f_SprintSpeed = 7.0f;
[SerializeField]
float f_Stamina = 100.0f;
[SerializeField]
float f_StaminaRegen = 15.0f;
[SerializeField]
float f_StaminaWaste = 30.0f;

[Space]
[Header("Camera")]
[SerializeField]
float lookSpeed = 0.1f;
[SerializeField]
float lookHeight = 3.0f;
[SerializeField]
float lookDistance = 4.0f;
[SerializeField]
float forwardLookOffset = 1.0f;
[SerializeField]
float upLookOffset = 1.0f;
//############################

    CharacterController r_CC;
    PlayerInput r_PI;
    Joystick r_JOY;
    Camera r_CAM;
    CinemachineFreeLook r_CFL;
    Player r_Player;

    Vector2 m_Movement = Vector3.zero;
    float m_CurrentStamina = 0.0f;

    void Awake()
    {
        r_Player = GetComponent<Player>();
        r_CC = GetComponent<CharacterController>();
        r_JOY = FindObjectOfType<DynamicJoystick>();
        r_CAM = FindObjectOfType<Camera>();
        r_PI = GetComponent<PlayerInput>();
        r_CFL = FindObjectOfType<CinemachineFreeLook>();

        m_CurrentStamina = f_Stamina;
        r_Player.m_Stamina = new CodeMonkey.HealthSystemCM.HealthSystem(m_CurrentStamina);
    }

    void Start()
    {
        r_Player.r_StaminaUI.SetHealthSystem(r_Player.m_Stamina);
    }

    void OnEnable()
    {
        r_PI.ActivateInput();
        r_PI.actions["Move"].performed += MovePresed;
        r_PI.actions["Move"].canceled += MoveReleased;
    }


    void OnDisable()
    {
        r_PI.DeactivateInput();

        r_PI.actions["Move"].performed -= MovePresed;
        r_PI.actions["Move"].canceled -= MoveReleased;
    }

    void Update()
    {
        move();
        r_Player.m_Stamina.SetHealth(m_CurrentStamina);
    }

    void MovePresed(InputAction.CallbackContext context)
    {
        m_Movement = context.ReadValue<Vector2>();
    }

    void MoveReleased(InputAction.CallbackContext context)
    {
        m_Movement = Vector3.zero;
    }

    void move()
    {
        float magnitude = m_Movement.magnitude;
        float speed = f_WalkSpeed;
        #if UNITY_EDITOR && NULL
        if(Input.GetKey(KeyCode.LeftShift))
        #else
        if(magnitude > 0.99f)
        #endif
        {
            if(m_CurrentStamina > 0.5f) speed = f_SprintSpeed;
            m_CurrentStamina = Mathf.Max(0.0f, m_CurrentStamina - Time.deltaTime * f_StaminaWaste);
        }
        else
        {
            m_CurrentStamina = Mathf.Min(f_Stamina, m_CurrentStamina + Time.deltaTime * f_StaminaRegen);
        }

        Vector3 mov = r_CAM.transform.right * m_Movement.x + r_CAM.transform.forward * m_Movement.y;
        mov = new Vector3(mov.x, 0, mov.z).normalized * magnitude * speed;
        r_CC.SimpleMove(mov);
    }

}