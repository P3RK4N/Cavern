using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorGenerator : MonoBehaviour
{
    public GameObject cube;

    Transform walls;

    TextureViewer tw;

    void Awake()
    {
        tw = GetComponent<TextureViewer>();
        walls = new GameObject("Walls").transform;
        walls.parent = transform;
    }

    void Start()
    {
        createEnvironment();
    }

    void createEnvironment()
    {
        if(tw.seed != 0) Random.InitState(tw.seed);

        float scale = tw.transform.localScale.x / 10.0f;
        cube.transform.localScale = new Vector3(scale,scale,scale);

        float[,] perlinNoise = Noise.perlinNoise(tw.textureSize.x, tw.textureSize.y, tw.perlinSettings);
        Filter.binarize(perlinNoise, tw.perlinTreshold, 0.0f, 1.0f);
        Filter.borderize(perlinNoise, 0.0f);

        Vector3 initPos = - new Vector3(0.5f, 0, 0.5f) + new Vector3(tw.textureSize.x, 0, tw.textureSize.y) / 2;
        initPos *= scale;

        for(int i = 0; i < perlinNoise.GetLength(0); i++)
            for(int j = 0; j < perlinNoise.GetLength(1); j++)
                if(perlinNoise[i,j] < 0.01f)
                    Instantiate(cube, initPos + new Vector3(-j, 0.5f, -i) * scale, Quaternion.identity, walls);
    }
}
