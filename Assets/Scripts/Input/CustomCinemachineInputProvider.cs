using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cinemachine;

public class CustomCinemachineInputProvider : CinemachineInputProvider
{
    public override float GetAxisValue(int axis)
    {
        if(axis == 0) return base.GetAxisValue(axis);
        else return 0;
    }
}
