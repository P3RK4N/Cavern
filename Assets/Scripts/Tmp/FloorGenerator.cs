using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

#if UNITY_EDITOR
using UnityEditor;

#endif

public class FloorGenerator : MonoBehaviour
{
    [SerializeField]
    int f_Seed = 0;
    [SerializeField]
    Noise.PerlinSettings f_PerlinSettings;
    [SerializeField]
    float f_PerlinTreshold;
    [SerializeField]
    float f_FloorAmplitude = 1.0f;
    [SerializeField]
    GameObject cube;
    [SerializeField]
    Material m_CaveWallMat;
    [SerializeField]
    Material m_CaveFloorMat;

    Transform r_TF;
    NavMeshSurface r_NMS;

    GameObject m_CaveTop;
    GameObject m_CaveWall;
    GameObject m_CaveFloor;

    void Awake()
    {
        r_NMS = GetComponentInParent<NavMeshSurface>();
        r_TF = transform;

        createCaveObjects();
    }

    void Start()
    {
        createCaveWall();
        createCaveFloor();
        r_NMS.BuildNavMesh();
    }

    public void createCaveObjects()
    {
        deleteCaveObjects();

        {
            m_CaveTop = new GameObject("CaveTop");
            m_CaveTop.transform.parent = r_TF;
            m_CaveTop.transform.position = new Vector3(0.0f, 3.0f, 0.0f);

            var topFilter = m_CaveTop.AddComponent<MeshFilter>();
            var topRenderer = m_CaveTop.AddComponent<MeshRenderer>();
            topRenderer.material = m_CaveWallMat;
        }

        {
            m_CaveWall = new GameObject("CaveWall");
            m_CaveWall.transform.parent = r_TF;
            m_CaveWall.transform.position = new Vector3(0.0f, 3.0f, 0.0f);

            // m_CaveWall.layer = Layer.s_Instance.m_ObstacleLayer;
            m_CaveWall.layer = LayerMask.NameToLayer("Obstacle");
            
            var wallFilter = m_CaveWall.AddComponent<MeshFilter>();
            var wallRenderer = m_CaveWall.AddComponent<MeshRenderer>();
            var meshCollider = m_CaveWall.AddComponent<MeshCollider>();

            wallRenderer.material = m_CaveWallMat;
        }

        {
            m_CaveFloor = new GameObject("CaveFloor");
            m_CaveFloor.transform.parent = r_TF;

            var floorFilter = m_CaveFloor.AddComponent<MeshFilter>();
            var floorRenderer = m_CaveFloor.AddComponent<MeshRenderer>();

            floorRenderer.material = m_CaveFloorMat;
        }
    }

    public void deleteCaveObjects()
    {
        if(m_CaveFloor)
        {
            DestroyImmediate(m_CaveFloor);
            m_CaveFloor = null;
        }
        if(m_CaveWall)
        {
            DestroyImmediate(m_CaveWall);
            m_CaveWall = null;
        }
        if(m_CaveTop)
        {
            DestroyImmediate(m_CaveTop);
            m_CaveTop = null;
        }
    }

    public void createCaveFloor()
    {
        if(f_Seed != 0) Random.InitState(f_Seed);

        Mesh terrain = TerrainGenerator.s_CreateNoisyTerrain(200.0f, 50, f_PerlinSettings, f_FloorAmplitude);

        m_CaveFloor.GetComponent<MeshFilter>().mesh = terrain;
    }

    // void createEnvironment()
    // {
    //     if(r_TW.seed != 0) Random.InitState(r_TW.seed);

    //     float scale = r_TW.transform.localScale.x / 10.0f;
    //     cube.transform.localScale = new Vector3(scale,scale,scale);

    //     float[,] perlinNoise = Noise.perlinNoise(r_TW.textureSize.x, r_TW.textureSize.y, r_TW.perlinSettings);
    //     Filter.binarize(perlinNoise, r_TW.perlinTreshold, 0.0f, 1.0f);
    //     Filter.borderize(perlinNoise, 0.0f);

    //     Vector3 initPos = - new Vector3(0.5f, 0, 0.5f) + new Vector3(r_TW.textureSize.x, 0, r_TW.textureSize.y) / 2;
    //     initPos *= scale;

    //     for(int i = 0; i < perlinNoise.GetLength(0); i++)
    //         for(int j = 0; j < perlinNoise.GetLength(1); j++)
    //             if(perlinNoise[i,j] < 0.01f)
    //                 Instantiate(cube, initPos + new Vector3(-j, 0.5f, -i) * scale, Quaternion.identity, r_TF);
    // }

    public void createCaveWall()
    {
        Mesh caveTop, caveWall;
        SquareMarcher.s_CreateCave
        (
            200.0f, 
            200, 
            (x,y) => 
            {
                if(f_Seed != 0) Random.InitState(f_Seed);
                return Noise.samplePerlinNoise(x, y, f_PerlinSettings) > f_PerlinTreshold ? 0 : 1;
            },
            out caveTop,
            out caveWall
        );

        m_CaveTop.GetComponent<MeshFilter>().mesh = caveTop;
        m_CaveWall.GetComponent<MeshFilter>().mesh = caveWall;
        m_CaveWall.GetComponent<MeshCollider>().sharedMesh = caveWall;
    }
}

#if UNITY_EDITOR

[CustomEditor (typeof (FloorGenerator))]
public class FloorGeneratorEditor : Editor
{
    void refreshCave()
    {
        FloorGenerator fg = (FloorGenerator)target;

        fg.createCaveObjects();

        fg.createCaveFloor();
        fg.createCaveWall();
    }

    public override void OnInspectorGUI() 
    {
        FloorGenerator fg = (FloorGenerator)target;

        if(GUILayout.Button("Refresh Cave")) refreshCave();

        GUILayout.Space(10);

        if(GUILayout.Button("Refresh Wall")) fg.createCaveWall();
        if(GUILayout.Button("Refresh Floor")) fg.createCaveFloor();

        GUILayout.Space(10);

        if(GUILayout.Button("Delete Cave")) fg.deleteCaveObjects();
        if(GUILayout.Button("Create Cave"))
        { 
            fg.deleteCaveObjects();
            fg.createCaveObjects();
        }

        GUILayout.Space(20);

        GUILayout.Label("Settings");
        if(DrawDefaultInspector()) 
        {
            refreshCave();
        }
    }

}

#endif