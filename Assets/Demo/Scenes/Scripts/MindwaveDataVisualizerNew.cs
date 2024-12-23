using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEngine.EventSystems;
using System.Linq;

public class MindwaveDataVisualizerNew : MonoBehaviour
{
    private MindwaveController m_Controller;
    private int m_BlinkStrength = 0;
    private bool isSessionActive = false;   // Flag to track if a session is running
    public Image attentionFillImage;
    public TextMeshProUGUI attentionTextField;
    public Image meditationFillImage;
    public TextMeshProUGUI meditationTextField;
    public TextMeshProUGUI blinkStrengthTextField;
    public TextMeshProUGUI deltaTextField;
    public TextMeshProUGUI thetaTextField;
    public TextMeshProUGUI lowAlphaTextField;
    public TextMeshProUGUI highAlphaTextField;
    public TextMeshProUGUI lowBetaTextField;
    public TextMeshProUGUI highBetaTextField;
    public TextMeshProUGUI lowGammaTextField;
    public TextMeshProUGUI highGammaTextField;

    [SerializeField] private GridCalibrationUDP gridCalibrationUDP;
    private List<EEGData> eegDataList = new List<EEGData>();   // List to store all EEG data
    Vector2 gazePosition;

    [SerializeField] TimeCounter timeCounter;

    [SerializeField] private RectTransform canvasRectTransform;  // Reference to the canvas RectTransform
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private GameObject dottedLinePrefab;  // The dotted line prefab to instantiate between the dots
    [SerializeField] private float dotSpacing = 0.1f;

    private string latestCSVFilePath;

    [SerializeField] private GameObject tooltipPrefab;  // Reference to a tooltip UI element
    private GameObject tooltipInstance;

    private float lastAttentionValue;
    private Coroutine tooltipCoroutine;
    [SerializeField] private TextMeshProUGUI highestAttentionTextField;
    [SerializeField] private TextMeshProUGUI highestAttentionValuePoint;
    private int highestAttentionDotIndex = -1;
    private string userId;  // This will store the user ID from userdata
    //[SerializeField] Registration registration;


    //Calculation for EngagementScore
    float weightAttentionC = 0.7f, weightMeditationC = 0.3f;
    [SerializeField] TextMeshProUGUI engagementScoreTextField;
    //Average vlaue for Engagement score
    float totalAttention = 0f;
    float totalMeditation = 0f;


    public float CalculateEngagementScore(float attention, float meditation)
    {
        // Check if the conditions are met
        if (attention > 20 && meditation > 20)
        {
            // Calculate the Engagement Score
            return (weightAttentionC * attention) + (weightMeditationC * meditation);
        }
        else
        {
            // If conditions are not met, return 0
            return 0;
        }
    }


    // Structure to hold EEG data for CSV
    public struct EEGData
    {
        public float attentionValue;
        public float meditationValue;
        public int blinkStrength;
        public float delta, theta, lowAlpha, highAlpha, lowBeta, highBeta, lowGamma, highGamma;
        public Vector2 gazePosition;  // 
        public DateTime timestamp;
        public string sessionTime;

        public EEGData(float attention, string sessionTime, float meditation, int blinkStrength, Vector2 gazePos, MindwaveDataModel data, DateTime timestamp)
        {
            this.attentionValue = attention;
            this.meditationValue = meditation;
            this.blinkStrength = blinkStrength;
            this.delta = data.eegPower.delta;
            this.theta = data.eegPower.theta;
            this.lowAlpha = data.eegPower.lowAlpha;
            this.highAlpha = data.eegPower.highAlpha;
            this.lowBeta = data.eegPower.lowBeta;
            this.highBeta = data.eegPower.highBeta;
            this.lowGamma = data.eegPower.lowGamma;
            this.highGamma = data.eegPower.highGamma;
            this.timestamp = timestamp;
            this.gazePosition = gazePos;
            this.sessionTime = sessionTime;
        }




