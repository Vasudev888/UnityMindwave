using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class VideoReceiverTCP : MonoBehaviour
{
    public RawImage rawImage;
    private TcpListener tcpListener;
    private TcpClient tcpClient;
    private NetworkStream networkStream;
    private StringBuilder messageBuilder = new StringBuilder();
    private bool isClientConnected = false;

    void Start()
    {
        try
        {
            // Start TCP listener
            tcpListener = new TcpListener(IPAddress.Any, 5000);
            tcpListener.Start();
            Debug.Log("Listening for TCP connections...");
            tcpListener.BeginAcceptTcpClient(OnClientConnected, null);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to set up TCP listener: {e.Message}");
        }
    }

    private void OnClientConnected(IAsyncResult result)
    {
        try
        {
            tcpClient = tcpListener.EndAcceptTcpClient(result);
            networkStream = tcpClient.GetStream();
            isClientConnected = true;
            Debug.Log("Client connected successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error accepting client connection: {e.Message}");
        }
    }

    void Update()
    {
        if (isClientConnected && networkStream != null && networkStream.DataAvailable)
        {
            try
            {
                byte[] buffer = new byte[8192];
                int bytesRead = networkStream.Read(buffer, 0, buffer.Length);
                string messagePart = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                if (messagePart.Contains("<END>"))
                {
                    Debug.Log("Frame complete, processing...");
                    try
                    {
                        string[] parts = messagePart.Split(new[] { "<END>" }, StringSplitOptions.None);

                        // Add remaining part before <END> to the messageBuilder
                        messageBuilder.Append(parts[0]);

                        byte[] imageBytes = Convert.FromBase64String(messageBuilder.ToString());
                        Texture2D texture = new Texture2D(2, 2);
                        texture.LoadImage(imageBytes);
                        rawImage.texture = texture;
                        Debug.Log("Frame updated successfully.");
                        messageBuilder.Clear();

                        // Append any remaining data after <END> (in case of overlapping frames)
                        if (parts.Length > 1)
                        {
                            messageBuilder.Append(parts[1]);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error decoding image: {e.Message}");
                    }
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
        if (tcpClient != null)
        {
            tcpClient.Close();
        }

        if (tcpListener != null)
        {
            tcpListener.Stop();
        }
    }
}
