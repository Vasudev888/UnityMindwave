using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageSender : MonoBehaviour
{
    public Texture2D imagetoEncode;

    public string SendImageAsJson()
    {
        byte[] imageBytes = imagetoEncode.EncodeToPNG();
        string base64Image = Convert.ToBase64String(imageBytes);

        //Create a JSON Object with Base64 Image
        var jsonObject = new { imageData = base64Image };

        //Serialize to Json string
        string jsonString = JsonUtility.ToJson(jsonObject);
        return jsonString;
    }

}
