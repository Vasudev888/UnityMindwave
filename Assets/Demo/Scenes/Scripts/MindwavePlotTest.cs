using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class MindwavePlotTest : MonoBehaviour
{
    [SerializeField] private RectTransform canvasRectTransform;  // Reference to the canvas RectTransform
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private GameObject tooltipPrefab;
    private GameObject tooltipInstance;

    private List<TestEEGData> testEEGDataList = new List<TestEEGData>(); // List to store random EEG data
    private Coroutine tooltipCoroutine;

    // Structure to hold random EEG data and gaze position
    public struct TestEEGData
    {
        public float attentionValue;
        public float meditationValue;
        public Vector2 gazePosition; // Random gaze position
        public DateTime timestamp;
        public string sessionTime;

        public TestEEGData(float attention, float meditation, Vector2 gazePos, string sessionTime, DateTime timestamp)
        {
            this.attentionValue = attention;
            this.meditationValue = meditation;
            this.gazePosition = gazePos;
            this.sessionTime = sessionTime;
            this.timestamp = timestamp;
        }

        // Convert to CSV format for testing if needed
        public string ToCSV()
        {
            return $"{timestamp},{attentionValue},{sessionTime},{meditationValue},{gazePosition.x},{gazePosition.y}";
        }
    }

    private void Start()
    {
        GenerateRandomEEGData(5);  // Generate 5 random EEG data points for testing
        PlaceDotsOnScreen(testEEGDataList);
    }

    // Function to generate random EEG data and gaze coordinates
    private void GenerateRandomEEGData(int count)
    {
        for (int i = 0; i < count; i++)
        {
            float randomAttention = UnityEngine.Random.Range(0f, 100f);
            float randomMeditation = UnityEngine.Random.Range(0f, 100f);

            // Generate random gaze positions
            Vector2 randomGazePosition = new Vector2(
                UnityEngine.Random.Range(0f, 1280f),  // Assuming a screen width of 1280px
                UnityEngine.Random.Range(0f, 720f));  // Assuming a screen height of 720px

            string sessionTime = $"00:00:{i * 2}";  // Increment session time for each dot (for debugging)

            // Add random EEG data to the list
            testEEGDataList.Add(new TestEEGData(randomAttention, randomMeditation, randomGazePosition, sessionTime, DateTime.Now.AddSeconds(i * 2)));
        }
    }


    /*[SerializeField] private LineRenderer lineRenderer;
    public void PlaceDotsOnScreen(List<TestEEGData> eegDataList)
    {
        Vector3[] linePositions = new Vector3[eegDataList.Count];
        for (int i = 0; i < eegDataList.Count; i++)
        {
            TestEEGData eeg = eegDataList[i];

            // Map the gaze position to canvas space
            Vector2 screenPosition = MapToScreenSpace(eeg.gazePosition);

            // Apply a small random offset to the screen position to avoid overlap
            Vector2 randomOffset = new Vector2(UnityEngine.Random.Range(-20f, 20f), UnityEngine.Random.Range(-20f, 20f));
            screenPosition += randomOffset;  // Add the offset to the original position

            // Instantiate the dot prefab and set its position
            GameObject dot = Instantiate(dotPrefab, canvasRectTransform);
            RectTransform dotRectTransform = dot.GetComponent<RectTransform>();
            dotRectTransform.anchoredPosition = screenPosition;

            Image dotImage = dot.GetComponent<Image>();  // Assuming the dot prefab has an Image component
            if (dotImage != null)
            {
                if (i == 0)  // First dot red
                {
                    dotImage.color = Color.red;
                }
                else if (i == 1)  // Second dot yellow
                {
                    dotImage.color = Color.yellow;
                }
                else  // All remaining dots green
                {
                    dotImage.color = Color.green;
                }
            }

            // Add the tooltip to the dot
            AddTooltipToDot(dot, eeg.attentionValue, eeg.sessionTime.ToString(), eeg.meditationValue);

            // Add the order as a Text component on the dot
            TextMeshProUGUI orderText = dot.GetComponentInChildren<TextMeshProUGUI>();  // Assuming the dot prefab has a Text component
            if (orderText != null)
            {
                orderText.text = (i + 1).ToString();  // Display the order
            }

            dotRectTransform.sizeDelta = new Vector2(30f, 30f);  // Adjust the size as needed to avoid overlap

            // Store the world position of the dot for the line
            Vector3 worldPosition = dot.transform.position;  // Get the world position of the dot
            linePositions[i] = worldPosition;  //  Store it in the line positions array

            // Optionally scale the dot to a smaller size if needed
            dotRectTransform.sizeDelta = new Vector2(30f, 30f);  // Adjust the size as needed to avoid overlap
        }
        // Set up the LineRenderer
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = eegDataList.Count;  // Set the number of positions in the line
            lineRenderer.SetPositions(linePositions);  // Apply the positions to the LineRenderer
        }

    }*/
    [SerializeField] private GameObject dottedLinePrefab;  // The dotted line prefab to instantiate between the dots
    [SerializeField] private float dotSpacing = 0.1f;      // The spacing between each dotted line instance (adjust as needed)

    public void PlaceDotsOnScreen(List<TestEEGData> eegDataList)
    {
        List<GameObject> dotList = new List<GameObject>();  // To store instantiated dots for later use

        for (int i = 0; i < eegDataList.Count; i++)
        {
            TestEEGData eeg = eegDataList[i];

            // Map the gaze position to canvas space
            Vector2 screenPosition = MapToScreenSpace(eeg.gazePosition);

            // Apply a small random offset to the screen position to avoid overlap
            Vector2 randomOffset = new Vector2(UnityEngine.Random.Range(-20f, 20f), UnityEngine.Random.Range(-20f, 20f));
            screenPosition += randomOffset;  // Add the offset to the original position

            // Instantiate the dot prefab and set its position
            GameObject dot = Instantiate(dotPrefab, canvasRectTransform);
            RectTransform dotRectTransform = dot.GetComponent<RectTransform>();
            dotRectTransform.anchoredPosition = screenPosition;

            // Store the dot GameObject in the list
            dotList.Add(dot);

            // Set color based on the order
            Image dotImage = dot.GetComponent<Image>();  // Assuming the dot prefab has an Image component
            if (dotImage != null)
            {
                if (i == 0)
                    dotImage.color = Color.red;  // First dot red
                else if (i == 1)
                    dotImage.color = Color.yellow;  // Second dot yellow
                else
                    dotImage.color = Color.green;  // Remaining dots green
            }

            // Add tooltip and order text as before
            AddTooltipToDot(dot, eeg.attentionValue, eeg.sessionTime.ToString(), eeg.meditationValue);
            TextMeshProUGUI orderText = dot.GetComponentInChildren<TextMeshProUGUI>();
            if (orderText != null)
            {
                orderText.text = (i + 1).ToString();  // Display the order
            }

            // Optionally scale the dot to a smaller size if needed
            dotRectTransform.sizeDelta = new Vector2(30f, 30f);  // Adjust the size as needed to avoid overlap
        }

        // Now connect the dots with the dotted line prefab
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
        int numberOfDots = Mathf.FloorToInt(distance / dotSpacing);  // Number of dots needed based on the spacing

        for (int j = 0; j <= numberOfDots; j++)
        {
            // Calculate the interpolated position
            float t = j / (float)numberOfDots;
            Vector3 dotPosition = Vector3.Lerp(start, end, t);

            // Instantiate a single dotted line prefab at the calculated position
            GameObject dottedLine = Instantiate(dottedLinePrefab, canvasRectTransform);

            // Set its position in screen space
            dottedLine.transform.position = dotPosition;

            // Optionally, adjust the scale or size if needed
            RectTransform rectTransform = dottedLine.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(5f, 5f);  // Adjust size for better visibility if needed
            }
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
        float mappedX = (gazePos.x / 1280f) * canvasRectTransform.sizeDelta.x;  // Assuming the webcam resolution is 1280x720
        float mappedY = -(gazePos.y / 720f) * canvasRectTransform.sizeDelta.y;

        return new Vector2(mappedX, mappedY);
    }
}


