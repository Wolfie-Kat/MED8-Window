using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;

public class UnityPythonConnector : MonoBehaviour
{
    public int port = 8888;
    public Vector2 receivedValue = Vector2.zero;
    public bool recentering;
    
    private UdpClient _udpClient;
    private Thread _receiveThread;
    private volatile float _latestX;
    private volatile float _latestY;
    public float _FOV;
    [SerializeField] private volatile bool hasNewData = false;
    [SerializeField] private float recenterTimeLimit;
    [SerializeField] private float timer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeUDP();
    }

    // Update is called once per frame
    void Update()
    {
        if (hasNewData)
        {
            timer = 0f;
            recentering = false;
            RecenterView(100, _latestX, _latestY); // Use for interpolation
            //receivedValue = new Vector2(_latestX, _latestY); // Use for pure data
            hasNewData = false;
        }
        else if (!hasNewData && recentering == false)
        {
            timer += Time.deltaTime;
        }

        if (timer >= recenterTimeLimit)
        {
            recentering = true;
            RecenterView(2f);
        }
    }

    private void InitializeUDP()
    {
        _udpClient = new UdpClient(port);
        _receiveThread = new Thread(PythonReceiver)
        {
            IsBackground = true
        };
        _receiveThread.Start();
        print("UDP port open!");
    }

    private void PythonReceiver()
    {
        IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);

        while (true)
        {
            try
            {
                byte[] data = _udpClient.Receive(ref anyIP);
                if (data.Length >= 12)
                {
                    _latestX = BitConverter.ToSingle(data, 0);
                    _latestY = BitConverter.ToSingle(data, 4);
                    _FOV = BitConverter.ToSingle(data, 8);
                    hasNewData = true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("UDP receive error: " + e.Message);
            }
        }
    }

    private void RecenterView(float recenterSpeed = 1, float endPointX = 0.5f, float endPointY = 0.5f)
    {
        if (recentering == false)
        {
            _latestX = Mathf.MoveTowards(receivedValue.x, endPointX, Time.deltaTime * recenterSpeed);
            _latestY = Mathf.MoveTowards(receivedValue.y, endPointY, Time.deltaTime * recenterSpeed);
        }
        else
        {
            _latestX = Mathf.Lerp(receivedValue.x, endPointX, Time.deltaTime * recenterSpeed);
            _latestY = Mathf.Lerp(receivedValue.y, endPointY, Time.deltaTime * recenterSpeed);
        }
        receivedValue = new Vector2(_latestX, _latestY);
    }
    
    private void OnDestroy()
    {
        if (_receiveThread != null)
        {
            _receiveThread.Abort();
        }
        
        _udpClient?.Close();
    }
}