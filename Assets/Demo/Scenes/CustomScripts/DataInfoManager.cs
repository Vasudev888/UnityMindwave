using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataInfoManager : MonoBehaviour
{
    [SerializeField] GameObject panel;

    public void OnlickOpenClosePanel()
    {
        if(panel != null)
        {
            panel.SetActive(!panel.activeSelf);
        }
       
    }
   
}
