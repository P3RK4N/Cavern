using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Noise
{   
    [System.Serializable]
    public struct PerlinSettings
    {
        [SerializeField]
        public Vector2 offset;
        [SerializeField]
        public float scale;
        [SerializeField]
        public int octaves;
        [SerializeField]
        public float persistence;
        [SerializeField]
        public float lacunarity;

        public PerlinSettings(PerlinSettings other)
        {
            offset = other.offset;
            scale = other.scale;
            octaves = other.octaves;
            persistence = other.persistence;
            lacunarity = other.lacunarity;
        }
    };

    struct PerlinSamplerState
    {
        public Vector2[] octaveOffsets;
        public float maxHeight;
    }

    public static float samplePerlinNoise(float x, float y, PerlinSettings ps)
    {
        return samplePerlinNoise(x, y, ps.offset, ps.scale, ps.octaves, ps.persistence, ps.lacunarity);
    }

    public static float samplePerlinNoise(float x, float y, Vector2 offset, float scale, int octaves, float persistance, float lacunarity)
    {
        float maxHeight = 0.0f;
        float amplitude = 1.0f;
        float frequency = 1.0f;

        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = Random.Range(-10000.0f, 10000.0f) + offset.x;
            float offsetY = Random.Range(-10000.0f, 10000.0f) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxHeight += amplitude;
            amplitude *= persistance;
        }

        amplitude = 1;
        frequency = 1;
        float noiseHeight = 0;

        for(int i = 0; i < octaves; i++)
        {
            float sampleX = (x + octaveOffsets[i].x) / scale * frequency;
            float sampleY = (y + octaveOffsets[i].y) / scale * frequency;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2.0f - 1.0f;
            noiseHeight += perlinValue * amplitude;

            amplitude *= persistance;
            frequency *= lacunarity;
        }

        return (noiseHeight + 1.0f) / (2.0f * maxHeight / 1.75f);
    }
    
    public static float[,] perlinNoise(int width, int height, PerlinSettings ps)
    {
        return perlinNoise(width, height, ps.offset, ps.scale, ps.octaves, ps.persistence, ps.lacunarity);
    }

    //Perlin noise
    public static float[,] perlinNoise(int width, int height, Vector2 offset, float scale, int octaves, float persistance, float lacunarity)
    {
        float[,] perlinNoise = new float[width,height];

        float maxHeight = 0.0f;
        float amplitude = 1.0f;
        float frequency = 1.0f;

        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = Random.Range(-10000.0f, 10000.0f) + offset.x;
            float offsetY = Random.Range(-10000.0f, 10000.0f) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxHeight += amplitude;
            amplitude *= persistance;
        }

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        for(int x = 0; x < width; x++)
            for(int y = 0; y < height; y++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for(int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;
                    // float sampleX = (x + octaveOffsets[i].x) / scale * frequency;
                    // float sampleY = (y + octaveOffsets[i].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2.0f - 1.0f;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                // perlinNoise[x,y] = noiseHeight;
                // Normalized
                perlinNoise[x,y] = (noiseHeight + 1.0f) / (2.0f * maxHeight / 1.75f);
            }
        
        return perlinNoise;
    }
    
    public static float sampleSphere(Vector3 pos, Vector3 mid)
    {
        return Vector3.Distance(pos, mid);
    }

    public static float sampleSphere(Vector3 pos, List<Vector3> mids)
    {
        return mids.Min((mid) => Vector3.Distance(pos, mid));
    }

    // //Perlin noise with step
    // public static Color[,] generatePerlinNoiseIsland(int width, int height, int seed, Vector2 offset, float scale, int octaves, float persistance, float lacunarity, float limit1, float limit2)
    // {
    //     float[,] perlinNoise = new float[width,height];

    //     //Generate random seed for each octave
    //     System.Random rng = new System.Random(seed == 0 ? Random.Range(1, 2147483647) : seed);
    //     Vector2[] octaveOffsets = new Vector2[octaves];
    //     for (int i = 0; i < octaves; i++)
    //     {
    //         float offsetX = rng.Next(-10000, 10000) + offset.x;
    //         float offsetY = rng.Next(-10000, 10000) + offset.y;
    //         octaveOffsets[i] = new Vector2(offsetX, offsetY);
    //     }

    //     for(int x = 0; x < width; x++)
    //         for(int y = 0; y < height; y++)
    //         {
    //             float amplitude = 1;
    //             float frequency = 1;
    //             float noiseHeight = 0;

    //             float halfWidth = width / 2f;
    //             float halfHeight = height / 2f;

    //             for(int i = 0; i < octaves; i++)
    //             {
    //                 float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
    //                 float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

    //                 float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
    //                 noiseHeight += perlinValue * amplitude;

    //                 amplitude *= persistance;
    //                 frequency *= lacunarity;
    //             }

    //             perlinNoise[x,y] = noiseHeight;
    //         }

    //     float[,] radialMask = Mask.radialMask(width, height);
    //     Color[,] colorMap = new Color[width, height];
    //     float[,] perlinMini = Noise.generatePerlinNoise(width, height, ((int)(rng.Next() * 0.4379)) + 69, new Vector2(0, 0), 9.5f, 3, -0.96f, 1.33f);

    //     for(int i = 0; i < width; i++)
    //         for(int j = 0; j < height; j++)
    //         {
    //             if(perlinNoise[i, j] * radialMask[i, j] < 0.061)
    //             {
    //                 colorMap[i, j] = new Color(0f, 0f, 0f, 0f);
    //                 continue;
    //             }
    //             float tmp = 1f - perlinMini[i, j];
    //             float val = perlinNoise[i, j] * tmp * tmp;
    //             colorMap[i, j] = val < limit1 ? new Color(0.3f, 0.3f, 0f, 0.5f) : val < limit2 ? new Color(0f, 1f, 0f, 0.5f) : new Color(0f, 0.6f, 0.2f, 0.5f);      
    //         }
        
    //     return colorMap;
    // }

    // // Choose point inside each block and assign it area closest to it
    // public static float[,] generateVoronoiNoise(int width, int height, int blockSize, int seed)
    // {
    //     blockSize = Mathf.Max(1, blockSize);

    //     int widthBlock = width/blockSize;
    //     int heightBlock = height/blockSize;

    //     seed = seed == 0 ? Random.Range(1, 2147483647) : seed;
    //     Random.InitState(seed);
    //     System.Random rng = new System.Random(seed);

    //     Vector2Int[,] points = new Vector2Int[widthBlock, heightBlock];
    //     float[,] pointVals = new float[widthBlock, heightBlock];

    //     HashSet<float> usedVals = new HashSet<float>();

    //     for(int x = 0; x < widthBlock; x++)
    //         for(int y = 0; y < heightBlock; y++)
    //         {
    //             points[x, y] = new Vector2Int(rng.Next(0, blockSize) + x * blockSize, rng.Next(0, blockSize) + y * blockSize);

    //             //Make sure all areas are of different value
    //             float newVal = Random.Range(0.1f, 1.0f);
    //             while(usedVals.Contains(newVal)) newVal = Random.Range(0.1f, 1.0f);
    //             usedVals.Add(newVal);

    //             pointVals[x, y] = newVal;
    //         }
        
    //     float[,] noiseMap = new float[width,height];
        
    //     //Find the closest cell point
    //     for (int x = 0; x < width; x++)
    //         for(int y = 0; y < height; y++)
    //         {
    //             Vector2Int currentPos = new Vector2Int(x,y);

    //             int blockX = x / blockSize;
    //             int blockY = y / blockSize;

    //             int left = Mathf.Max(0, blockX - 1);
    //             int right = Mathf.Min(widthBlock - 1, blockX + 1);

    //             int down = Mathf.Max(0, blockY - 1);
    //             int up = Mathf.Min(heightBlock - 1, blockY + 1);
                
    //             float minDist = float.MaxValue;
    //             Vector2Int closestPoint = new Vector2Int();

    //             for(int posX = left; posX <= right; posX++)
    //                 for(int posY = down; posY <= up; posY++)
    //                 {
    //                     Vector2Int point = points[posX, posY];
    //                     float newDistance = (point.x - currentPos.x) * (point.x - currentPos.x) + (point.y - currentPos.y) * (point.y - currentPos.y);
    //                     if(newDistance < minDist)
    //                     {
    //                         minDist = newDistance;
    //                         closestPoint = new Vector2Int(posX, posY);
    //                     }
    //                 }
    //             noiseMap[currentPos.x, currentPos.y] = pointVals[closestPoint.x, closestPoint.y];
    //         }
        
    //     return noiseMap;
    // }

}