        // Convert to CSV format
        public string ToCSV()
        {
            return $"{timestamp},{attentionValue}, {sessionTime},{meditationValue},{blinkStrength},{delta},{theta},{lowAlpha},{highAlpha},{lowBeta},{highBeta},{lowGamma},{highGamma},{gazePosition.x},{gazePosition.y}";
        }
    }

    private void Awake()
    {
        m_Controller = MindwaveManager.Instance.Controller;
    }

    private void Start()
    {
        //userID = Registration.Instance.GetUserID();
        //PlaceDotsOnScreen(eegDataList);
        MindwaveManager.Instance.Controller.OnUpdateMindwaveData += OnMindwaveDataUpdated;
        m_Controller.OnUpdateBlink += OnBlinkUpdated;
        userId = Registration.Instance.GetUserID();  // Assuming Singleton.Instance.UserId stores the user_id
    }

    private void OnDestroy()
    {
        m_Controller.OnUpdateBlink -= OnBlinkUpdated;
        if (MindwaveManager.Instance != null)
        {
            MindwaveManager.Instance.Controller.OnUpdateMindwaveData -= OnMindwaveDataUpdated;
        }
    }

    private void DisplayHighestAttention()
    {
        if (eegDataList.Count == 0)
        {
            Debug.LogWarning("No EEG data available to find highest attention.");
            return;
        }

        // Find the EEGData entry with the highest attention value
        float maxAttention = float.MinValue;
        string maxAttentionTime = string.Empty;

        foreach (var data in eegDataList)
        {
            if (data.attentionValue > maxAttention)
            {
                maxAttention = data.attentionValue;
                maxAttentionTime = data.sessionTime;
            }
        }

        // Display the result in the TextMeshProUGUI field
        if (highestAttentionTextField != null)
        {
            highestAttentionTextField.text = $" {maxAttention}\n {maxAttentionTime}";
        }
    }


    private void OnBlinkUpdated(int _BlinkStrength)
    {
        m_BlinkStrength = _BlinkStrength;
        if (blinkStrengthTextField != null)
        {
            blinkStrengthTextField.text = m_BlinkStrength.ToString();
        }
    }

    private void OnMindwaveDataUpdated(MindwaveDataModel data)
    {
        //gazePosition = eyeDetection.GetCurrentGazePosition();
        //gazePosition = socketClient.GetLatestScreenPosition();
        gazePosition = gridCalibrationUDP.GetLatestScreenPosition();
        float attentionValue = data.eSense.attention;
        lastAttentionValue = attentionValue;
        float meditationValue = data.eSense.meditation;
        DateTime currentTime = DateTime.Now;
        string sessionTime = timeCounter.GetElapsedTime();

        if (isSessionActive)
        {
            // Capture and store EEG data for the session
            eegDataList.Add(new EEGData(attentionValue, sessionTime, meditationValue, m_BlinkStrength, gazePosition, data, currentTime));
        }

        // calculate the engagement score using these values



        // Optionally update UI
        if (attentionFillImage != null)
        {
            float normalizedAttention = attentionValue / 100f;
            attentionFillImage.fillAmount = normalizedAttention;
        }

        if (attentionTextField != null)
        {
            attentionTextField.text = attentionValue.ToString();
        }

        if (meditationFillImage != null)
        {
            float normalizedMeditation = meditationValue / 100f;
            meditationFillImage.fillAmount = normalizedMeditation;
        }

        if (meditationTextField != null)
        {
            meditationTextField.text = meditationValue.ToString();
        }

        UpdateEEGTextFields(data);

    }

