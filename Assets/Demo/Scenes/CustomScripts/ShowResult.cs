using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowResult : MonoBehaviour
{
    [SerializeField] private GameObject targetGameObject; // Assign the GameObject you want to control in the Inspector

/*    private void Start()
    {
        // Check if targetGameObject is assigned
        if (targetGameObject != null)
        {
            // Enable the GameObject
            EnableGameObject(false);
        }
        else
        {
            Debug.LogError("Target GameObject is not assigned.");
        }
    }*/

    // Method to enable or disable the GameObject
    public void EnableGameObject(bool enable)
    {
        if (targetGameObject != null)
        {
            targetGameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("Target GameObject is not assigned.");
        }
    }

    public void DisableGameObject(bool enable)
    {
        if (targetGameObject != null)
        {
            targetGameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Target GameObject is not assigned.");
        }
    }
}
