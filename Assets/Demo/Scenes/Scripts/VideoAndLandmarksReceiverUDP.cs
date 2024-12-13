using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/*[Serializable]
public class Landmark
{
    public float x;
    public float y;
    public float z;
}

[Serializable]
public class Payload
{
    public string frame;
    public List<Landmark> landmarks;
}*/

public class VideoAndLandmarksReceiverUDP : MonoBehaviour
{
  /*  public RawImage rawImage; // To display the camera feed
    public GameObject landmarkPrefab; // Prefab for visualizing landmarks
    public Transform landmarksParent; // Parent for instantiated landmark objects
    private List<GameObject> landmarkInstances = new List<GameObject>();

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
                    Debug.Log("Frame complete, processing...");
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
            Payload payload = JsonUtility.FromJson<Payload>(payloadJson);

            // Process the frame
            if (!string.IsNullOrEmpty(payload.frame))
            {
                byte[] imageBytes = Convert.FromBase64String(payload.frame);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageBytes);
                rawImage.texture = texture;
                Debug.Log("Frame updated successfully.");
            }

            // Process the landmarks
            if (payload.landmarks != null && payload.landmarks.Count > 0)
            {
                UpdateLandmarks(payload.landmarks);
                Debug.Log($"Received {payload.landmarks.Count} landmarks.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing payload: {e.Message}");
        }
    }

    private void UpdateLandmarks(List<Landmark> landmarks)
    {
        // Clear old landmarks
        foreach (var instance in landmarkInstances)
        {
            Destroy(instance);
        }
        landmarkInstances.Clear();

        // Instantiate new landmarks
        foreach (var lm in landmarks)
        {
            Vector3 position = new Vector3(lm.x * Screen.width, lm.y * Screen.height, lm.z);
            GameObject landmark = Instantiate(landmarkPrefab, position, Quaternion.identity, landmarksParent);
            landmarkInstances.Add(landmark);
        }
    }

    void OnDestroy()
    {
        if (udpClient != null)
        {
            udpClient.Close();
        }
    }*/
}
