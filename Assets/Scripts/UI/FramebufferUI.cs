using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FramebufferUI : MonoBehaviour
{
    public float f_ResolutionFactor = 0.4f;

    public RawImage r_RI;
    public RenderTexture r_RT;

    int m_Width;
    int m_Height;

    void Awake()
    {
        r_RI = GetComponent<RawImage>();
        r_RT = new RenderTexture(1, 1, 24);

        resize();
    }

    void Update()
    {
        if(m_Width != Screen.width || m_Height != Screen.height) resize();
    }

    void resize()
    {
        m_Width = Screen.width;
        m_Height = Screen.height;

        r_RT.Release();
        r_RT.width = (int)(m_Width * f_ResolutionFactor);
        r_RT.height = (int)(m_Height * f_ResolutionFactor);
        r_RT.Create();

        Camera.main.ResetAspect();

#if UNITY_EDITOR
        Debug.Log(string.Format("Recreated: {0} {1}", r_RT.width, r_RT.height));        
#endif
    }
}
