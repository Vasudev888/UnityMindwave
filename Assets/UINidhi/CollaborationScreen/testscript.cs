using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class testscript : MonoBehaviour
{
    // Start is called before the first frame update
    Image progressFillImage;
    private float fillValue = 0f;
    void Start()
    {
        progressFillImage = gameObject.GetComponent<Image>();   
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // Press Space to increment
        {
            if (progressFillImage != null)
            {
                fillValue += 0.1f; // Increment fill value
                fillValue = Mathf.Clamp01(fillValue); // Clamp between 0 and 1
                progressFillImage.fillAmount = fillValue; // Update the fill amount
                Debug.Log($"Fill Amount Updated: {progressFillImage.fillAmount}");
            }
            else
            {
                Debug.LogError("progressFillImage is null!");
            }
        }
    }
}
