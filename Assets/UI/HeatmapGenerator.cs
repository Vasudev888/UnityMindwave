using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(RawImage))]
public class HeatmapGenerator : MonoBehaviour
{
    public Material heatmapMaterial;  // Reference to the heatmap shader material
    int hitCount;         // Number of random heatmap points to generate
                          // public Vector2 imageSize = new Vector2(256, 256); // Dimensions of the RawImage
    public List<float> hits = new List<float>();  // Dynamic list to store (x, y, intensity) for each point
    public int maxHitCount = 300;
    [SerializeField] RawImage rawImage;

    void Start()
    {
        // Initialize heatmap when the game starts
        //GenerateRandomHeatmapPoints();  // Optional, for testing purposes
    }


    public void GenerateHeatmapPoint(float x, float y, float intensity)
    {
        // Add new (x, y, intensity) to the list
        hits.Add(x);
        hits.Add(y);
        hits.Add(intensity);

        // Limit the number of points to maxHitCount by removing the oldest ones
        if (hits.Count > maxHitCount * 3)  // Each point has 3 values: x, y, intensity
        {
            hits.RemoveRange(0, 3);  // Remove the oldest point (x, y, intensity)
        }

        Debug.Log("Calling the function GenerateHeatmapPoint");

        // Pass the updated hits array to the shader
        heatmapMaterial.SetInt("_HitCount", hits.Count / 3);  // Update the hitCount based on the actual number of points
        heatmapMaterial.SetFloatArray("_Hits", hits.ToArray());  // Convert list to array and send it to the shader

        // Assign the material to the RawImage
        //rawImage = GetComponent<RawImage>();
        rawImage.material = heatmapMaterial;
    }
}
