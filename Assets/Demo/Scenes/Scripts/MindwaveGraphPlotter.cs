using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class MindwaveGraphPlotter : MonoBehaviour
{
    [Header("Graph Elements")]
    public List<Image> attentionBars;  // List of Image objects representing attention bars
    public List<Image> meditationBars; // List of Image objects representing meditation bars
    public List<TextMeshProUGUI> xAxisLabels;     // List of Text objects for X-axis labels
    public TextMeshProUGUI yAxisLabelPrefab;      // Prefab for Y-axis labels (0%, 20%, etc.)
    public RectTransform yAxisParent;  // Parent object for Y-axis labels

    [Header("Graph Data")]
    public List<float> attentionValues;  // List of attention values
    public List<float> meditationValues; // List of meditation values
    public List<string> xLabels;         // List of labels for X-axis (e.g., time intervals or website sections)

    private void Start()
    {
        // Set up Y-axis labels
        for (int i = 0; i <= 5; i++)  // For 0%, 20%, 40%, 60%, 80%, 100%
        {
            TextMeshProUGUI label = Instantiate(yAxisLabelPrefab, yAxisParent);
            label.text = (i * 20).ToString() + "%";
            label.rectTransform.anchoredPosition = new Vector2(0, i * (yAxisParent.rect.height / 5));
        }

        // Set up X-axis labels
        for (int i = 0; i < xAxisLabels.Count && i < xLabels.Count; i++)
        {
            xAxisLabels[i].text = xLabels[i];
        }

        // Plot attention and meditation bars
        for (int i = 0; i < attentionBars.Count && i < attentionValues.Count; i++)
        {
            float normalizedAttention = attentionValues[i] / 100f;
            attentionBars[i].fillAmount = normalizedAttention;

            float normalizedMeditation = meditationValues[i] / 100f;
            meditationBars[i].fillAmount = normalizedMeditation;
        }
    }
}
