using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPClient : MonoBehaviour
{
    private UdpClient udpClient;
    private IPEndPoint serverEndPoint;

    void Start()
    {
        udpClient = new UdpClient();
        serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 65432);

        // Start listening for data from Python
        udpClient.BeginReceive(OnReceive, null);

        // Start sending continuous data
        InvokeRepeating(nameof(SendContinuousData), 0f, 0.5f); // Send every 0.5 seconds
    }

    void SendContinuousData()
    {
        try
        {
            string message = $"Continuous data from Unity: {Time.time}";
            byte[] data = Encoding.UTF8.GetBytes(message);
            udpClient.Send(data, data.Length, serverEndPoint);
            Debug.Log("Sent to Python: " + message);
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending data: " + e.Message);
        }
    }

    void OnReceive(IAsyncResult result)
    {
        try
        {
            byte[] data = udpClient.EndReceive(result, ref serverEndPoint);
            string response = Encoding.UTF8.GetString(data);
            Debug.Log("Received from Python: " + response);

            // Continue listening for responses
            udpClient.BeginReceive(OnReceive, null);
        }
        catch (Exception e)
        {
            Debug.LogError("Error receiving data: " + e.Message);
        }
    }

    void OnDestroy()
    {
        CancelInvoke(nameof(SendContinuousData));

        if (udpClient != null)
        {
            udpClient.Close();
        }
    }
}
