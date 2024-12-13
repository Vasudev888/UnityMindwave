using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CalibrationReceiverUDP : MonoBehaviour
{
    [Header("Video Settings")]
    public RawImage rawImage;

    [Header("Grid Settings")]
    [SerializeField] private RectTransform targetPrefab;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private float edgeMargin = 50f;
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private float rotationAngle = 360f;

    private UdpClient udpClient;
    private Thread clientThread;
    private StringBuilder messageBuilder = new StringBuilder();
    private string receivedMessage = string.Empty;
    private RectTransform targetInstance;
    private Vector2[] primaryPositions;
    private Vector2[] extraPositions;
    private int currentIndex = 0;
    private int iterationCount = 0;

    private enum CalibrationType { Screen, Iris, Extra }
    private CalibrationType currentCalibrationType = CalibrationType.Screen;
    private bool isCalibrationActive = true;

    private IPEndPoint remoteEndPoint; // Add this declaration at the class level


    private void Start()
    {
        try
        {
            udpClient = new UdpClient(5001);
            clientThread = new Thread(ListenForMessages);
            clientThread.Start();
            Debug.Log("Listening for UDP packets...");

            // Calculate primary and extra positions
            float canvasWidth = canvasRect.rect.width;
            float canvasHeight = canvasRect.rect.height;

            Vector2 center = Vector2.zero;
            Vector2 leftMid = new Vector2(-canvasWidth / 2 + edgeMargin, 0);
            Vector2 rightMid = new Vector2(canvasWidth / 2 - edgeMargin, 0);
            Vector2 topMid = new Vector2(0, canvasHeight / 2 - edgeMargin);
            Vector2 bottomMid = new Vector2(0, -canvasHeight / 2 + edgeMargin);

            primaryPositions = new Vector2[] { leftMid, rightMid, topMid, bottomMid };

            Vector2 extraLeftMid = (center + leftMid) / 2;
            Vector2 extraRightMid = (center + rightMid) / 2;
            Vector2 extraTopMid = (center + topMid) / 2;
            Vector2 extraBottomMid = (center + bottomMid) / 2;

            extraPositions = new Vector2[] { extraLeftMid, extraRightMid, extraTopMid, extraBottomMid };

            // Instantiate target prefab
            targetInstance = Instantiate(targetPrefab, canvasRect);
            targetInstance.anchoredPosition = leftMid;

            Button targetButton = targetInstance.GetComponent<Button>();
            if (targetButton != null)
            {
                targetButton.onClick.AddListener(OnTargetButtonClick);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error during initialization: {e.Message}");
        }
    }

    private void Update()
    {
        // Handle video frames
        while (udpClient.Available > 0)
        {
            try
            {
                byte[] receivedData = udpClient.Receive(ref remoteEndPoint);
                string messagePart = Encoding.UTF8.GetString(receivedData);

                if (messagePart == "<END>")
                {
                    byte[] imageBytes = Convert.FromBase64String(messageBuilder.ToString());
                    Texture2D texture = new Texture2D(2, 2);
                    texture.LoadImage(imageBytes);
                    rawImage.texture = texture;
                    messageBuilder.Clear();
                }
                else
                {
                    messageBuilder.Append(messagePart);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error processing video data: {e.Message}");
            }
        }

        // Handle calibration messages
        lock (this)
        {
            if (!string.IsNullOrEmpty(receivedMessage))
            {
                Debug.Log($"Message from Python: {receivedMessage}");
                receivedMessage = string.Empty;
            }
        }
    }

    private void ListenForMessages()
    {
        try
        {
            udpClient = new UdpClient(65432);
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                byte[] data = udpClient.Receive(ref remoteEndPoint);
                string message = Encoding.UTF8.GetString(data);
                lock (this)
                {
                    receivedMessage = message;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in UDP listener: {e.Message}");
        }
    }

    private void OnTargetButtonClick()
    {
        switch (currentCalibrationType)
        {
            case CalibrationType.Screen:
                SendScreenCalibrationCommand();
                break;
            case CalibrationType.Iris:
                SendIrisCalibrationCommand();
                break;
            case CalibrationType.Extra:
                SendExtraCalibrationCommand();
                break;
        }

        MoveToNextPosition();
    }

    private void SendScreenCalibrationCommand()
    {
        string command = currentIndex switch
        {
            0 => "calibrate_screen_left",
            1 => "calibrate_screen_right",
            2 => "calibrate_screen_top",
            3 => "calibrate_screen_bottom",
            _ => string.Empty
        };

        if (!string.IsNullOrEmpty(command))
        {
            SendCommand(command);

            if (currentIndex == 3)
            {
                currentCalibrationType = CalibrationType.Iris;
                Debug.Log("Switching to Iris Calibration");
            }
        }
    }

    private void SendIrisCalibrationCommand()
    {
        string command = currentIndex switch
        {
            0 => "calibrate_iris_left",
            1 => "calibrate_iris_right",
            2 => "calibrate_iris_top",
            3 => "calibrate_iris_bottom",
            _ => string.Empty
        };

        if (!string.IsNullOrEmpty(command))
        {
            SendCommand(command);

            if (currentIndex == 3)
            {
                currentCalibrationType = CalibrationType.Extra;
                Debug.Log("Switching to Extra Calibration");
            }
        }
    }

    private void SendExtraCalibrationCommand()
    {
        if (!isCalibrationActive)
        {
            Debug.Log("Calibration is already complete.");
            return;
        }

        string command = currentIndex switch
        {
            0 => "calibrate_extra_left",
            1 => "calibrate_extra_right",
            2 => "calibrate_extra_top",
            3 => "calibrate_extra_bottom",
            _ => string.Empty
        };

        if (!string.IsNullOrEmpty(command))
        {
            SendCommand(command);

            if (currentIndex == 3)
            {
                Debug.Log("Completed all Extra calibration steps.");
                EndCalibration();
            }
        }
    }

    private void SendCommand(string command)
    {
        byte[] data = Encoding.UTF8.GetBytes(command + "\n");
        udpClient.Send(data, data.Length, "127.0.0.1", 65432);
        Debug.Log($"Sent command: {command}");
    }

    private void MoveToNextPosition()
    {
        if (!isCalibrationActive) return;

        Vector2 targetPosition = iterationCount < 2 ? primaryPositions[currentIndex] : extraPositions[currentIndex];

        currentIndex++;
        if (currentIndex >= 4)
        {
            currentIndex = 0;
            iterationCount++;

            if (iterationCount == 2)
            {
                switch (currentCalibrationType)
                {
                    case CalibrationType.Screen:
                        currentCalibrationType = CalibrationType.Iris;
                        break;
                    case CalibrationType.Iris:
                        currentCalibrationType = CalibrationType.Extra;
                        break;
                    case CalibrationType.Extra:
                        Debug.Log("Calibration complete!");
                        EndCalibration();
                        break;
                }
            }

            targetPosition = iterationCount < 2 ? primaryPositions[currentIndex] : extraPositions[currentIndex];
        }

        float rotationDirection = (currentIndex % 2 == 0) ? -rotationAngle : rotationAngle;
        targetInstance.DOAnchorPos(targetPosition, moveDuration).SetEase(Ease.Linear);
        targetInstance.DORotate(new Vector3(0, 0, rotationDirection), moveDuration, RotateMode.LocalAxisAdd);
    }

    private void EndCalibration()
    {
        isCalibrationActive = false;
        if (targetInstance != null) Destroy(targetInstance.gameObject);
    }

    private void OnDestroy()
    {
        if (clientThread != null && clientThread.IsAlive) clientThread.Abort();
        if (udpClient != null) udpClient.Close();
    }
}
