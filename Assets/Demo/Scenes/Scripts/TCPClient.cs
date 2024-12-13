using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class TCPClient : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;

    void Start()
    {
        ConnectToServer();
    }

    void Update()
    {
        // Example: Send a message to Python when the spacebar is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SendMessageToServer("Hello from Unity!");
        }

        // Listen for incoming messages
        if (stream != null && stream.DataAvailable)
        {
            ReceiveMessageFromServer();
        }
    }

    void ConnectToServer()
    {
        try
        {
            client = new TcpClient("127.0.0.1", 65432);
            stream = client.GetStream();
            Debug.Log("Connected to server.");
        }
        catch (Exception e)
        {
            Debug.LogError("Error connecting to server: " + e.Message);
        }
    }

    void SendMessageToServer(string message)
    {
        try
        {
            if (stream != null && stream.CanWrite)
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                Debug.Log("Sent to server: " + message);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending message: " + e.Message);
        }
    }

    void ReceiveMessageFromServer()
    {
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Debug.Log("Received from server: " + response);
        }
        catch (Exception e)
        {
            Debug.LogError("Error receiving message: " + e.Message);
        }
    }

    void OnDestroy()
    {
        if (stream != null)
        {
            stream.Close();
        }
        if (client != null)
        {
            client.Close();
        }
    }
}