    private void UpdateEEGTextFields(MindwaveDataModel data)
    {
        if (deltaTextField != null) deltaTextField.text = data.eegPower.delta.ToString();
        if (thetaTextField != null) thetaTextField.text = data.eegPower.theta.ToString();
        if (lowAlphaTextField != null) lowAlphaTextField.text = data.eegPower.lowAlpha.ToString();
        if (highAlphaTextField != null) highAlphaTextField.text = data.eegPower.highAlpha.ToString();
        if (lowBetaTextField != null) lowBetaTextField.text = data.eegPower.lowBeta.ToString();
        if (highBetaTextField != null) highBetaTextField.text = data.eegPower.highBeta.ToString();
        if (lowGammaTextField != null) lowGammaTextField.text = data.eegPower.lowGamma.ToString();
        if (highGammaTextField != null) highGammaTextField.text = data.eegPower.highGamma.ToString();
    }

    // Start capturing EEG data
    public void StartSession()
    {
        isSessionActive = true;
        eegDataList.Clear();  // Clear previous session data
    }

    // Stop capturing EEG data and save to CSV
    public void EndSession()
    {
        isSessionActive = false;
        SaveToCSV();  // Save the captured EEG data to a CSV file
        DisplayHighestAttention();  // Show the highest attention value and time
    }

    public float GetAttentionValue()
    {
        return lastAttentionValue;  // Assuming 'lastAttentionValue' is tracking the most recent attention value
    }

    // Save the captured EEG data to a CSV file
    private void SaveToCSV()
    {
        int count = eegDataList.Count;
        string filePath = Path.Combine(Application.persistentDataPath, "EEGData_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");
        StringBuilder csvContent = new StringBuilder();

        // Add CSV headers
        //csvContent.AppendLine("User Details");
        csvContent.AppendLine("Username,Company Name,Age");
        csvContent.AppendLine($"{UserData.Instance.Username},{UserData.Instance.CompanyName},{UserData.Instance.Age}");
        csvContent.AppendLine("Timestamp,Attention,SessionTime,Meditation,BlinkStrength,Delta,Theta,LowAlpha,HighAlpha,LowBeta,HighBeta,LowGamma,HighGamma,GazeX,GazeY");

        // Add EEG data
        foreach (EEGData data in eegDataList)
        {
            csvContent.AppendLine(data.ToCSV());
            totalAttention += data.attentionValue;       // Assuming EEGData has a .Attention property
            totalMeditation += data.meditationValue;     // And a .Meditation property

        }

        if (count > 0)
        {

            float avgAttention = totalAttention / count;
            float avgMeditation = totalMeditation / count;
            float engagementScore = CalculateEngagementScore(avgAttention, avgMeditation);
            engagementScoreTextField.text = engagementScore.ToString();
        }
        else
        {
            engagementScoreTextField.text = "No Data";
        }
        PlaceDotsOnScreen(eegDataList);

        // Write the CSV file
        File.WriteAllText(filePath, csvContent.ToString());

        latestCSVFilePath = filePath;

        Debug.Log("EEG data saved to: " + filePath);
    }


