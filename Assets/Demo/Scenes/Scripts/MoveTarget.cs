using System.Collections;
using UnityEngine;

public class MoveTarget : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private int points = 4; // Number of points across the screen
    [SerializeField] private float movementInterval = 1f; // Time between moves

    private Vector3[] positions;
    private int currentIndex = 0;
    private Coroutine movementCoroutine;

    private void Start()
    {
        InitializePositions();
        if (movementCoroutine == null)
        {
            movementCoroutine = StartCoroutine(MoveToNextPoint());
        }
    }

    // Initialize the four points from top-left to top-right
    private void InitializePositions()
    {
        positions = new Vector3[points];
        float screenWidth = Screen.width;
        float spacing = screenWidth / (points - 1);

        for (int i = 0; i < points; i++)
        {
            float xPosition = spacing * i;
            float yPosition = Screen.height - 50; // Slight offset from top
            positions[i] = Camera.main.ScreenToWorldPoint(new Vector3(xPosition, yPosition, Camera.main.nearClipPlane + 1f));
        }
    }

    // Coroutine to move the target between points
    private IEnumerator MoveToNextPoint()
    {
        while (true)
        {
            transform.position = positions[currentIndex];
            currentIndex = (currentIndex + 1) % positions.Length; // Loop back after reaching the last point
            yield return new WaitForSeconds(movementInterval);
        }
    }
}
