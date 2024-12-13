using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasGridPlacer : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int rows = 4;
    [SerializeField] private int columns = 4;
    [SerializeField] private RectTransform targetPrefab; // Prefab to instantiate at each grid point
    [SerializeField] private RectTransform canvasRect; // Canvas RectTransform to get canvas size
    [SerializeField] private float edgeMargin = 50f; // Margin from edges to avoid truncation

    private List<RectTransform> placedTargets = new List<RectTransform>();

    private void Start()
    {
        PlacePointsOnCanvas();
    }

    //This will place 16 points on Screen 4X4
    private void PlacePointsOnCanvas()
    {
        // Get canvas dimensions, reducing by edgeMargin on each side
        float canvasWidth = canvasRect.rect.width - (2 * edgeMargin);
        float canvasHeight = canvasRect.rect.height - (2 * edgeMargin);

        // Calculate the exact spacing between points to fit them evenly within the safe area
        float cellWidth = canvasWidth / (columns - 1);
        float cellHeight = canvasHeight / (rows - 1);

        // Start positioning slightly inward from the top-left corner to create a safe area
        Vector2 startPosition = new Vector2(-canvasRect.rect.width / 2 + edgeMargin, canvasRect.rect.height / 2 - edgeMargin);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                // Calculate the anchored position for each point within the safe area
                float xPosition = startPosition.x + (col * cellWidth);
                float yPosition = startPosition.y - (row * cellHeight);
                Vector2 pointPosition = new Vector2(xPosition, yPosition);

                // Instantiate target at calculated position
                RectTransform targetInstance = Instantiate(targetPrefab, canvasRect);
                targetInstance.anchoredPosition = pointPosition;
                placedTargets.Add(targetInstance);
            }
        }
    }
}
