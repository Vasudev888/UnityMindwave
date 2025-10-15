using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipButtons : MonoBehaviour
{
    [SerializeField] GameObject startTestButton;
    [SerializeField] GameObject endTestButton;
    [SerializeField] GameObject thatPanel;
    [Header("Timing")]
    [SerializeField] float loadingDuration = 3f;

    [SerializeField] GameObject analysisPanel;
    public void EnableEndButton()
    {
        if (startTestButton != null)
        {
            startTestButton.SetActive(false);
        }

        if(endTestButton != null)
        {
            endTestButton.SetActive(true);
        }
    }


    public void EnableAnalysisPanel()
    {
        if (analysisPanel != null)
        {
            analysisPanel.SetActive(true);
        }
    }

    public void DisableAnalysisPanel()
    {
        if(analysisPanel != null) { analysisPanel.SetActive(false); }
    }

}
