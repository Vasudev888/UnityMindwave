using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

public class WebServer : MonoBehaviour
{
    private HttpListener listener;
    private Thread serverThread;

    void Start()
    {
        StartServer();
    }

    void OnApplicationQuit()
    {
        StopServer();
    }

    void StartServer()
    {
        listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:3000/"); // Set the API URL
        listener.Start();
        Debug.Log("Web Server Started on");

        serverThread = new Thread(() =>
        {
            while (listener.IsListening)
            {
                var context = listener.GetContext();
                ProcessRequest(context);
            }
        });
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    void ProcessRequest(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        string responseString = "OK";

        if (request.HttpMethod == "POST")
        {
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                string requestData = reader.ReadToEnd();
                Debug.Log($"Received Data: {requestData}");

                // TODO: Process event data (e.g., save to file, analyze, etc.)
            }
        }

        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        context.Response.ContentLength64 = buffer.Length;
        using (var output = context.Response.OutputStream)
        {
            output.Write(buffer, 0, buffer.Length);
        }
    }

    void StopServer()
    {
        if (listener != null && listener.IsListening)
        {
            listener.Stop();
            listener.Close();
        }
        Debug.Log("Web Server Stopped");
    }
}
