using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshUtil
{
    public static Vector3[] s_IndicesToVertices(IList<int> indices, IList<Vector3> vertices)
    {
        Debug.Log(string.Format("{0} Vertices", indices.Count));
        Vector3[] outVertices = new Vector3[indices.Count];

        for(int i = 0; i < indices.Count; i++)
        {
            outVertices[i] = vertices[indices[i]];
        }

        return outVertices;
    }
}
