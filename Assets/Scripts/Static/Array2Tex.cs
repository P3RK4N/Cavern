using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;
using UnityEngine.Events;

public static class Array2Tex
{
    static Color RNGGrayscaleColor()
    {
        float val = UnityEngine.Random.Range(0.0f, 1.0f);
        return new Color(val,val,val,1.0f);
    }

    static Color GrayscaleColor(float val)
    {
        return new Color(val,val,val,1.0f);
    }

    static Color RNGColor() 
    {
        return new Color(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),1.0f);
    }

    public static Texture2D RandomGrayscaleTex(int width = 100, int height = 100, FilterMode filtering = FilterMode.Point)
    {
        Texture2D tex = new Texture2D
        (
            width, 
            height, 
            TextureFormat.RGB24, 
            1, 
            false
        );
        tex.filterMode = filtering;

        Color[] colors = new Color[width * height];

        for(int i = 0; i < width; i++)
            for(int j = 0; j < height; j++)
                colors[i*height + j] = RNGGrayscaleColor();

        tex.SetPixels(colors);
        tex.Apply();
        return tex;
    }
    
    public static Texture2D RandomTex(int width = 100, int height = 100, FilterMode filtering = FilterMode.Point)
    {
        Texture2D tex = new Texture2D
        (
            width, 
            height, 
            TextureFormat.RGB24, 
            1, 
            false
        );
        tex.filterMode = filtering;

        Color[] colors = new Color[width * height];

        for(int i = 0; i < width; i++)
            for(int j = 0; j < height; j++)
                colors[i*height + j] = RNGColor();

        tex.SetPixels(colors);
        tex.Apply();
        return tex;
    }

    public static Texture2D Tex(float[,] data, FilterMode filtering = FilterMode.Point)
    {
        int width = data.GetLength(0);
        int height = data.GetLength(1);
        
        Texture2D tex = new Texture2D
        (
            width, 
            height, 
            TextureFormat.RGB24, 
            1, 
            false
        );

        tex.filterMode = filtering;

        Color[] colors = new Color[width * height];

        for(int i = 0; i < width; i++)
            for(int j = 0; j < height; j++)
                colors[i*height + j] = GrayscaleColor(data[i,j]);

        tex.SetPixels(colors);
        tex.Apply();
        return tex;
    }

    public static Texture2D Tex(Color[,] data, FilterMode filtering = FilterMode.Point)
    {
        int width = data.GetLength(0);
        int height = data.GetLength(1);
        
        Texture2D tex = new Texture2D
        (
            width, 
            height, 
            TextureFormat.RGB24, 
            1, 
            false
        );

        tex.filterMode = filtering;

        Color[] colors = new Color[width * height];

        for(int i = 0; i < width; i++)
            for(int j = 0; j < height; j++)
                colors[i*height + j] = data[i,j];

        tex.SetPixels(colors);
        tex.Apply();
        return tex;
    }
}
