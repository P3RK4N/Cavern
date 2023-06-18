using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorUtil
{
    public static Vector3 Copy(this Vector3 vec3) 
    { 
        return new Vector3(vec3.x, vec3.y, vec3.z);
    }

}
