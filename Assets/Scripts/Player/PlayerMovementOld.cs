using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using Cinemachine;

public class PlayerMovementOld : MonoBehaviour
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
    Joystick ljoy;
    Joystick rjoy;
    Camera cam;
    CinemachineFreeLook cfl;

    float lookAngle = 0;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        ljoy = GameObject.Find("LeftJoystick").GetComponent<Joystick>();
        rjoy = GameObject.Find("RightJoystick").GetComponent<Joystick>();
        cam = FindObjectOfType<Camera>();
        cfl = FindObjectOfType<CinemachineFreeLook>();
    }

    void Update()
    {
        move();
        rotate();
    }

    // void MovePresed(InputAction.CallbackContext context)
    // {
    //     Debug.Log("Move Start");
    //     movement = context.ReadValue<Vector2>();
    // }

    // void MoveReleased(InputAction.CallbackContext context)
    // {
    //     Debug.Log("Move End");
    //     movement = Vector3.zero;
    // }

    // void LookPresed(InputAction.CallbackContext context)
    // {
    //     Debug.Log("Look Start");
    //     look = context.ReadValue<Vector2>();
    // }

    // void LookReleased(InputAction.CallbackContext context)
    // {
    //     Debug.Log("Look End");
    //     look = Vector2.zero;
    // }

    void move()
    {
        #if UNITY_EDITOR

        Vector2 dir = Vector2.zero;
        dir.x = ljoy.Direction.x + (Input.GetKey(KeyCode.A) ? -1.0f : 0.0f) + (Input.GetKey(KeyCode.D) ? 1.0f : 0.0f);
        dir.y = ljoy.Direction.y + (Input.GetKey(KeyCode.S) ? -1.0f : 0.0f) + (Input.GetKey(KeyCode.W) ? 1.0f : 0.0f);
        
        dir = Vector2.ClampMagnitude(dir, 1);
        Vector3 mov = cam.transform.right * dir.x + cam.transform.forward * dir.y;
        mov = new Vector3(mov.x, 0, mov.z).normalized * dir.magnitude * moveSpeed;

        #else

        if(ljoy.Direction == Vector2.zero) return;
        Vector3 mov = cam.transform.right * ljoy.Direction.x + cam.transform.forward * ljoy.Direction.y;
        mov = new Vector3(mov.x, 0, mov.z).normalized * ljoy.Direction.magnitude * moveSpeed;

        #endif

        cc.SimpleMove(mov);
    }

    void rotate()
    {
        // xAngle += look.x * lookSpeed;
        // xAngle %= 360.0f;

        // cfl.m_XAxis.Value += Time.deltaTime * lookSpeed * look.x * 60;
        
        // cameraLookAt.transform.position = transform.position + new Vector3(forwardLookOffset * Mathf.Sin(xAngle + 180.0f), upLookOffset, forwardLookOffset * Mathf.Cos(xAngle + 180.0f));
    }
}