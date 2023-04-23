using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.EventSystems;


[RequireComponent(typeof(Joystick))]
public class CustomOnScreenStick : OnScreenControl, IDragHandler, IEndDragHandler
{

[InputControl(layout = "*")]
[SerializeField]
public string m_ControlPath;

    Joystick joy;

    void Awake()
    {
        joy = GetComponent<Joystick>();
    }

    public void OnDrag(PointerEventData data)
    {
        SendValueToControl<Vector2>(joy.Direction);
    }

    public void OnEndDrag(PointerEventData data)
    {
        SendValueToControl<Vector2>(Vector2.zero);
    }

    protected override string controlPathInternal
    {
        get => m_ControlPath;
        set => m_ControlPath = value;
    }
}
