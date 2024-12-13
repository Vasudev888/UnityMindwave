using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GazeDotVisualizer : MonoBehaviour
{
    [SerializeField] private RectTransform canvasRectTransform;  // Reference to the canvas RectTransform
    [SerializeField] private GameObject dotPrefab;               // Prefab for the dot to be placed on the screen
    private string csvFilePath;                 // Path to the CSV file

    private List<Vector2> gazePositions = new List<Vector2>();   // List to store gaze positions
    private MindwaveDataVisualizerNew mindwaveData;

    private void Start()
    {
        // Load CSV data

        
        // Auto-assign mindwaveData if it's not assigned in the Inspector
        if (mindwaveData == null)
        {
            mindwaveData = FindObjectOfType<MindwaveDataVisualizerNew>();
        }

        // Check if mindwaveData was found
        if (mindwaveData == null)
        {
            Debug.LogError("MindwaveDataVisualizerNew component not found in the scene.");
        }
        csvFilePath = mindwaveData.GetLatestCSVFilePath();

        LoadGazeDataFromCSV();

        // Place dots on the screen based on gaze positions
        PlaceDotsOnScreen();

        /*
                Vector2[] gazePositionsz = {
                new Vector2(550, 110),
                new Vector2(250, 330),
                new Vector2(320, 540),
                new Vector2(459, 596)
            };

                foreach (Vector2 gazePos in gazePositionsz)
                {
                    Vector2 screenPosition = MapToScreenSpace(gazePos);
                    GameObject dot = Instantiate(dotPrefab, canvasRectTransform);
                    RectTransform dotRectTransform = dot.GetComponent<RectTransform>();
                    dotRectTransform.anchoredPosition = screenPosition;
                }*/

    }

    // Load the Gaze data from CSV
    private void LoadGazeDataFromCSV()
    {
        csvFilePath = mindwaveData.GetLatestCSVFilePath();
        if (string.IsNullOrEmpty(csvFilePath))
        {
            Debug.LogError("CSV file path is null or empty.");
            return;  // Exit if the file path is not valid
        }

        // Check if the file exists before trying to read it
        if (!File.Exists(csvFilePath))
        {
            Debug.LogError($"CSV file not found at path: {csvFilePath}");
            return;
        }

        string[] lines = File.ReadAllLines(csvFilePath);
       

        // Skip the header and start reading the data
        for (int i = 1; i < lines.Length; i++)  // Start from index 1 to skip header
        {
            string[] data = lines[i].Split(',');
            Debug.Log("++++ LoadGazeDataFromCSV is called ++++ : " + data);
            float gazeX = float.Parse(data[12]);  // Assuming GazeX is at index 12
            float gazeY = float.Parse(data[13]);  // Assuming GazeY is at index 13

            gazePositions.Add(new Vector2(gazeX, gazeY));
        }
       
    }

    // Place dots at the gaze positions on the screen
    public void PlaceDotsOnScreen()
    {
        foreach (Vector2 gazePos in gazePositions)
        {
            // Map the gaze position to canvas space
            Vector2 screenPosition = MapToScreenSpace(gazePos);


            // Instantiate the dot prefab and set its position
            GameObject dot = Instantiate(dotPrefab, canvasRectTransform);
            RectTransform dotRectTransform = dot.GetComponent<RectTransform>();
            dotRectTransform.anchoredPosition = screenPosition;
            Debug.Log("))) PlaceDotsOnScreen (((( " + screenPosition);
        }
    }

    // Map the gaze position to the canvas/screen space
    private Vector2 MapToScreenSpace(Vector2 gazePos)
    {
        // Assuming the gazePos is in the same scale as the screen resolution
        float mappedX = (gazePos.x / 1280f) * canvasRectTransform.sizeDelta.x;  // Assuming the webcam resolution is 1280x720
        float mappedY =  - (gazePos.y / 720f) * canvasRectTransform.sizeDelta.y;

        return new Vector2(mappedX, mappedY);
    }   
}
