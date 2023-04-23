using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainGenerator
{
    public class Island
    {
        public Island(List<Vector2Int> cells)
        {
            Cells = cells;
            // Fill the rest
        }
        public Island(HashSet<Vector2Int> cells)
        {
            Cells = new List<Vector2Int>(cells);
            // Fill the rest
        }

        public Vector2 Mean;
        public List<Vector2Int> Cells;
        public List<Vector2Int> EdgeCells;

        public static Dictionary<Vector2Int, Island> CellToIsland = new Dictionary<Vector2Int, Island>();
    }

    static void reduceIslands(float[,] map)
    {
        //
    }

}
