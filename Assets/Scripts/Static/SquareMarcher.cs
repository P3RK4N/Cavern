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

    static Vector3 dTL = new Vector3(-1.0f, -1.0f, 1.0f);
    static Vector3 dTR = new Vector3(1.0f, -1.0f, 1.0f);
    static Vector3 dBL = new Vector3(-1.0f, -1.0f, -1.0f);
    static Vector3 dBR = new Vector3(1.0f, -1.0f, -1.0f);
    static Vector3 dT  = new Vector3(0.0f, -1.0f, 1.0f);
    static Vector3 dB  = new Vector3(0.0f, -1.0f, -1.0f);
    static Vector3 dL  = new Vector3(-1.0f, -1.0f, 0.0f);
    static Vector3 dR  = new Vector3(1.0f, -1.0f, 0.0f);

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

    static readonly Vector3[][] s_Byte2TopVertices = new Vector3[][]
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


    static readonly Vector3 m_OffsetVec0 = new Vector3(-0.5f, -0.5f, -0.5f);
    static Vector3 m_Offset = Vector3.zero;
    static Vector3[] m_TmpTriangle = new Vector3[3];

    public static void s_CreateCave(CaveSpec cs, out Mesh caveTop, out Mesh caveWall)
    {
        Func<float, float, int> filter = (x,y) => Noise.samplePerlinNoise2x1(x, y, cs.wallPerlinSettings) > cs.perlinThreshold ? 0 : 1;
        Func<float, float> calc = (x) => cs.wallQuadraticDisplacement.x * Mathf.Pow(x-cs.wallQuadraticDisplacement.y, 2.0f) + cs.wallQuadraticDisplacement.z;

        caveTop = new Mesh();
        caveWall = new Mesh();

        List<Vector3> topVertices = new List<Vector3>();
        List<Vector3> wallVertices = new List<Vector3>();

        float step = cs.size / cs.wallResolution;
        float halfSize = cs.size / 2.0f;
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

                // Top Vertices -> Making Grid
                for(int i = 0; i < s_Byte2TopVertices[index].Length; i += 3)
                {
                    // Vertex transforms
                    m_TmpTriangle[0] = s_Byte2TopVertices[index][i] * halfStep + m_Offset;
                    m_TmpTriangle[1] = s_Byte2TopVertices[index][i+1] * halfStep + m_Offset;
                    m_TmpTriangle[2] = s_Byte2TopVertices[index][i+2] * halfStep + m_Offset;

                    // Height
                    m_TmpTriangle[0].y = (cs.wallVerticalSegments - 1) * cs.wallHeight / cs.wallVerticalSegments;
                    m_TmpTriangle[1].y = (cs.wallVerticalSegments - 1) * cs.wallHeight / cs.wallVerticalSegments;
                    m_TmpTriangle[2].y = (cs.wallVerticalSegments - 1) * cs.wallHeight / cs.wallVerticalSegments;

                    // Gradients / directions
                    Vector3 dir0 = Noise.samplePerlinDirection(m_TmpTriangle[0].x, m_TmpTriangle[0].z, cs.wallPerlinSettings);
                    Vector3 dir1 = Noise.samplePerlinDirection(m_TmpTriangle[1].x, m_TmpTriangle[1].z, cs.wallPerlinSettings);
                    Vector3 dir2 = Noise.samplePerlinDirection(m_TmpTriangle[2].x, m_TmpTriangle[2].z, cs.wallPerlinSettings);
                    
                    // Adding function offset
                    m_TmpTriangle[0] += calc(m_TmpTriangle[0].y) * dir0;
                    m_TmpTriangle[1] += calc(m_TmpTriangle[1].y) * dir1;
                    m_TmpTriangle[2] += calc(m_TmpTriangle[2].y) * dir2;

                    // Adding noise displacement
                    m_TmpTriangle[0] += Mathf.Pow(m_TmpTriangle[0].y / cs.wallHeight, cs.wallNoiseFalloff) * cs.wallNoiseIntensity * dir0 * Noise.samplePerlinNoise3x1(m_TmpTriangle[0].x, m_TmpTriangle[0].y, m_TmpTriangle[0].z, cs.wallNoiseDisplacementPerlinSettings);
                    m_TmpTriangle[1] += Mathf.Pow(m_TmpTriangle[1].y / cs.wallHeight, cs.wallNoiseFalloff) * cs.wallNoiseIntensity * dir1 * Noise.samplePerlinNoise3x1(m_TmpTriangle[1].x, m_TmpTriangle[1].y, m_TmpTriangle[1].z, cs.wallNoiseDisplacementPerlinSettings);
                    m_TmpTriangle[2] += Mathf.Pow(m_TmpTriangle[2].y / cs.wallHeight, cs.wallNoiseFalloff) * cs.wallNoiseIntensity * dir2 * Noise.samplePerlinNoise3x1(m_TmpTriangle[2].x, m_TmpTriangle[2].y, m_TmpTriangle[2].z, cs.wallNoiseDisplacementPerlinSettings);
                    
                    // Appending to a caveTop mesh
                    topVertices.Add(m_TmpTriangle[0]);
                    topVertices.Add(m_TmpTriangle[1]);
                    topVertices.Add(m_TmpTriangle[2]);
                }

                // Wall vertices
                for(int wallSegment = 0; wallSegment < cs.wallVerticalSegments - 1 /* TODO: Remove */; wallSegment++)
                {
                    float up = (cs.wallHeight / cs.wallVerticalSegments) * (wallSegment + 1);
                    float down = (cs.wallHeight / cs.wallVerticalSegments) * wallSegment;

                    for(int i = 0; i < s_Byte2WallVertices[index].Length; i += 3)
                    {
                        // Vertex transforms
                        m_TmpTriangle[0] = s_Byte2WallVertices[index][i] * halfStep + m_Offset;
                        m_TmpTriangle[1] = s_Byte2WallVertices[index][i+1] * halfStep + m_Offset;
                        m_TmpTriangle[2] = s_Byte2WallVertices[index][i+2] * halfStep + m_Offset;

                        // Vertices without noise and added function
                        m_TmpTriangle[0].y = m_TmpTriangle[0].y == 0.0f ? up : down;
                        m_TmpTriangle[1].y = m_TmpTriangle[1].y == 0.0f ? up : down;
                        m_TmpTriangle[2].y = m_TmpTriangle[2].y == 0.0f ? up : down;

                        // Gradients / directions
                        Vector3 dir0 = Noise.samplePerlinDirection(m_TmpTriangle[0].x, m_TmpTriangle[0].z, cs.wallPerlinSettings);
                        Vector3 dir1 = Noise.samplePerlinDirection(m_TmpTriangle[1].x, m_TmpTriangle[1].z, cs.wallPerlinSettings);
                        Vector3 dir2 = Noise.samplePerlinDirection(m_TmpTriangle[2].x, m_TmpTriangle[2].z, cs.wallPerlinSettings);

                        // Adding function offset
                        m_TmpTriangle[0] += calc(m_TmpTriangle[0].y) * dir0;
                        m_TmpTriangle[1] += calc(m_TmpTriangle[1].y) * dir1;
                        m_TmpTriangle[2] += calc(m_TmpTriangle[2].y) * dir2;
                        
                        // Adding noise displacement
                        m_TmpTriangle[0] += Mathf.Pow(m_TmpTriangle[0].y / cs.wallHeight, cs.wallNoiseFalloff) * cs.wallNoiseIntensity * dir0 * Noise.samplePerlinNoise3x1(m_TmpTriangle[0].x, m_TmpTriangle[0].y, m_TmpTriangle[0].z, cs.wallNoiseDisplacementPerlinSettings);
                        m_TmpTriangle[1] += Mathf.Pow(m_TmpTriangle[1].y / cs.wallHeight, cs.wallNoiseFalloff) * cs.wallNoiseIntensity * dir1 * Noise.samplePerlinNoise3x1(m_TmpTriangle[1].x, m_TmpTriangle[1].y, m_TmpTriangle[1].z, cs.wallNoiseDisplacementPerlinSettings);
                        m_TmpTriangle[2] += Mathf.Pow(m_TmpTriangle[2].y / cs.wallHeight, cs.wallNoiseFalloff) * cs.wallNoiseIntensity * dir2 * Noise.samplePerlinNoise3x1(m_TmpTriangle[2].x, m_TmpTriangle[2].y, m_TmpTriangle[2].z, cs.wallNoiseDisplacementPerlinSettings);

                        // Appending to a cave wall
                        wallVertices.Add(m_TmpTriangle[0]);
                        wallVertices.Add(m_TmpTriangle[1]);
                        wallVertices.Add(m_TmpTriangle[2]);
                    }
                }
            }

        // Setting UVs, Indices

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
        caveTop.RecalculateBounds();
        caveTop.RecalculateNormals();

        caveWall.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        caveWall.SetVertices(wallVertices);
        caveWall.SetIndices(wallIndices, MeshTopology.Triangles, 0);
        caveWall.RecalculateNormals();
        caveWall.RecalculateBounds();
    }
}
