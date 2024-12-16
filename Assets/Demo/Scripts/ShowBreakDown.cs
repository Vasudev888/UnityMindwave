using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowBreakDown : MonoBehaviour
{
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject closeButton;

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


    public void ScaleImageGameObject(bool enable)
    {
        if (canvas != null)
        {
            canvas.transform.localScale = new Vector3(1, 1, 1);
            closeButton.SetActive(true);

        }
        else
        {
            Debug.LogError("Target GameObject is not assigned.");
        }
    }

    public void DeScaleGameObject(bool enable)
    {
        if (canvas != null)
        {
            canvas.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            closeButton.SetActive(false);
        }
        else
        {
            Debug.LogError("Target GameObject is not assigned.");
        }
    }
}
