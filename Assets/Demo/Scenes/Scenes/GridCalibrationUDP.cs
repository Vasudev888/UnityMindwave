using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class GridCalibrationUDP : MonoBehaviour
{
    #region Grid 
    [Header("Grid Settings")]
    [SerializeField] private RectTransform targetPrefab; // Prefab to instantiate and move between points
    [SerializeField] private RectTransform canvasRect;   // Canvas RectTransform to get canvas size
    [SerializeField] private float edgeMargin = 50f;     // Margin from edges to avoid truncation
    [SerializeField] private float moveDuration = 1f;    // Duration for each move, adjustable in the inspector
    [SerializeField] private float rotationAngle = 360f; // Full rotation angle for each move
    #endregion

    #region Client-related Variables
    // Client-related Variables
    private UdpClient udpClient;            // UDP client for receiving data from Python
    private UdpClient sendUdpClient;        // UDP client for sending commands to Python
    private IPEndPoint remoteEndPoint;      // Remote endpoint for UDP communication
    private Thread clientThread;            // Thread for listening to incoming UDP messages
    private StringBuilder messageBuilder = new StringBuilder(); // For assembling incoming message chunks
    #endregion


    #region Calibration-related Variables
    private RectTransform targetInstance;   // Instance of the target prefab
    private Vector2[] primaryPositions;     // Array of primary positions for calibration targets
    private Vector2[] extraPositions;       // Array of extra positions for calibration targets
    private int currentIndex = 0;           // Current index in the positions array
    private int iterationCount = 0;         // Count of calibration iterations

    private enum CalibrationType { Screen, Iris, Extra } // Types of calibration
    private CalibrationType currentCalibrationType = CalibrationType.Screen; // Default to Screen calibration
    private bool isCalibrationActive = true; // Flag to manage calibration process
    #endregion

    #region Video Display
    [Header("Video Display")]
    public RawImage rawImage;               // UI element to display the video stream
    #endregion

    #region Heatmap Settings
    [Header("Heatmap Settings")]
    public Texture2D heatmapTexture;        // Texture to store the heatmap
    public int heatmapWidth = 1280;         // Width of the heatmap texture
    public int heatmapHeight = 720;         // Height of the heatmap texture
    public int brushSize = 20;              // Size of the brush for heatmap updates
    public float intensity = 0.1f;          // Intensity of the heatmap brush
    public RawImage heatmapDisplay;         // UI element to display the heatmap
    [SerializeField] private float orangeIntensity = 1.0f; // Increase to make the transition to orange faster



    // Heatmap-related Variables
    private float[,] heatmapData;                       // 2D array to store heatmap intensity values
    private Vector2Int latestScreenPosition = Vector2Int.zero; // Latest gaze position received
    private bool hasNewData = false;                    // Flag to indicate new gaze data received
    private bool isBrushActive = false;                  // Flag to control heatmap generation
    #endregion

    #region UI Elements
    [Header("UI Elements")]
    [SerializeField] private Image gridImageResult;               // Reference to the image to be manipulated
    [SerializeField] private GameObject closeButton;    // Reference to the close button GameObject
    [SerializeField] private GameObject calibrationCompletePanel;
    //[SerializeField] private TextMeshPro calibrationCompleteText;
    #endregion

    #region Unity Lifecycle Methods
    private void Start()
    {
        // Start the UDP listener thread
        clientThread = new Thread(ListenForMessages);
        clientThread.Start();

        // Initialize the sending UdpClient
        sendUdpClient = new UdpClient();

        // Calculate primary and extra positions for calibration targets
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
        Color[] clearColors = new Color[heatmapWidth * heatmapHeight];
        for (int i = 0; i < clearColors.Length; i++)
        {
            clearColors[i] = Color.clear;
        }
        heatmapTexture.SetPixels(clearColors);
        heatmapTexture.Apply();  // Apply changes to the texture

        // Assign heatmap texture to the RawImage in the UI, if set
        if (heatmapDisplay != null)
        {
            heatmapDisplay.texture = heatmapTexture;

        }
    }


    /// <summary>
    /// Called once per frame to handle data reception and heatmap updates.
    /// </summary>
    private void Update()
    {
        // Receive data from the UDP client
        while (udpClient != null && udpClient.Available > 0)
        {
            try
            {
                // Receive data from the remote endpoint
                byte[] receivedData = udpClient.Receive(ref remoteEndPoint);

                if (receivedData.Length == 8)
                {
                    // Assume it's gaze positions (8 bytes: two 4-byte integers)
                    int x_screen = BitConverter.ToInt32(receivedData, 0);
                    int y_screen = BitConverter.ToInt32(receivedData, 4);

                    // Update the latest screen position
                    latestScreenPosition = new Vector2Int(x_screen, y_screen);
                    hasNewData = true; // Set flag to indicate new data is available
                }
                else
                {
                    // Assume it's part of the video frame data
                    string messagePart = Encoding.UTF8.GetString(receivedData);

                    if (messagePart == "<END>")
                    {
                        // Frame complete, process the assembled image
                        try
                        {
                            byte[] imageBytes = Convert.FromBase64String(messageBuilder.ToString());
                            Texture2D texture = new Texture2D(2, 2);
                            texture.LoadImage(imageBytes);
                            rawImage.texture = texture; // Update the RawImage with the new texture
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Error decoding image: {e.Message}");
                        }
                        messageBuilder.Clear(); // Clear the message builder for the next frame
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

            // Apply brush to the heatmap at the gaze position
            ApplyBrush(xHeatmap, yHeatmap);

            // Apply changes to the heatmap texture
            heatmapTexture.Apply();

            hasNewData = false; // Reset the flag after processing
        }
    }

    #endregion


    #region Network Communication Methods
    // ===================================
    // Network Communication Methods
    // ===================================

    /// <summary>
    /// Initializes the UDP client for receiving data from Python.
    /// </summary>
    private void ListenForMessages()
    {
        try
        {
            // Initialize the UDP client to listen on port 5002
            udpClient = new UdpClient(5002);

            // Set up the remote endpoint to receive data from any IP address on any port
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
    #endregion

    #region Heatmap Generation Method
    // ===================================
    // Heatmap Generation Method
    // ===================================

    /// <summary>
    /// Applies a brush effect to the heatmap at the specified coordinates.
    /// </summary>
    /// <param name="xCenter">X-coordinate of the brush center.</param>
    /// <param name="yCenter">Y-coordinate of the brush center.</param>
    /*private void ApplyBrush(int xCenter, int yCenter)
    {
        int radius = brushSize / 2;

        // Loop through a square area around the center point
        for (int x = xCenter - radius; x <= xCenter + radius; x++)
        {
            for (int y = yCenter - radius; y <= yCenter + radius; y++)
            {
                // Check if the coordinates are within the heatmap bounds
                if (x >= 0 && x < heatmapWidth && y >= 0 && y < heatmapHeight)
                {
                    // Calculate the distance from the center
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(xCenter, yCenter));
                    if (distance <= radius)
                    {
                        // Calculate intensity addition based on distance
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
*/
    private void ApplyBrush(int xCenter, int yCenter)
    {
        int radius = brushSize / 2;
       

        // Choose a sigma value for Gaussian spread (adjust to taste).
        // A third of the radius is a common choice, but you can tweak this.
        float sigma = radius / 3f;
        float twoSigmaSquare = 2 * sigma * sigma;

        for (int x = xCenter - radius; x <= xCenter + radius; x++)
        {
            for (int y = yCenter - radius; y <= yCenter + radius; y++)
            {
                // Check if the coordinates are within the heatmap bounds
                if (x >= 0 && x < heatmapWidth && y >= 0 && y < heatmapHeight)
                {
                    // Calculate the distance from the center   
                    float dx = x - xCenter;
                    float dy = y - yCenter;
                    float distanceSquare = dx * dx + dy * dy;
                    float distance = Mathf.Sqrt(distanceSquare);

                    if (distance <= radius)
                    {
                        // Gaussian-like intensity addition
                        float gaussian = Mathf.Exp(-distanceSquare / twoSigmaSquare);
                        float addition = intensity * gaussian;
                        heatmapData[x, y] += addition;

                        // Calculate final intensity (0 to 1)
                        float clampedIntensity = Mathf.Clamp01(heatmapData[x, y]);

                        // Starting color: green at low intensity
                        Color startColor = Color.green;
                        // Target color: orange at high intensity
                        Color targetColor = new Color(1f, 0.5f, 0f, 1f);

                        // Interpolate from green to orange based on intensity and orangeIntensity factor
                        Color finalColor = Color.Lerp(startColor, targetColor, clampedIntensity * orangeIntensity);

                        // Set the alpha to represent how "intense" this pixel is
                        finalColor.a = clampedIntensity;

                        int invertedY = (heatmapHeight - 1) - y;
                        // Update the pixel in the texture
                        heatmapTexture.SetPixel(x, invertedY, finalColor);
                    }
                }
            }
        }

        // Apply all pixel changes to the texture at once for efficiency
        heatmapTexture.Apply();
    }

    public void ToggleBrushApplication()
    {
        isBrushActive = true;  // Toggle continuous application
        Debug.Log("Brush application toggled: " + isBrushActive);
    }



    /// <summary>
    /// Enlarges the image to its full size and displays the close button.
    /// </summary>
    public void ChangeImageSize()
    {
        gridImageResult.transform.localScale = new Vector3(1, 1, 1);
        closeButton.SetActive(true);
    }

    /// <summary>
    /// Resets the image to its original size and hides the close button.
    /// </summary>
    public void CloseEnlargedPanel()
    {
        gridImageResult.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        closeButton.SetActive(false);
    }
    #endregion


    #region Public Accessor Methods
    /// <summary>
    /// Gets the latest screen position received from the gaze data.
    /// </summary>
    /// <returns>The latest screen position as a Vector2.</returns>
    public Vector2 GetLatestScreenPosition()
    {
        return latestScreenPosition;
    }
    #endregion

    #region Calibration Control Methods
    // ===================================
    // Calibration Control Methods
    // ===================================

    /// <summary>
    /// Handles the click event on the calibration target button.
    /// Decides which calibration command to send based on the current calibration type.
    /// </summary>
    private void OnTargetButtonClick()
    {
        // Determine which calibration command to send based on the current calibration type
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

        // Move to the next calibration position
        MoveToNextPosition();
    }

    // ===================================
    // Calibration Command Methods
    // ===================================

    /// <summary>
    /// Sends the appropriate screen calibration command based on the current index.
    /// Switches to Iris calibration after completing screen calibration steps.
    /// </summary>
    private void SendScreenCalibrationCommand()
    {
        // Determine the calibration command based on currentIndex
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

            // Check if this is the last step of screen calibration
            if (currentIndex == 3)
            {
                currentCalibrationType = CalibrationType.Iris; // Switch to Iris Calibration
                Debug.Log("Switching to Iris Calibration");
            }
        }
    }

    /// <summary>
    /// Sends the appropriate iris calibration command based on the current index.
    /// Switches to Extra calibration after completing iris calibration steps.
    /// </summary>
    private void SendIrisCalibrationCommand()
    {
        // Determine the calibration command based on currentIndex
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

            // Check if this is the last step of iris calibration
            if (currentIndex == 3)
            {
                currentCalibrationType = CalibrationType.Extra; // Switch to Extra Calibration
                Debug.Log("Switching to Extra Calibration");
            }
        }
    }

    /// <summary>
    /// Sends the appropriate extra calibration command based on the current index.
    /// Ends calibration after completing extra calibration steps.
    /// </summary>
    private void SendExtraCalibrationCommand()
    {
        if (!isCalibrationActive)
        {
            Debug.Log("Calibration is already complete. No further commands will be sent.");
            return;
        }

        // Determine the calibration command based on currentIndex
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

            // Check if this is the last step of extra calibration
            if (currentIndex == 3)
            {
                Debug.Log("Completed all Extra calibration steps.");
                EndCalibration(); // End calibration after completing the last step
                                  // Now show your UI message that calibration is complete
                ShowCalibrationCompleteUI();
            }
        }
    }
    #endregion

    #region Command Sending Method
    // ===================================
    // Command Sending Method
    // ===================================

    /// <summary>
    /// Sends a command string to the Python server via UDP.
    /// </summary>
    /// <param name="command">The command string to send.</param>
    private void SendCommand(string command)
    {
        if (!string.IsNullOrEmpty(command))
        {
            // Convert the command string to bytes with a newline character
            byte[] data = Encoding.UTF8.GetBytes(command + "\n");
            Debug.Log("Encoded data: " + BitConverter.ToString(data));

            try
            {
                // Send the command via UDP to the Python server at port 5001
                sendUdpClient.Send(data, data.Length, "127.0.0.1", 5001);
                Debug.Log("Sent command: " + command);
            }
            catch (Exception e)
            {
                Debug.LogError("Error sending command: " + e.Message);
            }
        }
    }

    // ===================================
    // Calibration Navigation Method
    // ===================================

    /// <summary>
    /// Moves the calibration target to the next position and handles calibration type transitions.
    /// </summary>
    private void MoveToNextPosition()
    {
        if (!isCalibrationActive)
        {
            Debug.Log("Calibration is complete. No further movements.");
            return;
        }

        // Determine the target position based on the current iteration
        Vector2 targetPosition = iterationCount < 2 ? primaryPositions[currentIndex] : extraPositions[currentIndex];

        // Check if the target is already at the desired position
        if (targetInstance.anchoredPosition == targetPosition)
        {
            Debug.Log($"Prefab is already at position {targetPosition}. Moving to the next position.");

            // Move to the next index
            currentIndex++;

            // Check if we've completed a full set of positions
            if (currentIndex >= 4)
            {
                currentIndex = 0;      // Reset index for the next set
                iterationCount++;      // Increment the iteration count
                Debug.Log($"Iteration completed. CurrentCalibrationType: {currentCalibrationType}");

                // Transition to the next calibration type if necessary
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

        // Calculate rotation direction based on the current index
        float rotationDirection = (currentIndex % 2 == 0) ? -rotationAngle : rotationAngle;

        // Move and rotate the target to the new position
        targetInstance.DOAnchorPos(targetPosition, moveDuration).SetEase(Ease.Linear);
        targetInstance.DORotate(new Vector3(0, 0, rotationDirection), moveDuration, RotateMode.LocalAxisAdd);

        Debug.Log($"Moved to position: {targetPosition}, Rotation: {rotationDirection}");
    }
    #endregion

    #region Calibration Completion Method
    // ===================================
    // Calibration Completion Method
    // ===================================

    /// <summary>
    /// Ends the calibration process and cleans up the target instance.
    /// </summary>
    private void EndCalibration()
    {
        Debug.Log("Ending calibration process.");
        isCalibrationActive = false;

        // Destroy the target prefab instance if it exists
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


    private void ShowCalibrationCompleteUI()
    {
        if (calibrationCompletePanel != null)
        {
            // Set the panel active to show it
            calibrationCompletePanel.SetActive(true);

   /*         if (calibrationCompleteText != null)
            {
                calibrationCompleteText.text = "Calibration is complete.\nClick on Close button to proceed Next.";
            }*/

          /*  if (closeCalibrationButton != null)
            {
                // Assign a listener to the close button (if not already assigned)
                closeCalibrationButton.onClick.RemoveAllListeners();
                closeCalibrationButton.onClick.AddListener(CloseCalibrationCompleteUI);
            }*/
        }
    }

    #endregion

    #region Cleanup Method
    // ===================================
    // Cleanup Method
    // ===================================

    /// <summary>
    /// Cleans up resources when the application is closing.
    /// </summary>
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
    #endregion

}