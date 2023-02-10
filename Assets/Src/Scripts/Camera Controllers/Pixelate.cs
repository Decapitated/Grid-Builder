using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class Pixelate : MonoBehaviour
{
    [SerializeField]
    Camera camera;
    [SerializeField]
    RawImage rawImage;

    RenderTexture renderTexture;

    public int screenHeight = 145;
    int oldHeight = 145;
    float lastUpdate = 0;
    bool needsUpdate = false;

    void Start()
    {
        var ratio = Screen.width / Screen.height;
        renderTexture = new(screenHeight * ratio, screenHeight, 0, RenderTextureFormat.ARGB32);
        renderTexture.filterMode = FilterMode.Point;
        camera.targetTexture = renderTexture;
        rawImage.texture = renderTexture;
    }

    void Reset()
    {
        if(renderTexture != null)
            renderTexture.Release();
        Start();
    }
}
