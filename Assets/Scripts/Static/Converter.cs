using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Converter
{
    public static Color toColor(uint hexVal)
    {
        return new Color
        (
            ((hexVal >> 24) & 0xFF) / 255f,
            ((hexVal >> 16) & 0xFF) / 255f,
            ((hexVal >>  8) & 0xFF) / 255f,
            ((hexVal >>  0) & 0xFF) / 255f 
        );
    }
}
