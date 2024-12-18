using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

public class LaunchWebCanvas : MonoBehaviour
{
    [SerializeField] GameObject webComponent;
    [SerializeField] GameObject browserCanavas;
 

    public void ScaleImageGameObject()
    {

        webComponent.transform.localScale = new Vector3(1, 1, 1);

        Debug.Log("callING ScaleImageGameObject");


    }


    public void DisableWebCanvas()
    {
        browserCanavas.SetActive(false);
    }
}
