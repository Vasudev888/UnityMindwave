using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using static System.Net.WebRequestMethods;

// UnityWebRequest.Get example

// Access a website and use UnityWebRequest.Get to download a page.
// Also try to download a non-existing page. Display the error.

public class WebTest : MonoBehaviour
{
    string url = "http://localhost:81/sqlconnect/webtest.php";
    void Start()
    {
        // A correct website page.
        StartCoroutine(GetRequest(url));
        Debug.Log(Application.persistentDataPath);

        // A non-existing page.
        //StartCoroutine(GetRequest("https://error.html"));
    }

    IEnumerator GetRequest(string uri)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(url);

        // Request and wait for the desired page.
        yield return webRequest.SendWebRequest();
        Debug.Log(". " + webRequest.downloadHandler.text);
        string[] webresults = webRequest.downloadHandler.text.Split('\t');
        Debug.Log(webresults[0]);
        int webno = int.Parse(webresults[1]);
        webno *= 2;
        Debug.Log("ZZZ" + webno);
        foreach (string s in webresults)
        {
            Debug.Log(s);
        }
    }
}


/*string[] pages = uri.Split('/');
int page = pages.Length - 1;

switch (webRequest.result)
{
    case UnityWebRequest.Result.ConnectionError:
    case UnityWebRequest.Result.DataProcessingError:
        Debug.LogError(pages[page] + ": Error: " + webRequest.error);
        break;
    case UnityWebRequest.Result.ProtocolError:
        Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
        break;
    case UnityWebRequest.Result.Success:
        Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
        break;
}*/