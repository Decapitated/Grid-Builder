using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class MVC
{
    private Vector2[] inputQuad;
    private Vector3[] inputPoints;
    private Vector3[] outputPoints;

    // Constructor that takes a quad and points
    public MVC(Vector2[] inputQuad, Vector3[] inputPoints)
    {
        this.inputQuad = inputQuad;
        this.inputPoints = inputPoints;
    }

    // Function to transform the input points to a new quad
    public Vector3[] TransformPoints(Vector2[] newQuad)
    {
        // Initialize outputPoints array
        outputPoints = new Vector3[inputPoints.Length];

        // Loop through each input point
        for (int i = 0; i < inputPoints.Length; i++)
        {
            // Initialize weight sum and coordinate values
            float weightSum = 0;
            float xCoord = 0;
            float yCoord = 0;

            // Loop through each vertex of the input quad
            for (int j = 0; j < inputQuad.Length; j++)
            {
                // Calculate distance between input point and current vertex
                float distance = Vector2.Distance(new(inputPoints[i].x, inputPoints[i].z), inputQuad[j]);

                // Calculate weight for current vertex
                float weight = 1 / distance;
                weightSum += weight;

                // Add weighted coordinate values to xCoord and yCoord
                xCoord += weight * newQuad[j].x;
                yCoord += weight * newQuad[j].y;
            }

            // Divide weighted coordinate values by total weight sum
            xCoord /= weightSum;
            yCoord /= weightSum;

            // Assign transformed coordinate values to outputPoints array
            outputPoints[i] = new(xCoord, inputPoints[i].y, yCoord);
        }

        return outputPoints;
    }
}
