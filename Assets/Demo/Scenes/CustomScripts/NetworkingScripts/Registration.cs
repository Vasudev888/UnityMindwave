using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using XCharts.Runtime;

public class Registration : MonoBehaviour
{
    [SerializeField] TMP_InputField username;
    [SerializeField] TMP_InputField age;
    [SerializeField] TMP_InputField companyname;
    //[SerializeField] TMP_InputField email;
    // string url = "http://localhost/sqlconnect/register.php";
    private string userID; // Field to store the UserID

    public static Registration Instance { get; private set; } // Singleton instance

    private void Awake()
    {
        // Check if the instance already exists and ensure it's unique.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy this instance if a Singleton already exists
        }
        else
        {
            Instance = this; // Set this as the Singleton instance
            DontDestroyOnLoad(gameObject); // Persist across scenes
            GenerateUniqueUserID(); // Generate unique ID on registration
        }
    }

    public void CallRegister()
    {
        StartCoroutine(Register());
    }

    /*    IEnumerator Register()
        {
            WWWForm form = new WWWForm();
            form.AddField("userName", username.text);
            form.AddField("age", age.text);
            form.AddField("companyName", companyname.text);
            //form.AddField("email", email.text);


            UnityWebRequest request = UnityWebRequest.Post("http://localhost:81/sqlconnect/register.php", form); 
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Unsuccess" + request.error);
            }
            else
            {
                // Success! Handle the response data
                Debug.Log("Succkess" + request.downloadHandler.text);
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Parse JSON response to get UserID
                string responseText = request.downloadHandler.text;
                var jsonResponse = JsonUtility.FromJson<RegistrationResponse>(responseText);
                userID = jsonResponse.UserID;
                //RegistrationResponse registrationResponse = new RegistrationResponse();
                //registrationResponse.UserID = jsonResponse.UserID;
                Debug.Log("Last inserted UserID is: " + jsonResponse.UserID);
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }

        }*/

    private IEnumerator Register()
    {
        // Prepare form data
        WWWForm form = new WWWForm();
        form.AddField("userID", userID); // Include the generated unique ID
        form.AddField("userName", username.text);
        form.AddField("age", age.text);
        form.AddField("companyName", companyname.text);

        // Send registration request
        UnityWebRequest request = UnityWebRequest.Post("http://localhost:81/sqlconnect/register.php", form);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Request Error: " + request.error);
        }
        else
        {
            Debug.Log("Response received: " + request.downloadHandler.text);

            // Parse JSON response to get UserID
            try
            {
                string responseText = request.downloadHandler.text;
                var jsonResponse = JsonUtility.FromJson<RegistrationResponse>(responseText);
                //userID = jsonResponse.UserID;
                Debug.Log("Last inserted UserID is: " + jsonResponse.UserID);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to parse UserID: " + e.Message);
            }
        }
    }

    private void GenerateUniqueUserID()
    {
        // Generate a unique ID using System.Guid
        userID = Guid.NewGuid().ToString();
        Debug.Log("Generated UserID: " + userID);
    }

    [System.Serializable]
    public class RegistrationResponse
    {
        public int UserID;
        public string error;
       

    }

    public string GetUserID()
    {
        return userID;
    }




}
