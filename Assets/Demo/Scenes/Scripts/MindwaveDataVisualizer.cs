using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

public class MindwaveDataVisualizer : MonoBehaviour
{
    [SerializeField] private GridCalibrationUDP gridCalibrationUDP;
    [SerializeField] private TimeCounter timeCounter;

    [SerializeField] private RectTransform canvasRectTransform;
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private GameObject dottedLinePrefab;
    [SerializeField] private float dotSpacing = 0.1f;
    [SerializeField] private GameObject tooltipPrefab;
    [SerializeField] private TextMeshProUGUI highestAttentionTextField;
    [SerializeField] private TextMeshProUGUI highestAttentionValuePoint; // Might be used later

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

    private MindwaveController m_Controller;
    private int m_BlinkStrength;
    private bool isSessionActive = false;
    private List<EEGData> eegDataList = new List<EEGData>();
    private string latestCSVFilePath;
    private GameObject tooltipInstance;
    private Coroutine tooltipCoroutine;
    private int highestAttentionDotIndex = -1;
    private string userId;
    private float lastAttentionValue;

    private Vector2 gazePosition;

    public struct EEGData
    {
        public float attentionValue;
        public float meditationValue;
        public int blinkStrength;
        public float delta, theta, lowAlpha, highAlpha, lowBeta, highBeta, lowGamma, highGamma;
        public Vector2 gazePosition;
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

        public string ToCSV()
        {
            return $"{timestamp},{attentionValue},{sessionTime},{meditationValue},{blinkStrength},{delta},{theta},{lowAlpha},{highAlpha},{lowBeta},{highBeta},{lowGamma},{highGamma},{gazePosition.x},{gazePosition.y}";
        }
    }

    private void Awake()
    {
        m_Controller = MindwaveManager.Instance.Controller;
    }

    private void Start()
    {
        MindwaveManager.Instance.Controller.OnUpdateMindwaveData += OnMindwaveDataUpdated;
        m_Controller.OnUpdateBlink += OnBlinkUpdated;
        userId = Registration.Instance.GetUserID();
    }

    private void OnDestroy()
    {
        m_Controller.OnUpdateBlink -= OnBlinkUpdated;

        if (MindwaveManager.Instance != null && MindwaveManager.Instance.Controller != null)
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

        var maxAttentionData = eegDataList.OrderByDescending(d => d.attentionValue).First();
        if (highestAttentionTextField != null)
        {
            highestAttentionTextField.text = $" {maxAttentionData.attentionValue}\n {maxAttentionData.sessionTime}";
        }
    }

    private void OnBlinkUpdated(int blinkStrength)
    {
        m_BlinkStrength = blinkStrength;
        blinkStrengthTextField?.SetText(m_BlinkStrength.ToString());
    }

    private void OnMindwaveDataUpdated(MindwaveDataModel data)
    {
        // Get the latest gaze position (modify this if you switch sources)
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

        // Update UI for Attention
        if (attentionFillImage != null)
        {
            attentionFillImage.fillAmount = attentionValue / 100f;
        }

        attentionTextField?.SetText(attentionValue.ToString());

        // Update UI for Meditation
        if (meditationFillImage != null)
        {
            meditationFillImage.fillAmount = meditationValue / 100f;
        }

        meditationTextField?.SetText(meditationValue.ToString());

        // Update EEG Text Fields
        UpdateEEGTextFields(data);
    }

    private void UpdateEEGTextFields(MindwaveDataModel data)
    {
        // Use null-conditional operators to simplify checks
        deltaTextField?.SetText(data.eegPower.delta.ToString());
        thetaTextField?.SetText(data.eegPower.theta.ToString());
        lowAlphaTextField?.SetText(data.eegPower.lowAlpha.ToString());
        highAlphaTextField?.SetText(data.eegPower.highAlpha.ToString());
        lowBetaTextField?.SetText(data.eegPower.lowBeta.ToString());
        highBetaTextField?.SetText(data.eegPower.highBeta.ToString());
        lowGammaTextField?.SetText(data.eegPower.lowGamma.ToString());
        highGammaTextField?.SetText(data.eegPower.highGamma.ToString());
    }

