using UnityEngine;

public class ImageGridDivider : MonoBehaviour
{
    public RectTransform targetImage;
    public Color lineColor = Color.black;
    public float lineWidth = 2f;

    void Start()
    {
        CreateGridLines();
    }

    void CreateGridLines()
    {
        float width = targetImage.rect.width;
        float height = targetImage.rect.height;
        Vector3[] positions;

        // Horizontal Lines
        for (int i = 1; i <= 2; i++) // Only 2 lines needed for 6 quadrants
        {
            positions = new Vector3[]
            {
                new Vector3(-width / 2, height * (i / 3f) - height / 2, 0),
                new Vector3(width / 2, height * (i / 3f) - height / 2, 0)
            };
            DrawLine(positions);
        }

        // Vertical Lines
        for (int i = 1; i <= 3; i++) // Only 3 lines needed for 6 quadrants
        {
            positions = new Vector3[]
            {
                new Vector3(width * (i / 4f) - width / 2, -height / 2, 0),
                new Vector3(width * (i / 4f) - width / 2, height / 2, 0)
            };
            DrawLine(positions);
        }
    }

    void DrawLine(Vector3[] positions)
    {
        GameObject line = new GameObject("GridLine");
        line.transform.SetParent(targetImage, false);
        LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }
}