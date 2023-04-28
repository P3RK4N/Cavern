using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Filter
{
    public static void binarize(float[,] array, float treshold, float lowVal, float highVal)
    {
        for(int i = 0; i < array.GetLength(0); i++)
            for(int j = 0; j < array.GetLength(1); j++)
                array[i,j] = array[i,j] < treshold ? lowVal : highVal;
    }

    public static void borderize(float[,] array, float lowVal)
    {
        int w = array.GetLength(0);
        int h = array.GetLength(1);

        for(int i = 0; i < w; i++)
        {
            array[i,0] = lowVal;
            array[i,h-1] = lowVal;
        }

        for(int i = 0; i < h; i++)
        {
            array[0,i] = lowVal;
            array[w-1,i] = lowVal;
        }
    }
}
