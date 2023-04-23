using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class TextureViewer : MonoBehaviour
{

[SerializeField]
public int seed;
[SerializeField]
public Vector2Int textureSize;

[Space(10)]

[SerializeField]
public Noise.PerlinSettings perlinSettings;
[SerializeField]
[Range(0.0f, 1.0f)]
public float perlinTreshold;
[SerializeField]
public PerlinOffset perlinOffset;

    public enum PerlinOffset
    {
        XY,
        YZ,
        ZX,
        Manual
    }

    public Texture2D m_Texture{get;set;}

    Material mat;

    void OnEnable()
    {
        mat = GetComponent<MeshRenderer>().material;
        refresh();
    }

    void OnDisable()
    {
        DestroyImmediate(mat);
    }

    public void refresh()
    {
        mat.SetTexture("_BaseMap", m_Texture);
    }
}
