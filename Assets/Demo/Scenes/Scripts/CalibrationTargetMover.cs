using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CalibrationTargetMover : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private Image targetImage; // Reference to the target image
    [SerializeField] private int rows = 4;
    [SerializeField] private int columns = 4;
    [SerializeField] private float movementInterval = 1f; // Time in seconds between each move
    [SerializeField] private Vector2 offset = new Vector2(100, 100); // Offset in pixels between points

    private Vector2[] points;
    private int currentIndex = 0;
    private Coroutine movementCoroutine;

    private void Start()
    {
        InitializePoints();
        if (movementCoroutine == null)
        {
            movementCoroutine = StartCoroutine(MoveObject());
        }
    }

    // Initialize the points on the screen in a grid pattern
    private void InitializePoints()
    {
        points = new Vector2[rows * columns];

        // Calculate starting position from the top-left corner
        RectTransform canvasRectTransform = targetImage.canvas.GetComponent<RectTransform>();
        Vector2 startPosition = new Vector2(offset.x, -offset.y); // Start slightly down from the top-left

        int index = 0;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                float xPosition = startPosition.x + col * offset.x;
                float yPosition = startPosition.y - row * offset.y; // Moving downward by subtracting offset
                points[index] = new Vector2(xPosition, yPosition);
                index++;
            }
        }
    }

    // Coroutine to move the image between points
    private IEnumerator MoveObject()
    {
        RectTransform rectTransform = targetImage.GetComponent<RectTransform>();

        while (true)
        {
            if (targetImage != null)
            {
                rectTransform.anchoredPosition = points[currentIndex];
                currentIndex = (currentIndex + 1) % points.Length; // Loop back to the start after reaching the last point
            }
            yield return new WaitForSeconds(movementInterval);
        }
    }

    // Update movement interval in real-time when values are changed in the editor
    private void OnValidate()
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = StartCoroutine(MoveObject());
        }
    }
}
