using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;
using System;

public class TestUDP : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private RectTransform targetPrefab; // Prefab to instantiate and move between points
    [SerializeField] private RectTransform canvasRect; // Canvas RectTransform to get canvas size
    [SerializeField] private float edgeMargin = 50f; // Margin from edges to avoid truncation
    [SerializeField] private float moveDuration = 1f; // Duration for each move, adjustable in the inspector
    [SerializeField] private float rotationAngle = 360f; // Full rotation angle for each move

    private UdpClient udpClient;
    private Thread clientThread;
    private string receivedMessage = string.Empty; // Store incoming messages from Python

    private RectTransform targetInstance;
    private Vector2[] primaryPositions;
    private Vector2[] extraPositions;
    private int currentIndex = 0;
    private int iterationCount = 0;

    private enum CalibrationType { Screen, Iris, Extra }
    private CalibrationType currentCalibrationType = CalibrationType.Screen; // Default to Screen calibration

    private void Start()
    {
        // Start the UDP listener thread
        clientThread = new Thread(ListenForMessages);
        clientThread.Start();

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

        // Instantiate the targetPrefab at the leftMid position and set initial position
        targetInstance = Instantiate(targetPrefab, canvasRect);
        targetInstance.anchoredPosition = Vector2.zero; // Start at the center position

        // Set up the button listener on the target prefab
        Button targetButton = targetInstance.GetComponent<Button>();
        if (targetButton != null)
        {
            targetButton.onClick.AddListener(OnTargetButtonClick);
            Debug.Log("Button Clicked: " + targetButton);
        }
    }

    private void ListenForMessages()
    {
        try
        {
            // Initialize the UDP client to listen on port 65432
            udpClient = new UdpClient(65432);
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            Debug.Log("Listening for UDP messages...");

            while (true)
            {
                // Receive data from Python
                byte[] data = udpClient.Receive(ref remoteEndPoint);
                string message = Encoding.UTF8.GetString(data);

                lock (this)
                {
                    receivedMessage = message; // Store received message
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error in UDP listener: " + e.Message);
        }
    }

    private void Update()
    {
        // Handle received message from Python on Unity's main thread
        lock (this)
        {
            if (!string.IsNullOrEmpty(receivedMessage))
            {
                Debug.Log("Message from Python: " + receivedMessage);
                // Process the message if necessary
                receivedMessage = string.Empty;
            }
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
        //SendCommand(command);

        if (!string.IsNullOrEmpty(command))
        {
            SendCommand(command);

            // Check if this is the last step of screen calibration
            if (currentIndex == 3)
            {
                currentCalibrationType = CalibrationType.Iris; // Switch to Iris Calibration
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
        //SendCommand(command);
        if (!string.IsNullOrEmpty(command))
        {
            SendCommand(command);

            // Check if this is the last step of iris calibration
            if (currentIndex == 3)
            {
                currentCalibrationType = CalibrationType.Extra; // Switch to Extra Calibration
                Debug.Log("Switching to Extra Calibration");
            }
        }
    }

    private void SendExtraCalibrationCommand()
    {
        string command = currentIndex switch
        {
            0 => "calibrate_extra_left",
            1 => "calibrate_extra_right",
            2 => "calibrate_extra_top",
            3 => "calibrate_extra_bottom",
            _ => string.Empty
        };
        //SendCommand(command);
        if (!string.IsNullOrEmpty(command))
        {
            SendCommand(command);
        }
    }

    private void SendCommand(string command)
    {
        if (!string.IsNullOrEmpty(command))
        {
            byte[] data = Encoding.UTF8.GetBytes(command + "\n");
            Debug.Log("Encoded data: " + BitConverter.ToString(data));
            try
            {
                // Send the command via UDP
                udpClient.Send(data, data.Length, "127.0.0.1", 65432); // Python server address and port
                Debug.Log("Sent command: " + command);
            }
            catch (Exception e)
            {
                Debug.LogError("Error sending command: " + e.Message);
            }
        }
    }

    private void MoveToNextPosition()
    {

        Debug.Log($"Moving to next position. CurrentIndex: {currentIndex}, IterationCount: {iterationCount}");
        // Determine target positions based on iteration count
        Vector2 targetPosition = iterationCount < 2 ? primaryPositions[currentIndex] : extraPositions[currentIndex];

        // Calculate rotation direction based on the current position
        // float rotationDirection = (currentIndex % 2 == 0) ? -rotationAngle : rotationAngle;

        // Move and rotate the target
        targetInstance.DOAnchorPos(targetPosition, moveDuration).SetEase(Ease.Linear);
        //targetInstance.DORotate(new Vector3(0, 0, rotationDirection), moveDuration, RotateMode.LocalAxisAdd);

        // Update the index for the next position
        currentIndex++;

        // If all positions in the current calibration type are completed
        if (currentIndex >= 4)
        {
            currentIndex = 0; // Reset index
            iterationCount++; // Increment iteration count

            // Transition to the next calibration type
            if (iterationCount == 2)
            {
                switch (currentCalibrationType)
                {
                    case CalibrationType.Screen:
                        currentCalibrationType = CalibrationType.Iris;
                        Debug.Log("Switching to Iris Calibration.");
                        break;

                    case CalibrationType.Iris:
                        currentCalibrationType = CalibrationType.Extra;
                        Debug.Log("Switching to Extra Calibration.");
                        break;

                    case CalibrationType.Extra:
                        Debug.Log("Calibration complete!");
                        break;
                }
            }
        }
    }

    private void OnDestroy()
    {
        // Clean up the UDP client and thread when the application exits
        if (clientThread != null && clientThread.IsAlive)
        {
            clientThread.Abort();
        }
        if (udpClient != null)
        {
            udpClient.Close();
        }
    }
}
