using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Sword sword;
    [SerializeField]
    private TMP_Text swingSpeed;
    [SerializeField]
    private float updateFrequency = 1f;

    //[SerializeField]
    //private RenderTexture fadeTexture;

    void Start()
    {
        //fadeTexture.width = Screen.width;
        //fadeTexture.height = Screen.height;
    }

    float lastUpdate = float.NegativeInfinity;
    void Update()
    {
        if (lastUpdate == float.NegativeInfinity || (Time.time - lastUpdate) >= 1f * updateFrequency)
        {
            swingSpeed.text = "Swing Speed: " + Math.Round(sword.CurrentVelocity / 1000, 2) + "m/s";
            lastUpdate = Time.time;
        }

        if (Input.GetMouseButton(0))
        {
            //fadeTexture.SetPixel(Input.mousePosition.x, Input.mousePosition.y);
        }
    }
}
