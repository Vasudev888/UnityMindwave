using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using static UnityEngine.GridBrushBase;

public class GridCalibrationUDP : MonoBehaviour
{
    #region Grid 
    [Header("Grid Settings")]
    [SerializeField] private RectTransform targetPrefab; // Prefab to instantiate and move between points
    [SerializeField] private RectTransform canvasRect;   // Canvas RectTransform to get canvas size
    [SerializeField] private float edgeMargin = 50f;     // Margin from edges to avoid truncation
    [SerializeField] private float moveDuration = 2f;    // Duration for each move, adjustable in the inspector
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
    public float intensity = 0.05f;         // Intensity of the heatmap brush (reduced to make red harder to reach)
    public RawImage heatmapDisplay;         // UI element to display the heatmap
    [SerializeField] private float orangeIntensity = 1.0f; // Increase to make the transition to orange faster
    [SerializeField] MindwaveDataVisualizerNew mindwaveDataVisualizer;
    [Header("Color Ramp (Yellow→Green→Red)")]
    public Gradient colorGradient;
    [Header("Heatmap Color Settings")]
    [SerializeField] private Color startColor = Color.yellow;      // Starting color for low intensity
    [SerializeField] private Color midColor = Color.green;         // Middle color
    [SerializeField] private Color endColor = Color.red;           // End color for high intensity (revisits)
    [SerializeField] private float redThreshold = 0.80f;           // Minimum intensity to show red (nucleus effect)



    // Heatmap-related Variables
    private float[,] heatmapData;                       // 2D array to store heatmap intensity values
    private Vector2Int latestScreenPosition = Vector2Int.zero; // Latest gaze position received
    private bool hasNewData = false;                    // Flag to indicate new gaze data received
    private bool isBrushActive = false;                  // Flag to control heatmap generation
    private float decayRate = 0.01f;                    // Rate at which heatmap intensity decays over time
    private float lastDecayTime = 0f;                   // Time of last decay update
    private float decayInterval = 0.1f;                 // Interval between decay updates (in seconds)
    #endregion

    #region UI Elements
    [Header("UI Elements")]
    [SerializeField] private Image gridImageResult;               // Reference to the image to be manipulated
    [SerializeField] private GameObject closeButton;    // Reference to the close button GameObject
    [SerializeField] private GameObject calibrationCompletePanel;
    [SerializeField] private Image progressFillImage; // Assign your fill image in the inspector
    [SerializeField] private int totalTurns = 12;
    private int currentTurn = 0;
    public AudioSource audioSource;
    public AudioClip Clip1, Clip2;
    
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
        /*        Button targetButton = targetInstance.GetComponent<Button>();
                if (targetButton != null)
                {
                    targetButton.onClick.AddListener(OnTargetButtonClick);
                    Debug.Log("Button Clicked: " + targetButton);
                }*/

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

