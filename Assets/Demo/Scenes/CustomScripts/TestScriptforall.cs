using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using BarGraph.VittorCloud;
using TMPro;

public class TestScriptforall : MonoBehaviour
{
    public Image attentionBar;
    public Image meditationBar;
    public TextMeshProUGUI attentionText;
    public TextMeshProUGUI meditationText;

    private int AttentionTextonUI;
    public string test101;

    public BarGraphGenerator barGraphGenerator;
    private List<BarGraphDataSet> mindwaveDataSet;

    private MindwaveDataModel m_MindwaveData;

    private void Start()
    {
        AttentionTextonUI = m_MindwaveData.eSense.attention;
        test101 = AttentionTextonUI.ToString();
        // Subscribe to the OnUpdateMindwaveData event
        MindwaveManager.Instance.Controller.OnUpdateMindwaveData += OnMindwaveDataUpdated;

        // Initialize bar graph
        InitializeMindwaveDataSet();
        barGraphGenerator.GeneratBarGraph(mindwaveDataSet);
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event when the script is destroyed
        if (MindwaveManager.Instance != null)
        {
            MindwaveManager.Instance.Controller.OnUpdateMindwaveData -= OnMindwaveDataUpdated;
        }
    }

    private void InitializeMindwaveDataSet()
    {
        mindwaveDataSet = new List<BarGraphDataSet>
        {
            new BarGraphDataSet
            {
                ListOfBars = new List<XYBarValues>
                {
                    new XYBarValues { XValue = "Attention", YValue = 0 },
                    new XYBarValues { XValue = "Meditation", YValue = 0 }
                }
            }
        };
    }

    private void OnMindwaveDataUpdated(MindwaveDataModel data)
    {
        // Update attention
        int attentionValue = data.eSense.attention;
        float normalizedAttention = attentionValue / 100f;
        attentionBar.fillAmount = normalizedAttention;
        attentionText.text = $"Attention: {attentionValue}%";

        // Update meditation
        int meditationValue = data.eSense.meditation;
        float normalizedMeditation = meditationValue / 100f;
        meditationBar.fillAmount = normalizedMeditation;
        meditationText.text = $"Meditation: {meditationValue}%";

        // Update bar graph data
        mindwaveDataSet[0].ListOfBars[0].YValue = attentionValue;
        mindwaveDataSet[0].ListOfBars[1].YValue = meditationValue;

        // Update bar graph visualization
        barGraphGenerator.AddNewDataSet(0, 0, attentionValue);
        barGraphGenerator.AddNewDataSet(0, 1, meditationValue);
    }
}
