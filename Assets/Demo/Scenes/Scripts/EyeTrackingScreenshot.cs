using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EyeTrackingScreenshot : MonoBehaviour
{
    [SerializeField] RawImage screenshotDisplay;
    [SerializeField] float delay = 0.5f;
    private Vector2 eyeCoordinates;

    void Start()
    {
        // Assume you have the eye coordinates from your eye-tracking system
        eyeCoordinates = new Vector2(Screen.width / 2, Screen.height / 2); // Example coordinates
        StartCoroutine(TakeScreenshotWithDelay());
    }

    IEnumerator TakeScreenshotWithDelay()
    {
        // Wait for the delay
        yield return new WaitForSeconds(delay);

        // Capture the screenshot
        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();

        // Draw a red dot at the eye coordinates
        DrawRedDot(screenshot, eyeCoordinates);

        // Assign the modified screenshot to a RawImage UI element (for display)
        screenshotDisplay.texture = screenshot;
    }

    void DrawRedDot(Texture2D texture, Vector2 position)
    {
        // Make sure the position is within bounds
        if (position.x >= 0 && position.x < texture.width && position.y >= 0 && position.y < texture.height)
        {
            Color red = Color.red;

            // Draw a small red dot (e.g., 5x5 pixels)
            for (int x = -2; x <= 2; x++)
            {
                for (int y = -2; y <= 2; y++)
                {
                    texture.SetPixel((int)position.x + x, (int)position.y + y, red);
                }
            }

            texture.Apply();
        }
        else
        {
            Debug.LogWarning("Eye coordinates are out of bounds.");
        }
    }
}