    public void StartSession()
    {
        isSessionActive = true;
        eegDataList.Clear();  // Clear previous session data
    }

    public void EndSession()
    {
        isSessionActive = false;
        SaveToCSV();          // Save captured EEG data
        DisplayHighestAttention();  // Show highest attention value and associated time
    }

    public float GetAttentionValue()
    {
        return lastAttentionValue; // Returns the most recent attention value
    }

    private void SaveToCSV()
    {
        var userData = UserData.Instance;
        string filePath = Path.Combine(Application.persistentDataPath,
                                       "EEGData_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");

        StringBuilder csvContent = new StringBuilder();

        // Add user info
        csvContent.AppendLine("Username,Company Name,Age");
        csvContent.AppendLine($"{userData.Username},{userData.CompanyName},{userData.Age}");
        csvContent.AppendLine("Timestamp,Attention,SessionTime,Meditation,BlinkStrength,Delta,Theta,LowAlpha,HighAlpha,LowBeta,HighBeta,LowGamma,HighGamma,GazeX,GazeY");

        // Add EEG data
        foreach (EEGData entry in eegDataList)
        {
            csvContent.AppendLine(entry.ToCSV());
        }

        // Place Dots on Screen
        PlaceDotsOnScreen(eegDataList);

        // Write the CSV file
        File.WriteAllText(filePath, csvContent.ToString());

        latestCSVFilePath = filePath;
        Debug.Log("EEG data saved to: " + filePath);
    }

    public void PlaceDotsOnScreen(List<EEGData> eegDataList)
    {
        if (eegDataList == null || eegDataList.Count == 0 || canvasRectTransform == null || dotPrefab == null)
            return;

        const int maxDotsToPlace = 4;

        // Find top two attention values using LINQ
        // We'll create a list of (value, index) pairs, sort by value descending, and take the top two
        var topTwo = eegDataList
            .Select((e, i) => new { EEG = e, Index = i })
            .OrderByDescending(x => x.EEG.attentionValue)
            .Take(2)
            .ToList();

        int highestAttentionDotIndex = topTwo.Count > 0 ? topTwo[0].Index : -1;
        int secondHighestAttentionDotIndex = topTwo.Count > 1 ? topTwo[1].Index : -1;

        List<GameObject> dotList = new List<GameObject>();

        // Place up to maxDotsToPlace dots
        for (int i = 0; i < eegDataList.Count && i < maxDotsToPlace; i++)
        {
            EEGData eeg = eegDataList[i];
            Vector2 screenPosition = MapToScreenSpace(eeg.gazePosition);

            // Instantiate the dot
            GameObject dot = Instantiate(dotPrefab, canvasRectTransform);
            var dotRectTransform = dot.GetComponent<RectTransform>();
            dotRectTransform.anchoredPosition = screenPosition;
            dotRectTransform.sizeDelta = new Vector2(70f, 70f);
            dotList.Add(dot);

            // Set the dot's color based on ranking
            Image dotImage = dot.GetComponent<Image>();
            if (dotImage != null)
            {
                if (i == highestAttentionDotIndex)
                    dotImage.color = Color.red; // highest attention
                else if (i == secondHighestAttentionDotIndex)
                    dotImage.color = new Color(1f, 0.5f, 0f); // second highest (orange)
                else
                    dotImage.color = Color.green;
            }

            // Add tooltip
            AddTooltipToDot(dot, eeg.attentionValue, eeg.sessionTime, eeg.meditationValue);

            // Set order text if available
            TextMeshProUGUI orderText = dot.GetComponentInChildren<TextMeshProUGUI>();
            if (orderText != null)
            {
                orderText.text = (i + 1).ToString();
            }
        }

        // Update the UI to show which dot had the highest attention value
        if (highestAttentionValuePoint != null && highestAttentionDotIndex >= 0 && highestAttentionDotIndex < maxDotsToPlace)
        {
            highestAttentionValuePoint.text = $"  {highestAttentionDotIndex + 1}";
        }

        // Connect placed dots with dotted lines
        for (int i = 0; i < dotList.Count - 1; i++)
        {
            ConnectDotsWithDottedLine(dotList[i].transform.position, dotList[i + 1].transform.position);
        }
    }

