using System.Collections;
using TMPro;
using UnityEngine;
using System;

public class TimeCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI counterText;
    private bool isRunning = false;   // Flag to check if the timer is running
    private DateTime startTime;       // Store the system start time

    // Method to start the timer
    public void StartTimer()
    {
        if (!isRunning)
        {
            startTime = DateTime.Now;  // Record the system start time
            isRunning = true;
            StartCoroutine(Counter());
        }
    }

    // Coroutine to update the timer display
    IEnumerator Counter()
    {
        while (isRunning)
        {
            // Calculate the elapsed time
            TimeSpan elapsedTime = DateTime.Now - startTime;

            // Display the elapsed time in "HH:mm:ss" format
            counterText.text = elapsedTime.ToString(@"hh\:mm\:ss");

            yield return new WaitForSeconds(1);  // Update every second
        }
    }

    // Method to stop the timer
    public void StopTimer()
    {
        isRunning = false;  // Stop the timer
    }

    // Method to get the current elapsed time as a string
    public string GetElapsedTime()
    {
        if (isRunning)
        {
            TimeSpan elapsedTime = DateTime.Now - startTime;
            return elapsedTime.ToString(@"hh\:mm\:ss");
        }
        return "00:00:00";  // Return default value if timer isn't running
    }
}
