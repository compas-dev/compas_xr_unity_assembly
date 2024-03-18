using System;
using System.Threading;
using RosSharp.RosBridgeClient.Protocols;
using UnityEngine;
using RosSharp.RosBridgeClient;

public class RosConnectionManager : MonoBehaviour
{
    public int SecondsTimeout = 10;

    public RosSocket RosSocket { get; private set; }
    public RosSocket.SerializerEnum Serializer;
    public Protocol protocol;
    public string RosBridgeServerUrl = "ws://192.168.0.1:9090";

    public ManualResetEvent IsConnected { get; private set; }
    public bool IsConnectedToRos { get { return IsConnected.WaitOne(0); } }

    public virtual void Awake()
    {
        IsConnected = new ManualResetEvent(false);
        //     new Thread(ConnectAndWait).Start();
    }

    public void ConnectAndWait()
    {
        RosSocket = ConnectToRos(protocol, RosBridgeServerUrl, OnConnected, OnClosed, Serializer);

        if (!IsConnected.WaitOne(SecondsTimeout * 1000))
            Debug.LogWarning("Failed to connect to RosBridge at: " + RosBridgeServerUrl);
    }

    public static RosSocket ConnectToRos(Protocol protocolType, string serverUrl, EventHandler onConnected = null, EventHandler onClosed = null, RosSocket.SerializerEnum serializer = RosSocket.SerializerEnum.Microsoft)
    {
        IProtocol protocol = ProtocolInitializer.GetProtocol(protocolType, serverUrl);
        protocol.OnConnected += onConnected;
        protocol.OnClosed += onClosed;

        return new RosSocket(protocol, serializer);
    }

    private void OnApplicationQuit()
    {
        if(RosSocket != null && IsConnectedToRos)
        {
            RosSocket.Close();
        }
    }

    private void OnConnected(object sender, EventArgs e)
    {
        IsConnected.Set();
        Debug.Log("Connected to RosBridge: " + RosBridgeServerUrl);
    }

    private void OnClosed(object sender, EventArgs e)
    {
        IsConnected.Reset();
        Debug.Log("Disconnected from RosBridge: " + RosBridgeServerUrl);
    }
}