    private void ConnectDotsWithDottedLine(Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        int numberOfDots = Mathf.FloorToInt(distance / dotSpacing);

        for (int j = 0; j <= numberOfDots; j++)
        {
            float t = j / (float)numberOfDots;
            Vector3 dotPosition = Vector3.Lerp(start, end, t);

            GameObject dottedLine = Instantiate(dottedLinePrefab, canvasRectTransform);
            RectTransform rectTransform = dottedLine.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = canvasRectTransform.InverseTransformPoint(dotPosition);
                rectTransform.sizeDelta = new Vector2(5f, 5f);
            }
        }
    }

    private void AddTooltipToDot(GameObject dot, float attention, string time, float meditation)
    {
        EventTrigger trigger = dot.AddComponent<EventTrigger>();

        // PointerEnter event
        EventTrigger.Entry pointerEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        pointerEnter.callback.AddListener((eventData) => StartTooltipCoroutine(attention, time, meditation));
        trigger.triggers.Add(pointerEnter);

        // PointerExit event
        EventTrigger.Entry pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
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
        // Small delay before showing the tooltip
        yield return new WaitForSeconds(0.2f);
        ShowTooltip(attention, time, meditation);
    }

    private void ShowTooltip(float attention, string time, float meditation)
    {
        if (tooltipPrefab == null || canvasRectTransform == null) return;

        if (tooltipInstance == null)
        {
            tooltipInstance = Instantiate(tooltipPrefab, canvasRectTransform);
        }

        tooltipInstance.SetActive(true);

        TextMeshProUGUI tooltipText = tooltipInstance.GetComponentInChildren<TextMeshProUGUI>();
        if (tooltipText != null)
        {
            tooltipText.text = $"Time: {time}\nAttention: {attention}\nMeditation: {meditation}";
        }

        // Positioning the tooltip near the mouse pointer
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            Input.mousePosition,
            null,
            out Vector2 localPoint))
        {
            Vector2 tooltipOffset = new Vector2(30f, -30f);
            tooltipInstance.GetComponent<RectTransform>().anchoredPosition = localPoint + tooltipOffset;
        }
    }

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
        // Consider making these configurable if resolutions change often
        const float referenceWidth = 1920f;
        const float referenceHeight = 1080f;

        // Map gaze coordinates to canvas space
        float mappedX = (gazePos.x / referenceWidth) * canvasRectTransform.sizeDelta.x;
        float mappedY = -(gazePos.y / referenceHeight) * canvasRectTransform.sizeDelta.y;

        return new Vector2(mappedX, mappedY);
    }

    public string GetLatestCSVFilePath()
    {
        return latestCSVFilePath;
    }

    public void PostEEGData()
    {
        // Check if userId or eegDataList are valid before starting
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID is not set. Cannot post EEG data.");
            return;
        }

        if (eegDataList == null || eegDataList.Count == 0)
        {
            Debug.LogWarning("No EEG data to send.");
            return;
        }

        StartCoroutine(SendEEGData());
    }

    private IEnumerator SendEEGData()
    {
        string url = "http://localhost:81/sqlconnect/eegpost.php";

        foreach (EEGData data in eegDataList)
        {
            UnityWebRequest request = UnityWebRequest.Post(url, CreateEEGForm(data));
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

    private WWWForm CreateEEGForm(EEGData data)
    {
        WWWForm form = new WWWForm();
        form.AddField("userID", userId);
        form.AddField("timestamp", data.timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
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

        return form;
    }
}
