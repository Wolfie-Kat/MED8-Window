using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class UnityPythonConnector : MonoBehaviour
{
    public int port = 8888;
    public Vector2 receivedValue = Vector2.zero;
    
    private UdpClient _udpClient;
    private Thread _receiveThread;
    private volatile float _latestX;
    private volatile float _latestY;
    private volatile bool _hasNewData = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeUDP();
    }

    // Update is called once per frame
    void Update()
    {
        if (_hasNewData)
        {
            receivedValue = new Vector2(_latestX, _latestY);
            _hasNewData = false;
            
            Debug.Log($"Received: {receivedValue.x}, {receivedValue.y}");
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
                if (data.Length >= 8)
                {
                    _latestX = BitConverter.ToSingle(data, 0);
                    _latestY = BitConverter.ToSingle(data, 4);
                    _hasNewData = true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("UDP receive error: " + e.Message);
            }
        }
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