    public void PlaceDotsOnScreen(List<EEGData> eegDataList)
    {
        List<GameObject> dotList = new List<GameObject>();  // To store instantiated dots for later use
        List<EEGData> sortedList = eegDataList.OrderByDescending(e => e.attentionValue).ToList();

/*        float highestAttention = -1f;
        float secondHighestAttention = -1f;
        //int highestAttentionDotIndex = -1;
        int secondHighestAttentionDotIndex = -1;
*/

        // Take only the top 4 entries (or fewer if less than 4 available)
        int count = Mathf.Min(sortedList.Count, 4);

        if (count == 0)
        {
            Debug.LogWarning("No EEG data to plot.");
            return;
        }

        // First pass: Find highest and second-highest attention values
/*        for (int i = 0; i < eegDataList.Count; i++)
        {
            float attentionValue = eegDataList[i].attentionValue;
            if (attentionValue > highestAttention)
            {
                secondHighestAttention = highestAttention;
                secondHighestAttentionDotIndex = highestAttentionDotIndex;

                highestAttention = attentionValue;
                highestAttentionDotIndex = i;
            }
            else if (attentionValue > secondHighestAttention)
            {
                secondHighestAttention = attentionValue;
                secondHighestAttentionDotIndex = i;
            }
        }*/

        // Second pass: Place dots and set colors based on attention values
        for (int i = 0; i < count; i++)
        {
            EEGData eeg = sortedList[i];

            // Map the gaze position to canvas space
            Vector2 screenPosition = MapToScreenSpace(eeg.gazePosition);

            // Instantiate the dot prefab and set its position
            GameObject dot = Instantiate(dotPrefab, canvasRectTransform);
            RectTransform dotRectTransform = dot.GetComponent<RectTransform>();
            dotRectTransform.anchoredPosition = screenPosition;
            dotList.Add(dot);

            // Set color based on attention value ranking
            Image dotImage = dot.GetComponent<Image>();
            if (dotImage != null)
            {
                if (i == 0)
                    dotImage.color = Color.red;           // Highest attention dot is red
                else if (i == 1)
                    dotImage.color = new Color(1f, 0.5f, 0f);  // Second highest is orange
                else
                    dotImage.color = Color.green;         // Remaining dots are green
            }

            // Tooltip and label for dot order
            AddTooltipToDot(dot, eeg.attentionValue, eeg.sessionTime.ToString(), eeg.meditationValue);
            TextMeshProUGUI orderText = dot.GetComponentInChildren<TextMeshProUGUI>();
            if (orderText != null)
            {
                orderText.text = (i + 1).ToString();  // Display the order
            }

            // Optionally scale the dot to a smaller size if needed
            dotRectTransform.sizeDelta = new Vector2(70f, 70f);
        }

        // Display the highest attention dot index if UI element is available
        if (highestAttentionValuePoint != null)
        {
            highestAttentionValuePoint.text = "  1";  // Display 1-based index
        }

        // Connect the dots with dotted lines
        for (int i = 0; i < dotList.Count - 1; i++)
        {
            GameObject startDot = dotList[i];
            GameObject endDot = dotList[i + 1];
            ConnectDotsWithDottedLine(startDot.transform.position, endDot.transform.position);
        }
    }

