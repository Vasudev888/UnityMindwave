using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using System.IO;
using DG.Tweening;
using UnityEngine.UI;

public class DiamaondGridUPDMain : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private RectTransform targetPrefab; // Prefab to instantiate and move between points
    [SerializeField] private RectTransform canvasRect; // Canvas RectTransform to get canvas size
    [SerializeField] private float edgeMargin = 50f; // Margin from edges to avoid truncation
    [SerializeField] private float moveDuration = 1f; // Duration for each move, adjustable in the inspector
    [SerializeField] private float rotationAngle = 360f; // Full rotation angle for each move

    private UdpClient udpClient;
    private UdpClient sendUdpClient;

    private Thread clientThread;
    private string receivedMessage = string.Empty; // Store incoming messages from Python

    private RectTransform targetInstance;
    private Vector2[] primaryPositions;
    private Vector2[] extraPositions;
    private int currentIndex = 0;
    private int iterationCount = 0;

    private enum CalibrationType { Screen, Iris, Extra }
    private CalibrationType currentCalibrationType = CalibrationType.Screen; // Default to Screen calibration
    private bool isCalibrationActive = true; // Flag to manage calibration process

    [Header("Video Display")]
    public RawImage rawImage;

    [Header("Heatmap Settings")]
    public Texture2D heatmapTexture;
    public int heatmapWidth = 1280;
    public int heatmapHeight = 720;
    public int brushSize = 20;
    public float intensity = 0.1f;
    public RawImage heatmapDisplay;


    private float[,] heatmapData;
    private Vector2Int latestScreenPosition = Vector2Int.zero;
    private bool hasNewData = false;
    private bool isBrushActive = true; // Set to true to start generating heatmap

    private IPEndPoint remoteEndPoint;

    private StringBuilder messageBuilder = new StringBuilder();

    private void Start()
    {


        // Start the UDP listener thread
        clientThread = new Thread(ListenForMessages);
        clientThread.Start();

        // Initialize the sending UdpClient
        sendUdpClient = new UdpClient();

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
        targetInstance.anchoredPosition = leftMid; // Start position

        // Set up the button listener on the target prefab
        Button targetButton = targetInstance.GetComponent<Button>();
        if (targetButton != null)
        {
            targetButton.onClick.AddListener(OnTargetButtonClick);
            Debug.Log("Button Clicked: " + targetButton);
        }

        // Initialize heatmap data array for storing intensity values
        heatmapData = new float[heatmapWidth, heatmapHeight];

        // Initialize the heatmap texture with RGBA format for transparency
        heatmapTexture = new Texture2D(heatmapWidth, heatmapHeight, TextureFormat.RGBA32, false);

        // Initialize the texture to be fully transparent
        for (int i = 0; i < heatmapWidth; i++)
        {
            for (int j = 0; j < heatmapHeight; j++)
            {
                heatmapTexture.SetPixel(i, j, Color.clear);
                heatmapData[i, j] = 0f;  // Initialize the intensity data array to zero
            }
        }
        heatmapTexture.Apply();  // Apply changes to the texture

        // Assign heatmap texture to the RawImage in the UI, if set
        if (heatmapDisplay != null)
        {
            heatmapDisplay.texture = heatmapTexture;
        }
    }

    private void ListenForMessages()
    {
        try
        {
            udpClient = new UdpClient(5002);
            remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            Debug.Log("Listening for UDP packets...");
        }

        catch (SocketException se)
        {
            Debug.LogError($"Socket exception: {se.Message}");
            // Optionally attempt to reconnect or log the issue
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to set up UDP client: {e.Message}");
        }

    }
    // Start is called before the first frame update
    private void Update()
    {
        // Receive data from the UDP client
        while (udpClient != null && udpClient.Available > 0)
        {
            try
            {
                byte[] receivedData = udpClient.Receive(ref remoteEndPoint);

                if (receivedData.Length == 8)
                {
                    // Assume it's gaze positions
                    int x_screen = BitConverter.ToInt32(receivedData, 0);
                    int y_screen = BitConverter.ToInt32(receivedData, 4);

                    latestScreenPosition = new Vector2Int(x_screen, y_screen);
                    hasNewData = true;
                }
                else
                {
                    string messagePart = Encoding.UTF8.GetString(receivedData);

                    if (messagePart == "<END>")
                    {
                        // Frame complete, process the assembled image
                        try
                        {
                            byte[] imageBytes = Convert.FromBase64String(messageBuilder.ToString());
                            Texture2D texture = new Texture2D(2, 2);
                            texture.LoadImage(imageBytes);
                            rawImage.texture = texture;
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Error decoding image: {e.Message}");
                        }
                        messageBuilder.Clear();
                    }
                    else
                    {
                        // Append the message part to the message builder
                        messageBuilder.Append(messagePart);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error processing data: {e.Message}");
            }
        }

        // Update the heatmap if we have new gaze data
        if (hasNewData && isBrushActive)
        {
            // Map latestScreenPosition to heatmap coordinates
            int xHeatmap = Mathf.Clamp(latestScreenPosition.x * heatmapWidth / Screen.width, 0, heatmapWidth - 1);
            int yHeatmap = Mathf.Clamp(latestScreenPosition.y * heatmapHeight / Screen.height, 0, heatmapHeight - 1);

            // Apply brush to the heatmap
            ApplyBrush(xHeatmap, yHeatmap);

            // Apply changes to the texture
            heatmapTexture.Apply();

            hasNewData = false;
        }
    }

    private void ApplyBrush(int xCenter, int yCenter)
    {
        int radius = brushSize / 2;

        for (int x = xCenter - radius; x <= xCenter + radius; x++)
        {
            for (int y = yCenter - radius; y <= yCenter + radius; y++)
            {
                if (x >= 0 && x < heatmapWidth && y >= 0 && y < heatmapHeight)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(xCenter, yCenter));
                    if (distance <= radius)
                    {
                        float addition = intensity * (1 - (distance / radius));
                        heatmapData[x, y] += addition;

                        // Update pixel color based on heatmapData
                        float alpha = Mathf.Clamp01(heatmapData[x, y]);
                        Color color = new Color(1f, 0f, 0f, alpha); // Red color with variable transparency
                        heatmapTexture.SetPixel(x, y, color);
                    }
                }
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

        if (!isCalibrationActive)
        {
            Debug.Log("Calibration is already complete. No further commands will be sent.");
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

            // Check if this is the last step of Extra calibration
            if (currentIndex == 3)
            {
                Debug.Log("Completed all Extra calibration steps.");
                EndCalibration(); // End calibration after completing the last step
            }
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
                sendUdpClient.Send(data, data.Length, "127.0.0.1", 5001); // Python server address and port
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
        if (!isCalibrationActive)
        {
            Debug.Log("Calibration is complete. No further movements.");
            return;
        }

        //Debug.Log($"Moving to next position. CurrentIndex: {currentIndex}, IterationCount: {iterationCount}");

        // Determine the target position based on the current iteration
        Vector2 targetPosition = iterationCount < 2 ? primaryPositions[currentIndex] : extraPositions[currentIndex];

        // Check if the prefab is already at the target position
        if (targetInstance.anchoredPosition == targetPosition)
        {
            Debug.Log($"Prefab is already at position {targetPosition}. Moving to the next position.");
            currentIndex++; // Move to the next index
            if (currentIndex >= 4) // Check if we need to reset or move to the next iteration
            {
                currentIndex = 0; // Reset index
                iterationCount++; // Increment iteration count
                Debug.Log($"Iteration completed. CurrentCalibrationType: {currentCalibrationType}");

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

            // Recalculate the target position after index change
            targetPosition = iterationCount < 2 ? primaryPositions[currentIndex] : extraPositions[currentIndex];
        }

        // Calculate rotation direction
        float rotationDirection = (currentIndex % 2 == 0) ? -rotationAngle : rotationAngle;

        // Move and rotate the target
        targetInstance.DOAnchorPos(targetPosition, moveDuration).SetEase(Ease.Linear);
        targetInstance.DORotate(new Vector3(0, 0, rotationDirection), moveDuration, RotateMode.LocalAxisAdd);

        Debug.Log($"Moved to position: {targetPosition}, Rotation: {rotationDirection}");
    }

    private void EndCalibration()
    {
        Debug.Log("Ending calibration process.");
        isCalibrationActive = false;

        // Optionally destroy or deactivate the targetInstance
        if (targetInstance != null)
        {
            Destroy(targetInstance.gameObject);
            Debug.Log("Target prefab destroyed.");
        }
        else
        {
            Debug.LogWarning("Target instance already null.");
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
            Debug.Log("UDP client closed properly.");
        }

        if (sendUdpClient != null)
        {
            sendUdpClient.Close();
            Debug.Log("Sending UDP client closed properly.");
        }
    }

}

