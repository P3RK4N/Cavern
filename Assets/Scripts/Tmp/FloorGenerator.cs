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
    GameObject cube;

    Transform r_Cave;
    NavMeshSurface r_NMS;
    TextureViewer r_TW;

    GameObject m_CaveTop;
    GameObject m_CaveWall;
    public Material m_CaveMat;

    void Awake()
    {
        r_TW = GetComponent<TextureViewer>();
        r_NMS = GetComponentInParent<NavMeshSurface>();
        r_Cave = new GameObject("Cave").transform;
        r_Cave.parent = transform;
    }

    void Start()
    {
        // createEnvironment();
        createMarchingEnvironment();
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
                    Instantiate(cube, initPos + new Vector3(-j, 0.5f, -i) * scale, Quaternion.identity, r_Cave);
    }

    public void createMarchingEnvironment()
    {
        if(!r_TW) return;

        if(!m_CaveTop)
        {
            m_CaveTop = new GameObject("Cave Top");
            m_CaveWall = new GameObject("Cave Wall");

            m_CaveWall.layer = Layer.s_Instance.m_ObstacleLayer;

            m_CaveTop.transform.parent = r_Cave;
            m_CaveWall.transform.parent = r_Cave;

            m_CaveTop.transform.position = new Vector3(0.0f, 3.0f, 0.0f);
            m_CaveWall.transform.position = new Vector3(0.0f, 3.0f, 0.0f);

            var topFilter = m_CaveTop.AddComponent<MeshFilter>();
            var topRenderer = m_CaveTop.AddComponent<MeshRenderer>();
            var wallFilter = m_CaveWall.AddComponent<MeshFilter>();
            var wallRenderer = m_CaveWall.AddComponent<MeshRenderer>();
            var meshCollider = m_CaveWall.AddComponent<MeshCollider>();

            topRenderer.material = m_CaveMat;
            wallRenderer.material = m_CaveMat;
        }

        Noise.PerlinSettings ps = new Noise.PerlinSettings(r_TW.perlinSettings);
        ps.scale *= r_TW.transform.localScale.x / 10.0f;

        Mesh caveTop, caveWall;
        SquareMarcher.s_CreateCave
        (
            200.0f, 
            200, 
            (x,y) => 
            {
                if(r_TW.seed != 0) Random.InitState(r_TW.seed);
                return Noise.samplePerlinNoise(x, y, ps) > r_TW.perlinTreshold ? 0 : 1;
            },
            out caveTop,
            out caveWall
        );

        caveTop.RecalculateBounds();
        caveTop.RecalculateNormals();
        caveWall.RecalculateBounds();
        caveWall.RecalculateNormals();

        m_CaveTop.GetComponent<MeshFilter>().mesh = caveTop;
        m_CaveWall.GetComponent<MeshFilter>().mesh = caveWall;
        m_CaveWall.GetComponent<MeshCollider>().sharedMesh = caveWall;

        MeshCollider mc = new MeshCollider();
    }
}

#if UNITY_EDITOR

[CustomEditor (typeof (FloorGenerator))]
public class FloorGeneratorEditor : Editor
{
    void generateFloor()
    {
        FloorGenerator fg = (FloorGenerator)target;
        fg.createMarchingEnvironment();
    }

    public override void OnInspectorGUI() 
    {
        FloorGenerator fg = (FloorGenerator)target;

        if(GUILayout.Button("Refresh")) generateFloor();

        GUILayout.Space(20);

        GUILayout.Label("Settings");
        if(DrawDefaultInspector()) 
        {
            generateFloor();
        }
    }

}

#endif