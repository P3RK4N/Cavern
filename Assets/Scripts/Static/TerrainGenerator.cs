using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainGenerator
{
    // public class Island
    // {
    //     public Island(List<Vector2Int> cells)
    //     {
    //         Cells = cells;
    //         // Fill the rest
    //     }
    //     public Island(HashSet<Vector2Int> cells)
    //     {
    //         Cells = new List<Vector2Int>(cells);
    //         // Fill the rest
    //     }

    //     public Vector2 Mean;
    //     public List<Vector2Int> Cells;
    //     public List<Vector2Int> EdgeCells;

    //     public static Dictionary<Vector2Int, Island> CellToIsland = new Dictionary<Vector2Int, Island>();
    // }

    // static void reduceIslands(float[,] map)
    // {

    // }

    public static Mesh s_CreateNoisyTerrain(float size, int subdivisions, Noise.PerlinSettings ps, float intensity = 1.0f)
    {
        // Mesh --------------
        Mesh terrain = new Mesh();
        terrain.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        // Mesh --------------

        // Noise Map ---------
        float[,] perlinNoise = Noise.perlinNoise(size, subdivisions, ps);
        // Noise Map ---------

        // Vertices ----------
        List<Vector3> terrainVertices = new List<Vector3>();
        float halfSize = size / 2.0f;
        float step = size / subdivisions;
        int i = -1, j = -1;
        for(float x = -halfSize; x <= halfSize + Mathf.Epsilon; x += step)
        {
            i++;
            j = -1;
            for(float y = -halfSize; y <= halfSize + Mathf.Epsilon; y += step)
            {
                j++;
                Vector3 vertex = new Vector3(x, perlinNoise[i,j], y);
                vertex.y -= 0.5f;
                vertex.y *= intensity;
                terrainVertices.Add(vertex);
            }
        }
        terrain.SetVertices(terrainVertices);
        // Vertices ----------

        // Indices -----------
        int[] terrainIndices = new int[subdivisions*subdivisions*6];
        int k = 0;
        for(i = 1; i < subdivisions + 1; i++)
            for(j = 0; j < subdivisions; j++)
            {
                terrainIndices[k++] = (subdivisions+1)*i + j;
                terrainIndices[k++] = (subdivisions+1)*(i-1) + j;
                terrainIndices[k++] = (subdivisions+1)*i + j + 1;

                terrainIndices[k++] = (subdivisions+1)*(i-1) + j;
                terrainIndices[k++] = (subdivisions+1)*(i-1) + j + 1;
                terrainIndices[k++] = (subdivisions+1)*i + j + 1;
            }
        terrain.SetIndices(terrainIndices, MeshTopology.Triangles, 0);
        // Indices -----------

        terrain.RecalculateBounds();
        terrain.RecalculateNormals();

        return terrain;
    }

}
