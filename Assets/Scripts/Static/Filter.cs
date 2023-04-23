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
}
