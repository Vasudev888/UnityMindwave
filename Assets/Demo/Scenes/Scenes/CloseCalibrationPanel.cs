using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseCalibrationPanel : MonoBehaviour
{
    [SerializeField] GameObject calibrationCanva;

    public void DisableCalibCanva()
    {
        calibrationCanva.SetActive(false);
    }

}
