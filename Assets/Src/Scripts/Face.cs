using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Vector2 = Shapes.Vector2;
public class Face : MonoBehaviour
{
    public GridBuilder gridBuilder;
    public Vector2 id;

    Renderer renderer;

    bool Toggled => gridBuilder.ToggledFaces.Contains(id);

    void Start()
    {
        renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if(ShouldToggle())
            renderer.sharedMaterial.SetFloat("_Active", 1);
        else
            renderer.sharedMaterial.SetFloat("_Active", 0);
    }

    bool ShouldToggle()
    {
        if ((gridBuilder.MouseClosestFace != null && gridBuilder.MouseClosestFace == id) || Toggled)
            return true;
        if (gridBuilder.MouseClosestFace != null)
        {
            if(gridBuilder.dualGraph.IsNeighbor(gridBuilder.MouseClosestFace, id)) return true;
        }
        
        return false;
    }

    // Checks if the button was clicked within the set length of time.
    float clickLength = 0.333f;
    Dictionary<int, float> downStartTimes = new();
    public bool GetMouseButtonClicked(int button)
    {
        float currentTime = Time.fixedTime;
        if (Input.GetMouseButtonDown(button)) downStartTimes[button] = currentTime;
        if (Input.GetMouseButtonUp(button))
        {
            float clickTime = currentTime - downStartTimes[button];
            if (clickTime <= clickLength)
            {
                return true;
            }
        }
        return false;
    }
}
