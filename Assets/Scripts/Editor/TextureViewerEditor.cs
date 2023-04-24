#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (TextureViewer))]
public class TextureViewerEditor : Editor
{
    static string[] GenerationType =
    {
        "Random Colored",
        "Random Grayscale",
        "Perlin"
    };

    int genType = 0;

    public override void OnInspectorGUI() 
    {
        TextureViewer texView = (TextureViewer)target;

        genType = EditorPrefs.GetInt("genType" + texView.GetInstanceID(), 0);
        genType = EditorGUILayout.Popup("Generation Type", genType, GenerationType);
        
        if(GUILayout.Button("Refresh")) generateTex();

        GUILayout.Space(20);

        GUILayout.Label("Settings");
        if(DrawDefaultInspector() || texView.transform.hasChanged) 
        {
            texView.transform.hasChanged = false;
            generateTex();
            texView.refresh();
        }

        EditorPrefs.SetInt("genType" + texView.GetInstanceID(), genType);
    }

    void posToPerlinOffset(ref Noise.PerlinSettings ps, TextureViewer.PerlinOffset po)
    {
        TextureViewer texView = (TextureViewer)target;
        //float fac = texView.m_Texture.width;
        Vector3 pos = texView.GetComponent<Transform>().position;
        switch(po)
        {
            case TextureViewer.PerlinOffset.XY:
            {
                ps.offset = new Vector2(pos.y, pos.x);
                break;
            }
            case TextureViewer.PerlinOffset.YZ:
            {
                ps.offset = new Vector2(pos.z, pos.y);
                break;
            }
            case TextureViewer.PerlinOffset.ZX:
            {
                ps.offset = new Vector2(pos.z, pos.x);
                break;
            }
        }
    }

    void generateTex()
    {
        TextureViewer texView = (TextureViewer)target;

        switch (GenerationType[genType])
        {
            case "Random Colored":
            {
                if(texView.seed != 0) Random.InitState(texView.seed);
                texView.m_Texture = Array2Tex.RandomTex();
                texView.refresh();
                break;
            }
            case "Random Grayscale":
            {
                if(texView.seed != 0) Random.InitState(texView.seed);
                texView.m_Texture = Array2Tex.RandomGrayscaleTex();
                texView.refresh();
                break;
            }
            case "Perlin":
            {
                if(texView.seed != 0) Random.InitState(texView.seed);
                posToPerlinOffset(ref texView.perlinSettings, texView.perlinOffset);
                float[,] perlinNoise = Noise.perlinNoise(texView.textureSize.x, texView.textureSize.y, texView.perlinSettings);
                Filter.binarize(perlinNoise, texView.perlinTreshold, 0.0f, 1.0f);
                texView.m_Texture = Array2Tex.Tex(perlinNoise);
                texView.refresh();
                break;
            }
        }
    }
}

#endif