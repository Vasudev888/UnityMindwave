using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class WebGLMessageReceiver : MonoBehaviour
{
    void Start()
    {
        Application.ExternalEval(@"
            window.addEventListener('message', function(event) {
                if (event.data.type === 'unityEvent') {
                    SendMessage('WebGLMessageReceiver', 'ReceiveEvent', JSON.stringify(event.data.data));
                }
            });
        ");
    }

    public void ReceiveEvent(string jsonData)
    {
        Debug.Log("Received event from Web: " + jsonData);

        // Parse JSON (optional)
        try
        {
            EventData eventData = JsonUtility.FromJson<EventData>(jsonData);
            Debug.Log($"Event Type: {eventData.eventType}, Details: {eventData.details}, Section: {eventData.section}, Timestamp: {eventData.timestamp}");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to parse event data: " + e.Message);
        }
    }

    [Serializable]
    public class EventData
    {
        public string eventType;
        public string details;
        public string section;
        public string timestamp;
    }
}
