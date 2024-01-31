using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using M2MqttUnity;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using TMPro;
using MQTTDataCompasXR;
using System.Security.Claims;

public class MqttTrajectoryReceiver : M2MqttUnityClient
{
    [Header("MQTT Settings")]
    [Tooltip("Set the topic to publish")]

    // Properties and events
    private string m_msg;
    
    //Compas XR Topics
    public CompasXRTopics compasXRTopics;
    
    public string msg
    {
        get { return m_msg; }
        set
        {
            if (m_msg == value) return;
            m_msg = value;
            OnMessageArrived?.Invoke(m_msg);
        }
    }
    public event OnMessageArrivedDelegate OnMessageArrived;
    public delegate void OnMessageArrivedDelegate(string newMsg);

    private bool m_isConnected;
    public bool isConnected
    {
        get { return m_isConnected; }
        set
        {
            if (m_isConnected == value) return;
            m_isConnected = value;
            OnConnectionSucceeded?.Invoke(isConnected);
        }
    }
    public event OnConnectionSucceededDelegate OnConnectionSucceeded;
    public delegate void OnConnectionSucceededDelegate(bool isConnected);

    private List<string> eventMessages = new List<string>();


    protected override void Start()
    {
        base.Start();
                
        //Connect to MQTT Broker on start with default settings.
        Connect();

        // client.IsConnected += SubscribeToCompasXRTopics;
        OnConnected();
        ConnectionSucceeded += SubscribeToCompasXRTopics;
        // ConnectionFailed += MQTTConnectionFailed;

        //TODO: CURRENTLY NO EVENT IN THE LIBRARY, BUT COULD BE ADDED?
        // OnDisconnected();

    }
    protected override void Update()
    {
        base.Update(); // call ProcessMqttEvents()
    }

    public void OnDestroy()
    {
        Disconnect();
    }
    
    //////////////////////////////////////////// General Methods ////////////////////////////////////////////
    
    //////////////////////////////////////////// Topic Managers /////////////////////////////////////////////
    public void SetCompasXRTopics(object source, ApplicationSettingsEventArgs e)
    {
        //Set Compas_XR Topics Information from the project name
        compasXRTopics = new CompasXRTopics(e.Settings.parentname);
    }
    private void SubscribeToTopic(string topicToSubscribe)
    {
        if (!string.IsNullOrEmpty(topicToSubscribe) || client == null)
        {
            Debug.Log("Subscribing to topic: " + topicToSubscribe);
            client.Subscribe(new string[] { topicToSubscribe }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }
        else
        {
            Debug.LogError("MQTT: Topic to subscribe is empty or client is null.");
        }
    }

    private void UnsubscribeToTopic(string topicToUnsubscribe)
    {
        if (!string.IsNullOrEmpty(topicToUnsubscribe))
        {
            client.Unsubscribe(new string[] { topicToUnsubscribe });
            Debug.Log("Unsubscribed from topic: " + topicToUnsubscribe);
        }
        else
        {
            Debug.LogWarning("MQTT: Topic to unsubscribe is empty.");
        }
    }
    public void SubscribeToCompasXRTopics()
    {
        //Subscribe to Compas XR Get Trajectory Topics
        SubscribeToTopic(compasXRTopics.subscribers.getTrajectoryResultTopic);

        //Subscribe to Compas XR Approve Trajectory Topics
        SubscribeToTopic(compasXRTopics.subscribers.approveTrajectoryTopic);
    }

    public void UnsubscribeToCompasXRTopics()
    {
        //Unsubscribe to Compas XR Get Trajectory Topics
        UnsubscribeToTopic(compasXRTopics.subscribers.getTrajectoryResultTopic);

        //Unsubscribe to Compas XR Approve Trajectory Topics
        UnsubscribeToTopic(compasXRTopics.subscribers.approveTrajectoryTopic);
    }  

    //////////////////////////////////////////// Message Managers ////////////////////////////////////////////
    protected override void DecodeMessage(string topic, byte[] message)
    {
        msg = System.Text.Encoding.UTF8.GetString(message);
        Debug.Log("Received: " + msg + " from topic: " + topic);

        //TODO: ADD MESSAGE HANDLER HERE BASED ON TOPIC NAME
        
        StoreMessage(msg);
    }

    private void StoreMessage(string eventMsg)
    {
        if (eventMessages.Count > 50) eventMessages.Clear();
        eventMessages.Add(eventMsg);
    }


}

