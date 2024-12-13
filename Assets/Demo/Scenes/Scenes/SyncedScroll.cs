using UnityEngine;
using UnityEngine.UI;

public class SyncedScroll : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The RawImage that displays the webpage content.")]
    public RawImage webContentRawImage;

    [Tooltip("The RawImage that displays the heatmap overlay.")]
    public RawImage heatmapRawImage;

    [Header("Scrolling Settings")]
    [Tooltip("How fast to scroll through the content.")]
    public float scrollSpeed = 0.1f;

    // Current vertical offset for both images
    private float verticalOffset = 0f;

    void Update()
    {
        // Example approach: detect mouse scroll wheel input
        // If the UnityWebBrowser plugin provides a scroll offset, you would replace this section
        // with logic to read that offset from the plugin instead.
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollDelta) > 0.0001f)
        {
            // Update vertical offset based on scroll input
            verticalOffset += scrollDelta * scrollSpeed;

            // Clamp the offset if needed (optional, depends on content size)
            // verticalOffset = Mathf.Clamp(verticalOffset, 0f, 1f); // adjust based on content

            // Apply the new offset to the webpage RawImage
            var webUv = webContentRawImage.uvRect;
            webUv.y += scrollDelta * scrollSpeed;
            webContentRawImage.uvRect = webUv;

            // Apply the same offset to the heatmap RawImage
            var heatmapUv = heatmapRawImage.uvRect;
            heatmapUv.y += scrollDelta * scrollSpeed;
            heatmapRawImage.uvRect = heatmapUv;
        }
    }
}
