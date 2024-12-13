using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BarGraph.VittorCloud;

public class MindwaveBarGraph : MonoBehaviour
{
    public List<BarGraphDataSet> mindwaveDataSet;
    private BarGraphGenerator barGraphGenerator;

    private void Start()
    {
        barGraphGenerator = GetComponent<BarGraphGenerator>();
        InitializeMindwaveDataSet();
        barGraphGenerator.GeneratBarGraph(mindwaveDataSet);

        // Subscribe to the OnUpdateMindwaveData event
        MindwaveManager.Instance.Controller.OnUpdateMindwaveData += OnMindwaveDataUpdated;
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
        int attentionValue = data.eSense.attention;
        int meditationValue = data.eSense.meditation;

        // Update the bar graph data
        mindwaveDataSet[0].ListOfBars[0].YValue = attentionValue;
        mindwaveDataSet[0].ListOfBars[1].YValue = meditationValue;

        // Update the bar graph visualization
        barGraphGenerator.AddNewDataSet(0, 0, attentionValue);
        barGraphGenerator.AddNewDataSet(0, 1, meditationValue);
    }
}