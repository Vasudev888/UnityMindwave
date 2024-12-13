using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class EEGDataSimulator : MonoBehaviour
{
    private string userId;  // This will store the user ID from userdata
    [SerializeField] private string url = "http://localhost:81/sqlconnect/eegpost.php";

    void Start()
    {
        userId = Registration.Instance.GetUserID();  // Assuming Singleton.Instance.UserId stores the user_id
        //StartSendingEEGData();
    }

    public void StartSendingEEGData()
    {
        StartCoroutine(SendDummyEEGData());
    }

    private IEnumerator SendDummyEEGData()
    {
        while (true)
        {
            int attention = Random.Range(0, 101);
            int meditation = Random.Range(0, 101);
            int blinkStrength = Random.Range(0, 101);
            int sessionTime = Random.Range(0, 300); // Simulate session time in seconds
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Simulate EEG wave data
            float delta = Random.Range(0f, 1f);
            float theta = Random.Range(0f, 1f);
            float lowAlpha = Random.Range(0f, 1f);
            float highAlpha = Random.Range(0f, 1f);
            float lowBeta = Random.Range(0f, 1f);
            float highBeta = Random.Range(0f, 1f);
            float lowGamma = Random.Range(0f, 1f);
            float highGamma = Random.Range(0f, 1f);

            // Simulate gaze coordinates
            float gazeX = Random.Range(0f, 1f);
            float gazeY = Random.Range(0f, 1f);

            WWWForm form = new WWWForm();
            form.AddField("userID", userId);
            form.AddField("timestampp", timestamp);
            form.AddField("attention", attention);
            form.AddField("sessionTime", sessionTime);
            form.AddField("meditation", meditation);
            form.AddField("blinkStrength", blinkStrength);
            form.AddField("delta", delta.ToString());
            form.AddField("theta", theta.ToString());
            form.AddField("lowAlpha", lowAlpha.ToString());
            form.AddField("highAlpha", highAlpha.ToString());
            form.AddField("lowBeta", lowBeta.ToString());
            form.AddField("highBeta", highBeta.ToString());
            form.AddField("lowGamma", lowGamma.ToString());
            form.AddField("highGamma", highGamma.ToString());
            form.AddField("gazeX", gazeX.ToString());
            form.AddField("gazeY", gazeY.ToString());

            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error: " + www.error);
                }
                else
                {
                    Debug.Log("Data sent: " + www.downloadHandler.text);
                    Debug.Log(" User ID Print : " + userId);
                }
            }

            yield return new WaitForSeconds(2);  // Adjust the interval as needed
        }
    }
}
