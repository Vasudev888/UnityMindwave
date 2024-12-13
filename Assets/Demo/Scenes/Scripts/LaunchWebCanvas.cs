using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchWebCanvas : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject canvas;
    public void EnableGameObject(bool enable)
    {
        if (canvas != null)
        {
            canvas.SetActive(true);
        }
        else
        {
            Debug.LogError("Target GameObject is not assigned.");
        }
    }

    public void DisableGameObject(bool enable)
    {
        if (canvas != null)
        {
            canvas.SetActive(false);
        }
        else
        {
            Debug.LogError("Target GameObject is not assigned.");
        }
    }
}
