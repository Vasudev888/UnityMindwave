/*using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class AttentionScreenshot : MonoBehaviour
{
    public Image screenshotImage;              // UI Image component to display the screenshot
    private bool isCapturing = false;
    private Texture2D screenshotTexture;
    private string screenshotFilePath; // File path to store the screenshot

    void Start()
    {
        // Set the file path for the screenshot (it will override the same file each time)
        // Generate a unique filename using current date and time
        string subFolder = "Screenshots";
        string folderPath = Path.Combine(Application.persistentDataPath, subFolder);

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string fileName = $"Screenshot_{timestamp}.png";

        *//*screenshotFilePath = Path.Combine(Application.persistentDataPath, fileName);*//*
        screenshotFilePath = Path.Combine(folderPath, fileName);
    }

    // Start capturing screenshots every 2 seconds
    public void StartCapturing()
    {
        if (!isCapturing)
        {
            isCapturing = true;
            StartCoroutine(CaptureScreenshotEveryTwoSeconds());
        }
    }

    // Stop capturing screenshots
    public void StopCapturing()
    {
        isCapturing = false;
    }

    private IEnumerator CaptureScreenshotEveryTwoSeconds()
    {
        while (isCapturing)
        {
            yield return new WaitForSeconds(1f);
            yield return CaptureScreenshot();
        }
    }

    private IEnumerator CaptureScreenshot()
    {
        // Wait for the end of the frame to ensure the screenshot captures the correct frame
        yield return new WaitForEndOfFrame();

        // Capture the screenshot as a texture
        screenshotTexture = ScreenCapture.CaptureScreenshotAsTexture();

        // Create a Sprite from the texture
        Sprite screenshotSprite = Sprite.Create(
            screenshotTexture,
            new Rect(0, 0, screenshotTexture.width, screenshotTexture.height),
            new Vector2(0.5f, 0.5f)
        );

        // Assign the sprite to the UI Image component (overrides the previous one)
        screenshotImage.sprite = screenshotSprite;

        // Save to file (overwriting each time)
        byte[] bytes = screenshotTexture.EncodeToPNG();
        File.WriteAllBytes(screenshotFilePath, bytes);

        Debug.Log("Screenshot saved to: " + screenshotFilePath);
    }

    // Load the last saved screenshot and display it (optional)
    public void DisplayLastScreenshot()
    {
        StartCoroutine(LoadScreenshotFromFile(screenshotFilePath));
    }

    private IEnumerator LoadScreenshotFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);

            Sprite screenshotSprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f)
            );

            screenshotImage.sprite = screenshotSprite;

            Debug.Log("Screenshot loaded and displayed in the UI.");
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
        }

        yield return null;
    }
}
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using XCharts.Runtime;

public class AttentionScreenshot : MonoBehaviour
{
    public Image screenshotImage;              // UI Image component to display the screenshot
    private bool isCapturing = false;
    private Texture2D screenshotTexture;
    private string screenshotFilePath; // File path to store the screenshot
    string fileName;

    [Header("Database Settings")]
    string serverUrl = "http://localhost:81/sqlconnect/ssnew.php";
    public string userId; // Default user ID, can be set from elsewhere

    void Start()
    {
        // Set the file path for the screenshot (it will override the same file each time)
        // Generate a unique filename using current date and time
       
        string subFolder = "Screenshots";
        string folderPath = Path.Combine(Application.persistentDataPath, subFolder);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
         fileName = $"Screenshot_{timestamp}.png";
        /*screenshotFilePath = Path.Combine(Application.persistentDataPath, fileName);*/
        screenshotFilePath = Path.Combine(folderPath, fileName);
        userId = Registration.Instance.GetUserID();
    }

    // Start capturing screenshots every 2 seconds
    public void StartCapturing()
    {
        if (!isCapturing)
        {
            isCapturing = true;
            StartCoroutine(CaptureScreenshotEveryTwoSeconds());
        }
    }

    // Stop capturing screenshots
    public void StopCapturing()
    {
        isCapturing = false;
    }

    private IEnumerator CaptureScreenshotEveryTwoSeconds()
    {
        while (isCapturing)
        {
            yield return new WaitForSeconds(1f);
            yield return CaptureScreenshot();
        }
    }

    private IEnumerator CaptureScreenshot()
    {
        // Wait for the end of the frame to ensure the screenshot captures the correct frame
        yield return new WaitForEndOfFrame();

        // Capture the screenshot as a texture
        screenshotTexture = ScreenCapture.CaptureScreenshotAsTexture();

        // Create a Sprite from the texture
        Sprite screenshotSprite = Sprite.Create(
            screenshotTexture,
            new Rect(0, 0, screenshotTexture.width, screenshotTexture.height),
            new Vector2(0.5f, 0.5f)
        );

        // Assign the sprite to the UI Image component (overrides the previous one)
        screenshotImage.sprite = screenshotSprite;

        // Save to file (overwriting each time)
        byte[] bytes = screenshotTexture.EncodeToPNG();
        File.WriteAllBytes(screenshotFilePath, bytes);

        Debug.Log("Screenshot saved to: " + screenshotFilePath);

        // Send to database
        StartCoroutine(UploadScreenshotToDatabase(bytes));
    }

    private IEnumerator UploadScreenshotToDatabase(byte[] screenshotBytes)
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // Create form data
        WWWForm form = new WWWForm();
        form.AddField("userID", userId);
        form.AddField("timestamp", timestamp);
        //form.AddBinaryData("screenshot", screenshotBytes, "screenshot.png", "image/png");
        form.AddBinaryData("screenshot", File.ReadAllBytes(screenshotFilePath), fileName);

        // Debug information
        Debug.Log("Uploading to URL: " + serverUrl);
        Debug.Log("User ID: " + userId);
        Debug.Log("Timestamp: " + timestamp);

        // Send request to PHP script
        UnityWebRequest request = UnityWebRequest.Post(serverUrl, form);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success && request.downloadHandler.text == "0")
        {
            Debug.Log("Screenshot uploaded successfully");
        }
        else
        {
            Debug.LogError("Upload error: " + request.error);
            Debug.LogError("Server response: " + request.downloadHandler.text);
            Debug.LogError("HTTP Status: " + request.responseCode);
        }
    }

    // Load the last saved screenshot and display it (optional)
    public void DisplayLastScreenshot()
    {
        StartCoroutine(LoadScreenshotFromFile(screenshotFilePath));
    }

    private IEnumerator LoadScreenshotFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);

            Sprite screenshotSprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f)
            );

            screenshotImage.sprite = screenshotSprite;

            Debug.Log("Screenshot loaded and displayed in the UI.");
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
        }

        yield return null;
    }
}