using UnityEngine;
using System;

public class MindwaveData : MonoBehaviour
{
    public static MindwaveData Instance;

    public event Action<float> OnAttentionReceived;
    public event Action<float> OnMeditationReceived;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Simulate receiving data
        if (Input.GetKeyDown(KeyCode.A))
        {
            OnAttentionReceived?.Invoke(UnityEngine.Random.Range(0f, 100f));
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            OnMeditationReceived?.Invoke(UnityEngine.Random.Range(0f, 100f));
        }
    }

    // Add real data reception logic here
}
