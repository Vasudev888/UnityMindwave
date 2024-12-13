using UnityEngine;
using System.Collections.Generic;
//using SimpleGraphs; // Replace with the correct namespace of your graphing package

public class MindmapManager : MonoBehaviour
{
    /*    public Graph attentionGraph; // Assign this from the inspector
        public Graph meditationGraph; // Assign this from the inspector
    */
   /* private List<float> attentionData = new List<float>();
    private List<float> meditationData = new List<float>();

    void Start()
    {
        // Initialize the Mindwave device and start receiving data
        MindwaveData.Instance.OnAttentionReceived += OnAttentionReceived;
        MindwaveData.Instance.OnMeditationReceived += OnMeditationReceived;
    }

    void OnAttentionReceived(float value)
    {
        attentionData.Add(value);
        UpdateGraph(attentionGraph, attentionData);
    }

    void OnMeditationReceived(float value)
    {
        meditationData.Add(value);
        UpdateGraph(meditationGraph, meditationData);
    }

    void UpdateGraph(Graph graph, List<float> data)
    {
        graph.Clear(); // Clear previous data
        graph.SetValues(data.ToArray()); // Update graph with new data
    }

    void OnDestroy()
    {
        // Unsubscribe from the events
        MindwaveData.Instance.OnAttentionReceived -= OnAttentionReceived;
        MindwaveData.Instance.OnMeditationReceived -= OnMeditationReceived;
    }*/
}
