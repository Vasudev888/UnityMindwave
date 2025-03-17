using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Text;
using Newtonsoft.Json;
using TMPro;

public class TestEEGAPI : MonoBehaviour
{
    // Reference to the UI Image you want to send
    //[SerializeField] private Image image;
    [SerializeField]TextMeshProUGUI responseText;  // Assign this in the Inspector
    string userID = "b4e4daee-4ed8-4f8d-bfa1-5e8c2d0650c9";


    private IEnumerator SendJSONToAPI(string url)
    { 

        // User activity details
        Dictionary<string, object> userActivity = new Dictionary<string, object>
        {
            {"Username", "Vasu"},
            {"Company Name", "LTIMindtree"},
            {"Age", 123},
            {"Activity", "Reading"},
            {"TaskCompletion", "75%"}
        };

        // Convert UI Image to base64 string
        //string base64Image = UIImageToBase64();

        // Create the JSON payload
        Dictionary<string, object> requestData = new Dictionary<string, object>
        {
            //{"image", base64Image},
            //{"eeg_data", eegDataList},
            {"user_activity", userActivity},
            {"userID", userID }
        };

        // Convert to JSON string using Newtonsoft.Json
        string jsonData = JsonConvert.SerializeObject(requestData);
        Debug.Log("Sending JSON with image data");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("JSON Data sent successfully!");
                Debug.Log("Response: " + request.downloadHandler.text);
                string response = request.downloadHandler.text;
                responseText.text = "API Response: " + response;
            }
            else
            {
                Debug.LogError("Error sending JSON: " + request.error);
            }
        }
    }

    
    void Start()
    {
       
        StartCoroutine(SendJSONToAPI("http://127.0.0.1:5000/upload"));
        //userID = Registration.Instance.GetUserID();
    }
}