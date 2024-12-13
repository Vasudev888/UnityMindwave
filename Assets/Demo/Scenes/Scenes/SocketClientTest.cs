using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class SocketClientTest : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private Thread clientThread;

    [Header("Command Buttons")]
    public Button calibrateScreenLeftButton;
    public Button calibrateScreenRightButton;
    public Button calibrateScreenTopButton;
    public Button calibrateScreenBottomButton;

    public Button calibrateIrisLeftButton;
    public Button calibrateIrisRightButton;
    public Button calibrateIrisTopButton;
    public Button calibrateIrisBottomButton;

    public Button calibrateExtraLeftButton;
    public Button calibrateExtraRightButton;
    public Button calibrateExtraTopButton;
    public Button calibrateExtraBottomButton;

    public Button stopCalibrationButton;

    void Start()
    {
        clientThread = new Thread(new ThreadStart(ConnectToServer));
        clientThread.Start();

        // Screen calibration buttons
        if (calibrateScreenLeftButton) calibrateScreenLeftButton.onClick.AddListener(() => SendCommand("calibrate_screen_left"));
        if (calibrateScreenRightButton) calibrateScreenRightButton.onClick.AddListener(() => SendCommand("calibrate_screen_right"));
        if (calibrateScreenTopButton) calibrateScreenTopButton.onClick.AddListener(() => SendCommand("calibrate_screen_top"));
        if (calibrateScreenBottomButton) calibrateScreenBottomButton.onClick.AddListener(() => SendCommand("calibrate_screen_bottom"));

        // Iris calibration buttons
        if (calibrateIrisLeftButton) calibrateIrisLeftButton.onClick.AddListener(() => SendCommand("calibrate_iris_left"));
        if (calibrateIrisRightButton) calibrateIrisRightButton.onClick.AddListener(() => SendCommand("calibrate_iris_right"));
        if (calibrateIrisTopButton) calibrateIrisTopButton.onClick.AddListener(() => SendCommand("calibrate_iris_top"));
        if (calibrateIrisBottomButton) calibrateIrisBottomButton.onClick.AddListener(() => SendCommand("calibrate_iris_bottom"));

        // Extra calibration buttons
        if (calibrateExtraLeftButton) calibrateExtraLeftButton.onClick.AddListener(() => SendCommand("calibrate_extra_left"));
        if (calibrateExtraRightButton) calibrateExtraRightButton.onClick.AddListener(() => SendCommand("calibrate_extra_right"));
        if (calibrateExtraTopButton) calibrateExtraTopButton.onClick.AddListener(() => SendCommand("calibrate_extra_top"));
        if (calibrateExtraBottomButton) calibrateExtraBottomButton.onClick.AddListener(() => SendCommand("calibrate_extra_bottom"));

        // Stop calibration button
        if (stopCalibrationButton) stopCalibrationButton.onClick.AddListener(() => SendCommand("stop_calibration"));
    }

    private void ConnectToServer()
    {
        try
        {
            client = new TcpClient("127.0.0.1", 65432);
            stream = client.GetStream();
            Debug.Log("Connected to Python server!");
        }
        catch (Exception e)
        {
            Debug.LogError("Socket error: " + e.Message);
        }
    }



    private void SendCommand(string command)
    {
        if (client != null && client.Connected && stream != null)
        {
            try
            {
                byte[] commandBytes = Encoding.UTF8.GetBytes(command + "\n");
                stream.Write(commandBytes, 0, commandBytes.Length);
                Debug.Log("Sent command: " + command);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to send command: " + e.Message);
            }
        }
        else
        {
            Debug.LogWarning("Not connected to the server.");
        }
    }

    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
        clientThread?.Abort();
    }
}