        progressFillImage = targetPrefab.GetChild(0).GetComponent<Image>();
        progressFillImage.fillAmount = 0f;

    }

    public void OnClickStartCalibration()
    {
        BeginCalibration();
        audioSource.clip = Clip1;
        audioSource.Play();

    }


    /// <summary>
    /// Called once per frame to handle data reception and heatmap updates.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // Press Space to simulate progress
        {
            currentTurn = Mathf.Clamp(currentTurn + 1, 0, totalTurns);

            if (progressFillImage != null)
            {
                progressFillImage.fillAmount += 0.1f;
               // Debug.Log($"Fill Amount Updated: {fillAmount}");
            }
        }
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

            float attentionValuez = mindwaveDataVisualizer.GetAttentionValue();
            // Apply brush to the heatmap at the gaze position
            ApplyBrush(xHeatmap, yHeatmap, attentionValuez);

            // Apply changes to the heatmap texture
            heatmapTexture.Apply();

            hasNewData = false; // Reset the flag after processing
        }

        // Apply decay to heatmap data periodically
        if (isBrushActive && Time.time - lastDecayTime >= decayInterval)
        {
            ApplyHeatmapDecay();
            lastDecayTime = Time.time;
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

    /*private void ApplyBrush(int xCenter, int yCenter, float attentionvalue)
    {
        if (attentionvalue <= 30)
            return;

        int radius = brushSize / 2;

        // Sigma value for Gaussian spread
        float sigma = radius / 3f;
        float twoSigmaSquare = 2 * sigma * sigma;

        for (int x = xCenter - radius; x <= xCenter + radius; x++)
        {
            for (int y = yCenter - radius; y <= yCenter + radius; y++)
            {
                // Ensure coordinates are within bounds
                if (x >= 0 && x < heatmapWidth && y >= 0 && y < heatmapHeight)
                {
                    // Distance from the brush center
                    float dx = x - xCenter;
                    float dy = y - yCenter;
                    float distanceSquare = dx * dx + dy * dy;

                    if (distanceSquare <= radius * radius)
                    {
                        // Gaussian intensity calculation
                        float gaussian = Mathf.Exp(-distanceSquare / twoSigmaSquare);
                        float addition = intensity * gaussian;
                        //heatmapData[x, y] += addition;
                        heatmapData[x, y] = Mathf.Clamp01(heatmapData[x, y] + addition);
                        Color finalColor;

                        // Clamp the intensity between 0 and 1
                        float clampedIntensity = Mathf.Clamp01(heatmapData[x, y]);

                        // Color interpolation from green to orange
                        Color startColor = Color.green;  // Starting color at low intensity
                        Color targetColor = new Color(1f, 0.5f, 0f, 1f);  // Orange at high intensity
                        finalColor = Color.Lerp(startColor, targetColor, clampedIntensity);

                        // Ensure alpha is proportional to intensity for smooth blending
                        finalColor.a = clampedIntensity;
                        int invertedY = (heatmapHeight - 1) - y;

                        // Update heatmap texture pixel
                        heatmapTexture.SetPixel(x, invertedY, finalColor);
                    }
                }
            }
        }

        // Apply changes to the texture
        heatmapTexture.Apply();
    }*/


    /*private void ApplyBrush(int xCenter, int yCenter, float attentionvalue)
    {
*//*        if (attentionvalue <= 30)
            return;*//*

        int radius = brushSize / 2;

        // Sigma value for Gaussian spread
        float sigma = radius / 4f;
        float twoSigmaSquare = 2 * sigma * sigma;

        for (int x = xCenter - radius; x <= xCenter + radius; x++)
        {
            for (int y = yCenter - radius; y <= yCenter + radius; y++)
            {
                // Ensure coordinates are within bounds
                if (x >= 0 && x < heatmapWidth && y >= 0 && y < heatmapHeight)
                {
                    // Distance from the brush center
                    float dx = x - xCenter;
                    float dy = y - yCenter;
                    float distanceSquare = dx * dx + dy * dy;

                    if (distanceSquare <= radius * radius)
                    {
                        // Gaussian intensity calculation
                        float gaussian = Mathf.Exp(-distanceSquare / twoSigmaSquare);
                        //float addition = intensity * gaussian;
                        float addition = intensity * gaussian;

                        // Update heatmap data with clamped value
                        heatmapData[x, y] = Mathf.Clamp01(heatmapData[x, y] + addition) ;

                        // Clamp the intensity between 0 and 1
                        float clampedIntensity = Mathf.Clamp01(heatmapData[x, y]);

                        // Multi-step color interpolation
                        Color finalColor = colorGradient.Evaluate(clampedIntensity);
                        finalColor.a = clampedIntensity;
                        if (clampedIntensity < 0.5f)
                        {
                            // Green to Yellow transition (0.0 to 0.5)
                            float t = clampedIntensity / 0.5f;
                            finalColor = Color.Lerp(Color.green, Color.yellow, t);
                        }
                        else
                        {
                            // Yellow to Orange transition (0.5 to 1.0)
                            float t = (clampedIntensity - 0.5f) / 0.5f;
                            Color orange = new Color(1f, 0.5f, 0f);
                            finalColor = Color.Lerp(Color.yellow, orange, t);
                        }

                        // Make alpha match the intensity for smoother blending
                        finalColor.a = clampedIntensity;

                        // Invert Y to match Unity texture coordinates
                        int invertedY = (heatmapHeight - 1) - y;

                        // Update the texture pixel
                        heatmapTexture.SetPixel(x, invertedY, finalColor);
                    }
                }
            }
        }

        // Apply changes to the texture
        heatmapTexture.Apply();
    }*/

    private Color EvaluateRainbow(float t)
    {
        // 5 stops: blue, cyan, green, yellow, red
        if (t < 0.30f)
        {
            // blue → cyan
            return Color.Lerp(Color.yellow, Color.green, t / 0.30f);
        }
        else if (t < 0.60f)
        {
            // cyan → green
            return Color.Lerp(Color.yellow, Color.green, (t - 0.30f) / 0.30f);
        }
        else if (t < 0.85f)
        {
            // green → yellow
            return Color.Lerp(Color.green, Color.yellow, (t - 0.60f) / 0.25f);
        }
        else
        {
            // yellow → red (only 15% of the ramp)
            return Color.Lerp(Color.yellow, Color.red, (t - 0.75f) / 0.25f);
        }
    }

    private Color EvaluateEnhancedRainbow(float t)
    {
        // Color mapping: yellow → green → red (for revisits)
        // Normalize t to 0-1 range for the base colors
        float normalizedT = Mathf.Clamp01(t);
        
        if (normalizedT < redThreshold)
        {
            // yellow → green (most of the range)
            return Color.Lerp(startColor, midColor, normalizedT / redThreshold);
        }
        else
        {
            // green → red (only in nucleus - very small area)
            return Color.Lerp(midColor, endColor, (normalizedT - redThreshold) / (1f - redThreshold));
        }
    }


    private void ApplyBrush(int xCenter, int yCenter, float attentionvalue)
    {
        int radius = brushSize / 2;
        // Make the Gaussian more concentrated by reducing sigma
        float sigma = radius / 6f; // More concentrated than before
        float twoSigmaSq = 2f * sigma * sigma;

        for (int x = xCenter - radius; x <= xCenter + radius; x++)
            for (int y = yCenter - radius; y <= yCenter + radius; y++)
            {
                if (x < 0 || x >= heatmapWidth || y < 0 || y >= heatmapHeight) continue;
                float dx = x - xCenter, dy = y - yCenter;
                float d2 = dx * dx + dy * dy;
                if (d2 > radius * radius) continue;

                // More concentrated Gaussian distribution
                float g = Mathf.Exp(-d2 / twoSigmaSq);
                
                // Apply distance-based intensity reduction for more nucleus-like effect
                float distanceFromCenter = Mathf.Sqrt(d2);
                float distanceFactor = Mathf.Clamp01(1f - (distanceFromCenter / radius));
                g *= distanceFactor * distanceFactor; // Square the distance factor for sharper falloff
                
                // Calculate intensity multiplier based on current heatmap value
                // Higher existing values get higher multipliers to intensify revisits
                float currentIntensity = heatmapData[x, y];
                float intensityMultiplier = 1f + (currentIntensity * 2f); // Stronger intensification for revisits
                
                // Add intensity with multiplier, but allow values to exceed 1.0 for more dramatic effects
                float newIntensity = currentIntensity + (intensity * g * intensityMultiplier);
                heatmapData[x, y] = Mathf.Clamp(newIntensity, 0f, 1.0f); // Keep values within 0-1 range

                float t = heatmapData[x, y];

                // Enhanced rainbow mapping that shows more red for higher intensities
                Color col = EvaluateEnhancedRainbow(t);
                col.a = Mathf.Clamp01(t); // Keep alpha between 0 and 1

                int invY = heatmapHeight - 1 - y;
                heatmapTexture.SetPixel(x, invY, col);
            }

        heatmapTexture.Apply();
    }

    /// <summary>
    /// Applies gradual decay to heatmap data to prevent indefinite accumulation
    /// </summary>
    private void ApplyHeatmapDecay()
    {
        bool needsUpdate = false;
        
        for (int x = 0; x < heatmapWidth; x++)
        {
            for (int y = 0; y < heatmapHeight; y++)
            {
                if (heatmapData[x, y] > 0f)
                {
                    // Apply decay
                    heatmapData[x, y] = Mathf.Max(0f, heatmapData[x, y] - decayRate);
                    
                    // Update texture pixel
                    float t = heatmapData[x, y];
                    Color col = EvaluateEnhancedRainbow(t);
                    col.a = Mathf.Clamp01(t);
                    
                    int invY = heatmapHeight - 1 - y;
                    heatmapTexture.SetPixel(x, invY, col);
                    needsUpdate = true;
                }
            }
        }
        
        if (needsUpdate)
        {
            heatmapTexture.Apply();
        }
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
    /// 

    private void BeginCalibration()
    {
        // Start from the first position
        currentCalibrationType = CalibrationType.Screen;
        currentIndex = -1;
        iterationCount = 0;
        isCalibrationActive = true;
        Debug.Log("yYyyyyyyyyyyyyyyyyyyyy");
        //SendScreenCalibrationCommand();
        MoveToNextPosition();
    }


    // Called when the tween finishes
    private void OnMovementComplete()
    {
        // Send the appropriate calibration command for the current type and index
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

        // Now move to the next position if calibration is still ongoing
        if (isCalibrationActive)
        {
            MoveToNextPosition();
        }
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
                audioSource.clip = Clip2;
                audioSource.loop = false;
                audioSource.Play();
            }
            catch (Exception e)
            {
                Debug.LogError("Error sending command: " + e.Message);
            }
        }
        else
        {
            audioSource.clip = Clip1; 
            audioSource.Play(); 
            audioSource.loop = true;
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
        currentIndex++;

        #region to fill UI progress bar
        // Increment the current turn count
        //Debug.Log(" progressFillImage.fillAmount +++++++++++++++++++++++++++++++++++++++ " + progressFillImage.fillAmount);
        currentTurn = Mathf.Clamp(currentTurn + 1, 0, totalTurns); // Ensure it doesn't exceed totalTurns

        // Update the fill image
        if (progressFillImage != null)
        {
            float fillAmount = (float)currentTurn / totalTurns;
            progressFillImage.fillAmount = fillAmount; // Update fill amount
        }

        // Check if the cycle is complete
        if (currentTurn >= totalTurns)
        {
            Debug.Log("Cycle complete!");
            // Optionally, trigger some action here when the cycle completes
        }
        #endregion



        if (!isCalibrationActive)
        {
            Debug.Log("Calibration is complete. No further movements.");
            return;
        }
        else
        {
            Debug.Log("Active........");
        }

        // Determine the target position based on the current iteration
        //Vector2 targetPosition = iterationCount < 2 ? primaryPositions[currentIndex] : extraPositions[currentIndex];

        // Check if the target is already at the desired position
/*        if (targetInstance.anchoredPosition == targetPosition)
        {
            Debug.Log($"Prefab is already at position {targetPosition}. Moving to the next position.");

            // Move to the next index

            currentIndex++;*/
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
                iterationCount = 0; // Reset iteration count for the next calibration type
            }
        }

        Vector2 targetPosition;
        switch (currentCalibrationType)
        {
            case CalibrationType.Screen:
            case CalibrationType.Iris:
                // For Screen and Iris, use primaryPositions
                targetPosition = primaryPositions[currentIndex];
                break;

            case CalibrationType.Extra:
                // For Extra calibration, use extraPositions
                targetPosition = extraPositions[currentIndex];
                break;

            default:
                // Fallback to primaryPositions if something unexpected happens
                targetPosition = primaryPositions[currentIndex];
                break;
        }

        // Recalculate the target position after index change
        //targetPosition = iterationCount < 2 ? primaryPositions[currentIndex] : extraPositions[currentIndex];

        #region original code
        // Calculate rotation direction based on the current index
        //float rotationDirection = (currentIndex % 2 == 0) ? -rotationAngle : rotationAngle;

        //// Move and rotate the target to the new position
        //targetInstance.DOAnchorPos(targetPosition, moveDuration).SetEase(Ease.Linear).OnComplete(() => OnMovementComplete()); // Call OnMovementComplete when movement finishes

        //targetInstance.DORotate(new Vector3(0, 0, rotationDirection), moveDuration, RotateMode.LocalAxisAdd);

        //Debug.Log($"Moved to position: {targetPosition}, Rotation: {rotationDirection}");

        #endregion

        #region Nidhi UI Modification

        // Calculate movement direction and set facing angle
        Vector2 currentPos = targetInstance.anchoredPosition;
        Vector2 direction = targetPosition - currentPos;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Adjust scale based on movement direction
        if (Mathf.Abs(direction.y)==0)
        {
            targetInstance.localScale = new Vector3(1, 1, 1); 
        }
        else
        {
          
            targetInstance.localScale = new Vector3(1, -1, 1); 
        }

        // Adjust bird's facing direction
        targetInstance.rotation = Quaternion.Euler(0, 0, angle); 

        // Move the target to the new position
        targetInstance.DOAnchorPos(targetPosition, moveDuration).SetEase(Ease.Linear).OnComplete(() => OnMovementComplete()); // Call OnMovementComplete when movement finishes
       #endregion


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