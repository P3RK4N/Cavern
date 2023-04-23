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
float moveSpeed = 1.0f;
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

    CharacterController cc;
    PlayerInput pi;
    Joystick joy;
    Camera cam;
    CinemachineFreeLook cfl;
    CinemachineInputProvider cip;
    Transform cameraLookAt;

    Vector2 movement = Vector3.zero;
    Vector2 look = Vector2.zero;

    float xAngle = 0;
    float yAngle = 0;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        joy = FindObjectOfType<DynamicJoystick>();
        cam = FindObjectOfType<Camera>();
        pi = GetComponent<PlayerInput>();
        cfl = FindObjectOfType<CinemachineFreeLook>();
        cip = FindObjectOfType<CinemachineInputProvider>();
    }

    void OnEnable()
    {
        pi.ActivateInput();
        pi.actions["Move"].performed += MovePresed;
        pi.actions["Move"].canceled += MoveReleased;
        
        pi.actions["Look"].performed += LookPresed;
        pi.actions["Look"].canceled += LookReleased;
    }


    void OnDisable()
    {
        pi.DeactivateInput();

        pi.actions["Move"].performed -= MovePresed;
        pi.actions["Move"].canceled -= MoveReleased;

        pi.actions["Look"].performed -= LookPresed;
        pi.actions["Look"].canceled -= LookReleased;
    }

    void Update()
    {
        move();
        rotate();
    }

    void MovePresed(InputAction.CallbackContext context)
    {
        Debug.Log("Move Start");
        movement = context.ReadValue<Vector2>();
    }

    void MoveReleased(InputAction.CallbackContext context)
    {
        Debug.Log("Move End");
        movement = Vector3.zero;
    }

    void LookPresed(InputAction.CallbackContext context)
    {
        Debug.Log("Look Start");
        look = context.ReadValue<Vector2>();
    }

    void LookReleased(InputAction.CallbackContext context)
    {
        Debug.Log("Look End");
        look = Vector2.zero;
    }

    void move()
    {
        Vector3 mov = cam.transform.right * movement.x + cam.transform.forward * movement.y;

        // mov = new Vector3(mov.x, 0, mov.z).normalized * joy.Direction.magnitude * moveSpeed;
        mov = new Vector3(mov.x, 0, mov.z).normalized * movement.magnitude * moveSpeed;

        cc.SimpleMove(mov);
    }

    void rotate()
    {
        // xAngle += look.x * lookSpeed;
        // xAngle %= 360.0f;

        cfl.m_XAxis.Value += Time.deltaTime * lookSpeed * look.x * 60;
        
        // cameraLookAt.transform.position = transform.position + new Vector3(forwardLookOffset * Mathf.Sin(xAngle + 180.0f), upLookOffset, forwardLookOffset * Mathf.Cos(xAngle + 180.0f));
    }
}