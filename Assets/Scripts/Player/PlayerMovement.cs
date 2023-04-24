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

    Vector2 movement = Vector3.zero;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        joy = FindObjectOfType<DynamicJoystick>();
        cam = FindObjectOfType<Camera>();
        pi = GetComponent<PlayerInput>();
        cfl = FindObjectOfType<CinemachineFreeLook>();
    }

    void OnEnable()
    {
        pi.ActivateInput();
        pi.actions["Move"].performed += MovePresed;
        pi.actions["Move"].canceled += MoveReleased;
    }


    void OnDisable()
    {
        pi.DeactivateInput();

        pi.actions["Move"].performed -= MovePresed;
        pi.actions["Move"].canceled -= MoveReleased;
    }

    void Update()
    {
        move();
    }

    void MovePresed(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
    }

    void MoveReleased(InputAction.CallbackContext context)
    {
        movement = Vector3.zero;
    }

    void move()
    {
        Vector3 mov = cam.transform.right * movement.x + cam.transform.forward * movement.y;
        mov = new Vector3(mov.x, 0, mov.z).normalized * movement.magnitude * moveSpeed;
        cc.SimpleMove(mov);
    }

}