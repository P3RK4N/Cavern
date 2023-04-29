using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

public class FloorGenerator : MonoBehaviour
{
    [SerializeField]
    GameObject cube;

    Transform r_Walls;
    NavMeshSurface r_NMS;
    TextureViewer r_TW;

    void Awake()
    {
        r_TW = GetComponent<TextureViewer>();
        r_NMS = GetComponentInParent<NavMeshSurface>();
        r_Walls = new GameObject("Walls").transform;
        r_Walls.parent = transform;
    }

    void Start()
    {
        createEnvironment();
        r_NMS.BuildNavMesh();
    }

    void createEnvironment()
    {
        if(r_TW.seed != 0) Random.InitState(r_TW.seed);

        float scale = r_TW.transform.localScale.x / 10.0f;
        cube.transform.localScale = new Vector3(scale,scale,scale);

        float[,] perlinNoise = Noise.perlinNoise(r_TW.textureSize.x, r_TW.textureSize.y, r_TW.perlinSettings);
        Filter.binarize(perlinNoise, r_TW.perlinTreshold, 0.0f, 1.0f);
        Filter.borderize(perlinNoise, 0.0f);

        Vector3 initPos = - new Vector3(0.5f, 0, 0.5f) + new Vector3(r_TW.textureSize.x, 0, r_TW.textureSize.y) / 2;
        initPos *= scale;

        for(int i = 0; i < perlinNoise.GetLength(0); i++)
            for(int j = 0; j < perlinNoise.GetLength(1); j++)
                if(perlinNoise[i,j] < 0.01f)
                    Instantiate(cube, initPos + new Vector3(-j, 0.5f, -i) * scale, Quaternion.identity, r_Walls);
    }
}