    private void ConnectDotsWithDottedLine(Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        int numberOfDots = Mathf.FloorToInt(distance / dotSpacing);

        for (int j = 0; j <= numberOfDots; j++)
        {
            float t = numberOfDots > 0 ? (float)j / numberOfDots : 0f;
            Vector3 dotWorldPosition = Vector3.Lerp(start, end, t);

            // Convert from world/screen position to the canvas' local position
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                new Vector2(dotWorldPosition.x, dotWorldPosition.y),
                null,
                out localPoint
            );

            // Instantiate the dotted line element
            GameObject dottedLine = Instantiate(dottedLinePrefab, canvasRectTransform);
            RectTransform rectTransform = dottedLine.GetComponent<RectTransform>();

            // Ensure pivot is at center (0.5, 0.5)
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            // Set the anchored position so the dot is placed at the calculated local position
            rectTransform.anchoredPosition = localPoint;

            // Keep the dots uniformly sized to avoid distortion
            rectTransform.sizeDelta = new Vector2(5f, 5f);  // Adjust if necessary
        }
    }






    private void AddTooltipToDot(GameObject dot, float attention, string time, float meditation)
    {
        EventTrigger trigger = dot.AddComponent<EventTrigger>();

        // Add PointerEnter event to show tooltip
        EventTrigger.Entry pointerEnter = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        pointerEnter.callback.AddListener((eventData) => StartTooltipCoroutine(attention, time, meditation));
        trigger.triggers.Add(pointerEnter);

        // Add PointerExit event to hide tooltip
        EventTrigger.Entry pointerExit = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        pointerExit.callback.AddListener((eventData) => HideTooltip());
        trigger.triggers.Add(pointerExit);
    }

    private void StartTooltipCoroutine(float attention, string time, float meditation)
    {
        if (tooltipCoroutine != null)
        {
            StopCoroutine(tooltipCoroutine);
        }
        tooltipCoroutine = StartCoroutine(ShowTooltipWithDelay(attention, time, meditation));
    }

    private IEnumerator ShowTooltipWithDelay(float attention, string time, float meditation)
    {
        yield return new WaitForSeconds(0.2f);  // Add a small delay before showing the tooltip

        ShowTooltip(attention, time, meditation);
    }

    // Show the tooltip with attention, time, and meditation data
    private void ShowTooltip(float attention, string time, float meditation)
    {
        if (tooltipPrefab != null)
        {
            if (tooltipInstance == null)
            {
                tooltipInstance = Instantiate(tooltipPrefab, canvasRectTransform);  // Create tooltip
            }
            tooltipInstance.SetActive(true);

            // Set tooltip text
            TextMeshProUGUI tooltipText = tooltipInstance.GetComponentInChildren<TextMeshProUGUI>();
            if (tooltipText != null)
            {
                tooltipText.text = $"Time: {time}\nAttention: {attention}\nMeditation: {meditation}";
            }

            // Position the tooltip near the mouse position
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                Input.mousePosition,
                null,
                out localPoint
            );
            Vector2 tooltipOffset = new Vector2(30f, -30f);
            tooltipInstance.GetComponent<RectTransform>().anchoredPosition = localPoint + tooltipOffset;
        }
    }

    // Hide the tooltip when mouse leaves the dot

    private void HideTooltip()
    {
        if (tooltipInstance != null)
        {
            tooltipInstance.SetActive(false);
        }

        if (tooltipCoroutine != null)
        {
            StopCoroutine(tooltipCoroutine);
            tooltipCoroutine = null;
        }
    }


    private Vector2 MapToScreenSpace(Vector2 gazePos)
    {
        // Assuming the gazePos is in the same scale as the screen resolution
        float mappedX = (gazePos.x / 1920f) * canvasRectTransform.sizeDelta.x;  // Assuming the webcam resolution is 1280x720
        float mappedY = -(gazePos.y / 1080f) * canvasRectTransform.sizeDelta.y;

        return new Vector2(mappedX, mappedY);
    }

    public string GetLatestCSVFilePath()
    {
        return latestCSVFilePath;
    }


    //Sending to backend PHP

    public void PostEEGData()
    {
        StartCoroutine(SendEEGData());
    }

    IEnumerator SendEEGData()
    {
        foreach (EEGData data in eegDataList)
        {
            WWWForm form = new WWWForm();
            form.AddField("userID", userId);
            form.AddField("timestampp", data.timestamp.ToString("yyyy-MM-dd HH:mm:ss")); // Format as needed
            form.AddField("attention", data.attentionValue.ToString());
            form.AddField("sessionTime", data.sessionTime);
            form.AddField("meditation", data.meditationValue.ToString());
            form.AddField("blinkStrength", data.blinkStrength);
            form.AddField("delta", data.delta.ToString());
            form.AddField("theta", data.theta.ToString());
            form.AddField("lowAlpha", data.lowAlpha.ToString());
            form.AddField("highAlpha", data.highAlpha.ToString());
            form.AddField("lowBeta", data.lowBeta.ToString());
            form.AddField("highBeta", data.highBeta.ToString());
            form.AddField("lowGamma", data.lowGamma.ToString());
            form.AddField("highGamma", data.highGamma.ToString());
            form.AddField("gazeX", data.gazePosition.x.ToString());
            form.AddField("gazeY", data.gazePosition.y.ToString());

            UnityWebRequest request = UnityWebRequest.Post("http://localhost:81/sqlconnect/eegpost.php", form);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Failed to send EEG data: " + request.error);
            }
            else
            {
                Debug.Log("EEG data successfully sent: " + request.downloadHandler.text);
            }
        }
    }


}
