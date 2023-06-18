using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

#if UNITY_EDITOR
using UnityEditor;

#endif

[System.Serializable]
public struct CaveSpec
{
    [Header("General")]

    [SerializeField]
    public int seed;
    [SerializeField] [Range(1.0f, 1000.0f)]
    public float size;

    [Space(5)]
    [Header("Wall Settings")]

    [SerializeField]
    public Noise.PerlinSettings wallPerlinSettings;
    [SerializeField]
    public Noise.PerlinSettings wallNoiseDisplacementPerlinSettings;
    [SerializeField]
    public float wallNoiseIntensity;
    [SerializeField]
    public float wallNoiseFalloff;
    [SerializeField]
    public Vector3 wallQuadraticDisplacement;
    [SerializeField] [Range(0.0f, 1.0f)]
    public float perlinThreshold;
    [SerializeField]
    public int wallResolution;
    [SerializeField]
    public float wallHeight;
    [SerializeField]
    public int wallVerticalSegments;

    [Space(5)]
    [Header("Floor Settings")]

    [SerializeField]
    public Noise.PerlinSettings floorPerlinSettings;
    [SerializeField]
    public float floorAmplitude;
    [SerializeField]
    public int floorResolution;
    [SerializeField]
    public bool smoothFloor;

    public CaveSpec(bool parameterless)
    {
        seed = 1;
        size = 100;

        wallPerlinSettings = new Noise.PerlinSettings
        (
            Vector3.zero,
            14.34f,
            3,
            0.47f,
            0.79f
        );
        wallNoiseDisplacementPerlinSettings = new Noise.PerlinSettings
        (
            Vector3.zero,
            0.03f,
            3,
            0.47f,
            0.79f
        );
        wallNoiseIntensity = 2.0f;
        wallNoiseFalloff = 0.5f;
        wallQuadraticDisplacement = new Vector3(0.13f, 2.1f, -0.4f);
        perlinThreshold = 0.45f;
        wallResolution = 100;
        wallHeight = 7;
        wallVerticalSegments = 5;

        floorPerlinSettings = new Noise.PerlinSettings
        (
            Vector3.zero,
            2.64f,
            3,
            0.47f,
            0.79f
        );
        floorAmplitude = 1;
        floorResolution = 100;
        smoothFloor = false;
    }
}

public class FloorGenerator : MonoBehaviour
{
    [SerializeField]
    CaveSpec f_CaveSpec;

    [SerializeField]
    public Material f_WallMaterial;
    [SerializeField]
    public Material f_TopMaterial;
    [SerializeField]
    public Material f_FloorMaterial;

    Transform r_TF;
    NavMeshSurface r_NMS;

    GameObject m_CaveTop;
    GameObject m_CaveWall;
    GameObject m_CaveFloor;

    void Awake()
    {
        Debug.Log("Awake Called!");

        r_NMS = GetComponentInParent<NavMeshSurface>();
        r_TF = transform;

        createCave();
    }

    void Start()
    {
        Debug.Log("Start Called!");

        r_NMS.BuildNavMesh();
    }

    public void resetCaveSpecs()
    {
        f_CaveSpec = new CaveSpec(true);
    }

    public void createCave()
    {
        Noise.s_PerlinSeed = f_CaveSpec.seed;

        // Cave wall and cave top gameobject -> generated with marching squares -> SquareMarcher.s_CreateCave
        {
            // Cave top
            if(m_CaveTop) DestroyImmediate(m_CaveTop);
            m_CaveTop = new GameObject("CaveTop");
            m_CaveTop.transform.parent = r_TF;
            m_CaveTop.transform.position = new Vector3(0.0f, -0.5f, 0.0f);

            var topFilter = m_CaveTop.AddComponent<MeshFilter>();
            var topRenderer = m_CaveTop.AddComponent<MeshRenderer>();
            topRenderer.material = f_TopMaterial;

            // Cave wall
            if(m_CaveWall) DestroyImmediate(m_CaveWall);
            m_CaveWall = new GameObject("CaveWall");
            m_CaveWall.transform.parent = r_TF;
            m_CaveWall.transform.position = new Vector3(0.0f, -0.5f, 0.0f);

            // m_CaveWall.layer = Layer.s_Instance.m_ObstacleLayer;
            m_CaveWall.layer = LayerMask.NameToLayer("Obstacle");
            
            var wallFilter = m_CaveWall.AddComponent<MeshFilter>();
            var wallRenderer = m_CaveWall.AddComponent<MeshRenderer>();
            var meshCollider = m_CaveWall.AddComponent<MeshCollider>();

            wallRenderer.material = f_WallMaterial;

            Mesh top, wall;
            SquareMarcher.s_CreateCave(f_CaveSpec, out top, out wall); // TODO: Add simplified mesh collider

            wallFilter.mesh = wall;
            topFilter.mesh = top;
            meshCollider.sharedMesh = wallFilter.mesh;
        }
        // Cave bottom gameobject -> generated with perlin noise only -> TerrrainGenerator.s_CreateNoisyTerrain
        {
            if(m_CaveFloor) DestroyImmediate(m_CaveFloor);
            m_CaveFloor = new GameObject("CaveFloor");
            m_CaveFloor.transform.parent = r_TF;

            var floorFilter = m_CaveFloor.AddComponent<MeshFilter>();
            var floorRenderer = m_CaveFloor.AddComponent<MeshRenderer>();

            floorRenderer.material = f_FloorMaterial;

            Mesh floor  = TerrainGenerator.s_CreateNoisyTerrain(f_CaveSpec.size, f_CaveSpec.floorResolution, f_CaveSpec.floorPerlinSettings, f_CaveSpec.floorAmplitude, !f_CaveSpec.smoothFloor);

            floorFilter.mesh = floor;
        }
    }
}

#if UNITY_EDITOR

[CustomEditor (typeof (FloorGenerator))]
public class FloorGeneratorEditor : Editor
{
    void resetCave()
    {
        FloorGenerator fg = (FloorGenerator)target;

        fg.resetCaveSpecs();
    }

    void refreshCave()
    {
        FloorGenerator fg = (FloorGenerator)target;

        fg.createCave();
    }

    public override void OnInspectorGUI() 
    {
        FloorGenerator fg = (FloorGenerator)target;

        if(GUILayout.Button("Reset Cave")) resetCave();

        if(Application.isPlaying)
        {
            if(GUILayout.Button("Refresh Cave")) refreshCave();
        }

        GUILayout.Space(20);

        GUILayout.Label("Settings");
        if(DrawDefaultInspector() && Application.isPlaying)
        {
            refreshCave();
        }
    }

}

#endif