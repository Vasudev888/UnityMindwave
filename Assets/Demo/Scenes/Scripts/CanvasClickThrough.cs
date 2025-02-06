using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class CanvasClickThrough : MonoBehaviour
{
    public Canvas targetCanvas; // Assign the specific canvas in the Inspector
    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;

    private void Start()
    {
        // Get the GraphicRaycaster and EventSystem components
        raycaster = targetCanvas.GetComponent<GraphicRaycaster>();
        eventSystem = EventSystem.current;

        if (raycaster == null)
        {
            Debug.LogError("The target canvas must have a GraphicRaycaster component.");
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Detect left mouse button or touch
        {
            Vector2 screenPosition = Input.mousePosition;
            DetectClickInCanvas(screenPosition);
        }
    }

    private void DetectClickInCanvas(Vector2 screenPosition)
    {
        Debug.Log($"Screen Coordinates: X = {screenPosition.x}, Y = {screenPosition.y}");

        // Set up a PointerEventData for the EventSystem
        PointerEventData pointerEventData = new PointerEventData(eventSystem)
        {
            position = screenPosition
        };

        // Store the results of the raycast
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);

        if (results.Count > 0)
        {
            Debug.Log("Clicked inside the canvas on: " + results[0].gameObject.name);

            // Example: Get the screen coordinates
            Vector2 clickedScreenCoords = screenPosition;

            // Example actions based on clicked object
            GameObject clickedObject = results[0].gameObject;

            if (clickedObject.CompareTag("Button"))
            {
                Debug.Log("Clicked on a button!");
                // Perform button-specific action
            }
        }
        else
        {
            Debug.Log("Clicked inside the canvas but not on any UI element.");
        }
    }
}
