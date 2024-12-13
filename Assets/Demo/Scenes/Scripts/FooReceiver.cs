using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class GazeData
{
    public int x_screen;
    public int y_screen;
}

[Serializable]
public class Payload
{
    public string frame; // Base64-encoded frame
    public GazeData gaze_data; // Gaze data object
}

public class FooReceiver : MonoBehaviour
{
    public RawImage rawImage; // UI element to display the video feed
    public RectTransform gazeIndicator; // Optional: UI element to show gaze position
    public Vector2 screenResolution = new Vector2(1920, 1080); // Expected screen resolution

    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;
    private StringBuilder messageBuilder = new StringBuilder();

    void Start()
    {
        try
        {
            udpClient = new UdpClient(5001);
            remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            Debug.Log("Listening for UDP packets...");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to set up UDP client: {e.Message}");
        }
    }

    void Update()
    {
        while (udpClient.Available > 0)
        {
            try
            {
                byte[] receivedData = udpClient.Receive(ref remoteEndPoint);
                string messagePart = Encoding.UTF8.GetString(receivedData);

                if (messagePart == "<END>")
                {
                    ProcessPayload(messageBuilder.ToString());
                    messageBuilder.Clear();
                }
                else
                {
                    messageBuilder.Append(messagePart);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error processing data: {e.Message}");
            }
        }
    }

    private void ProcessPayload(string payloadJson)
    {
        try
        {
            // Deserialize the JSON payload
            Payload payload = JsonUtility.FromJson<Payload>(payloadJson);

            // Process video frame
            if (!string.IsNullOrEmpty(payload.frame))
            {
                try
                {
                    byte[] imageBytes = Convert.FromBase64String(payload.frame);
                    Texture2D texture = new Texture2D(2, 2);
                    texture.LoadImage(imageBytes);
                    rawImage.texture = texture;
                    rawImage.rectTransform.sizeDelta = new Vector2(texture.width, texture.height);
                    Debug.Log("Frame updated successfully.");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error decoding image: {e.Message}");
                }
            }

            // Process gaze data
            if (payload.gaze_data != null)
            {
                int xScreen = payload.gaze_data.x_screen;
                int yScreen = payload.gaze_data.y_screen;

                Debug.Log($"Received Gaze Data: X={xScreen}, Y={yScreen}");

                // Optionally, update the gaze indicator position
                if (gazeIndicator != null)
                {
                    // Normalize gaze coordinates to Unity UI space
                    Vector2 normalizedPosition = new Vector2(
                        (float)xScreen / screenResolution.x,
                        (float)yScreen / screenResolution.y
                    );

                    // Map normalized coordinates to gaze indicator position
                    gazeIndicator.anchoredPosition = new Vector2(
                        normalizedPosition.x * rawImage.rectTransform.sizeDelta.x,
                        normalizedPosition.y * rawImage.rectTransform.sizeDelta.y
                    );
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing payload: {e.Message}");
        }
    }

    void OnDestroy()
    {
        if (udpClient != null)
        {
            udpClient.Close();
        }
    }
}
