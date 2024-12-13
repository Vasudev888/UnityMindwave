using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class VideoReceiverUDP : MonoBehaviour
{
    public RawImage rawImage;
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
                    try
                    {
                        byte[] imageBytes = Convert.FromBase64String(messageBuilder.ToString());
                        Texture2D texture = new Texture2D(2, 2);
                        texture.LoadImage(imageBytes);
                        rawImage.texture = texture;
                        Debug.Log("Frame updated successfully.");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error decoding image: {e.Message}");
                    }
                    messageBuilder.Clear();
                }
                else
                {
                    Debug.Log($"Received message part, length: {messagePart.Length}");
                    messageBuilder.Append(messagePart);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error processing data: {e.Message}");
            }
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
