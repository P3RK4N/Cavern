using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class SquareMarcher
{
    static Vector3 TL = new Vector3(-1.0f, 0.0f, 1.0f);
    static Vector3 TR = new Vector3(1.0f, 0.0f, 1.0f);
    static Vector3 BL = new Vector3(-1.0f, 0.0f, -1.0f);
    static Vector3 BR = new Vector3(1.0f, 0.0f, -1.0f);
    static Vector3 T  = new Vector3(0.0f, 0.0f, 1.0f);
    static Vector3 B  = new Vector3(0.0f, 0.0f, -1.0f);
    static Vector3 L  = new Vector3(-1.0f, 0.0f, 0.0f);
    static Vector3 R  = new Vector3(1.0f, 0.0f, 0.0f);

    static Vector3 dTL = new Vector3(-1.0f, -10.0f, 1.0f);
    static Vector3 dTR = new Vector3(1.0f, -10.0f, 1.0f);
    static Vector3 dBL = new Vector3(-1.0f, -10.0f, -1.0f);
    static Vector3 dBR = new Vector3(1.0f, -10.0f, -1.0f);
    static Vector3 dT  = new Vector3(0.0f, -10.0f, 1.0f);
    static Vector3 dB  = new Vector3(0.0f, -10.0f, -1.0f);
    static Vector3 dL  = new Vector3(-1.0f, -10.0f, 0.0f);
    static Vector3 dR  = new Vector3(1.0f, -10.0f, 0.0f);

    /*
    *   2----3
    *   |    |
    *   |    |
    *   0----1
    *
    *   z|__
    *     x
    */

    static readonly Vector3[][] s_Byte2WallVertices = new Vector3[][]
    {
        // 0b0000
        new Vector3[]
        {

        },
        
        // 0b0001
        new Vector3[]
        {
            B, L, dB,
            dB, L, dL,
        },

        // 0b0010
        new Vector3[]
        {
            R, B, dR,
            dR, B, dB,
        },

        // 0b0011
        new Vector3[]
        {
            R, L, dR,
            dR, L, dL,
        },

        // 0b0100
        new Vector3[]
        {
            L, T, dT,
            dT, dL, L,
        },

        // 0b0101
        new Vector3[]
        {
            B, T, dT,
            dT, dB, B,
        },

        // 0b0110
        new Vector3[]
        {
            R, B, dR,
            dR, B, dB,

            L, T, dT,
            dT, dL, L,
        },

        // 0b0111
        new Vector3[]
        {
            R, T, dT,
            dT, dR, R,
        },

        // 0b1000
        new Vector3[]
        {
            T, R, dT,
            dR, dT, R,
        },

        // 0b1001
        new Vector3[]
        {
            R, T, dT,
            dT, dR, R,

            L, B, dB,
            L, dB, dL,
        },

        // 0b1010
        new Vector3[]
        {
            T, B, dB,
            dB, dT, T,
        },

        // 0b1011
        new Vector3[]
        {
            T, L, dT,
            dL, dT, L,
        },

        // 0b1100
        new Vector3[]
        {
            L, R, dR,
            L, dR, dL,
        },

        // 0b1101
        new Vector3[]
        {
            B, R, dR,
            B, dR, dB,
        },

        // 0b1110
        new Vector3[]
        {
            L, B, dB,
            L, dB, dL,
        },

        // 0b1111
        new Vector3[]
        {

        },
    };

    static readonly Vector3[][] s_Byte2Vertices = new Vector3[][]
    {
        // 0b0000
        new Vector3[]
        {

        },
        
        // 0b0001
        new Vector3[]
        {
            L, B, BL
        },

        // 0b0010
        new Vector3[]
        {
            B, R, BR,
        },

        // 0b0011
        new Vector3[]
        {
            L, R, BL,
            BL, R, BR,
        },

        // 0b0100
        new Vector3[]
        {
            L, TL, T,
        },

        // 0b0101
        new Vector3[]
        {
            T, BL, TL,
            BL, T, B,
        },

        // 0b0110
        new Vector3[]
        {
            B, R, BR,
            L, TL, T,
        },

        // 0b0111
        new Vector3[]
        {
            T, BL, TL,
            R, BL, T,
            BL, R, BR,
        },

        // 0b1000
        new Vector3[]
        {
            T, TR, R,
        },

        // 0b1001
        new Vector3[]
        {
            T, TR, R,
            L, B, BL,
        },

        // 0b1010
        new Vector3[]
        {
            T, BR, B,
            BR, T, TR,
        },

        // 0b1011
        new Vector3[]
        {
            L, BR, BL,
            T, BR, L,
            BR, T, TR,
        },

        // 0b1100
        new Vector3[]
        {
            TL, R, L,
            R, TL, TR,
        },

        // 0b1101
        new Vector3[]
        {
            TL, B, BL,
            B, TL, R,
            TL, TR, R,
        },

        // 0b1110
        new Vector3[]
        {
            TL, TR, L,
            TR, B, L,
            TR, BR, B,
        },

        // 0b1111
        new Vector3[]
        {
            TL, TR, BL,
            TR, BR, BL,
        },
    };


    static Vector3 m_Offset = Vector3.zero;

    public static void s_CreateCave(float size, int subdivisions, Func<float, float, int> filter, out Mesh caveTop, out Mesh caveWall)
    {
        caveTop = new Mesh();
        caveWall = new Mesh();

        List<Vector3> topVertices = new List<Vector3>();
        List<Vector3> wallVertices = new List<Vector3>();

        float step = size / subdivisions;
        float halfSize = size / 2.0f;
        float halfStep = step / 2.0f;
        float half3Step = halfStep * 3.0f;

        for(float x = -halfSize + halfStep; x <= halfSize; x += step)
            for(float y = -halfSize + halfStep; y <= halfSize; y += step)
            {
                int index = 0;

                index += filter(x - halfStep, y - halfStep) << 0;
                index += filter(x + halfStep, y - halfStep) << 1;
                index += filter(x - halfStep, y + halfStep) << 2;
                index += filter(x + halfStep, y + halfStep) << 3;

                m_Offset.x = x;
                m_Offset.z = y;

                // Top Vertices
                for(int i = 0; i < s_Byte2Vertices[index].Length; i += 3)
                {
                    topVertices.Add(s_Byte2Vertices[index][i] * halfStep + m_Offset);
                    topVertices.Add(s_Byte2Vertices[index][i+1] * halfStep + m_Offset);
                    topVertices.Add(s_Byte2Vertices[index][i+2] * halfStep + m_Offset);
                }

                // Wall vertices
                for(int i = 0; i < s_Byte2WallVertices[index].Length; i += 3)
                {
                    wallVertices.Add(s_Byte2WallVertices[index][i] * halfStep + m_Offset);
                    wallVertices.Add(s_Byte2WallVertices[index][i+1] * halfStep + m_Offset);
                    wallVertices.Add(s_Byte2WallVertices[index][i+2] * halfStep + m_Offset);
                }
            }

        int[] topIndices = new int[topVertices.Count];
        int[] wallIndices = new int[wallVertices.Count];

        Vector2[] topUVs = new Vector2[topVertices.Count];

        for(int i = 0; i < topVertices.Count; i++) 
        {
            topIndices[i] = i;
            topUVs[i].x = topVertices[i].x;
            topUVs[i].y = topVertices[i].z;
        }

        for(int i = 0; i < wallVertices.Count; i++)
        {
            wallIndices[i] = i;
        }

        caveTop.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        caveTop.SetVertices(topVertices);
        caveTop.SetIndices(topIndices, MeshTopology.Triangles, 0);
        caveTop.SetUVs(0, topUVs);

        caveWall.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        caveWall.SetVertices(wallVertices);
        caveWall.SetIndices(wallIndices, MeshTopology.Triangles, 0);
    }
}
