using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using M2MqttUnity;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using TMPro;
using MQTTDataCompasXR;
using Newtonsoft.Json;
using System.Security.Claims;

public class MqttTrajectoryReceiver : M2MqttUnityClient
{
    [Header("MQTT Settings")]
    [Tooltip("Set the topic to publish")]

    // Properties and events
    private string m_msg;    
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

    private List<string> eventMessages = new List<string>();

    //Compas XR Topics Class
    public CompasXRTopics compasXRTopics;

    //Other Scripts
    public UIFunctionalities UIFunctionalities;

    protected override void Start()
    {
        //Calling Base Start is a property of Inheritance from M2MqttUnityClient. Ensures that the base class is called First.
        base.Start();

        //On Start Initialization
        OnStartInitilization();

    }
    protected override void Update()
    {
        //Calling Base Update is a property of Inheritance from M2MqttUnityClient. Ensures that the base class is called First.
        base.Update();
    }

    public void OnDestroy()
    {
        Disconnect();
    }
    
    //////////////////////////////////////////// General Methods ////////////////////////////////////////////
    public void OnStartInitilization()
    {
        //Find UI Functionalities
        UIFunctionalities = GameObject.Find("UIFunctionalities").GetComponent<UIFunctionalities>();
        
        //Connect to MQTT Broker on start with default settings.
        Connect();

        //Add event listners
        AddConnectionEventListners();
    }

    //////////////////////////////////////////// Connection Managers /////////////////////////////////////////////
    //Methods Overriden from M2MqttUnityClient Class //TODO: Not sure if I need these. Might be useful. OnConnectionLost is helpful if it works though.
    protected override void OnConnected()
    {
        base.OnConnected();
        Debug.Log("MQTT: ON CONNECTED INTERNAL METHOD");
    }
    protected override void OnDisconnected()
    {
        Debug.Log("MQTT: ON DISCONNECTED INTERNAL METHOD.");
        //I dont think we need the is connected bool.
        // isConnected=false;
    }
    protected override void OnConnectionLost() //I am not sure if this is working?
    {
        Debug.Log("MQTT: CONNECTION LOST!");
        //TODO: ADD ON SCREEN MESSAGE... I dont think we need the is connected bool.
        // isConnected=false;
    }

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
            Debug.Log("MQTT: Subscribing to topic: " + topicToSubscribe);
            client.Subscribe(new string[] { topicToSubscribe }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }
        else
        {
            Debug.LogError("MQTT: Topic to subscribe is empty or client is null.");
        }
    }
    private void UnsubscribeFromTopic(string topicToUnsubscribe)
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
    public void PublishToTopic(string publishingTopic,  Dictionary<string, object> message)
    {   
        string messagePublish = JsonConvert.SerializeObject(message);
        client.Publish(publishingTopic, System.Text.Encoding.UTF8.GetBytes(messagePublish), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
    }
    public void SubscribeToCompasXRTopics()
    {
        Debug.Log("MQTT: Subscribing to Compas XR Topics");

        //Subscribe to Compas XR Get Trajectory Topics
        SubscribeToTopic(compasXRTopics.subscribers.getTrajectoryResultTopic);

        //Subscribe to Compas XR Approve Trajectory Topics
        SubscribeToTopic(compasXRTopics.subscribers.approveTrajectoryTopic);
    }
    public void UnsubscribeFromCompasXRTopics()
    {
        //Unsubscribe to Compas XR Get Trajectory Topics
        UnsubscribeFromTopic(compasXRTopics.subscribers.getTrajectoryResultTopic);

        //Unsubscribe to Compas XR Approve Trajectory Topics
        UnsubscribeFromTopic(compasXRTopics.subscribers.approveTrajectoryTopic);
    }  

    //////////////////////////////////////////// Message Managers ////////////////////////////////////////////
    protected override void DecodeMessage(string topic, byte[] message)
    {
        //TODO: CAN I DO SOMETHING OTHER THEN A STRING?
        msg = System.Text.Encoding.UTF8.GetString(message);
        Debug.Log("MQTT: Received: " + msg + " from topic: " + topic);

        //TODO: ADD MESSAGE HANDLER HERE BASED ON TOPIC NAME: Input topic name and message.

        StoreMessage(msg);
    }

    private void StoreMessage(string eventMsg)
    {
        if (eventMessages.Count > 50) eventMessages.Clear();
        eventMessages.Add(eventMsg);
    }

    //////////////////////////////////////////// Event Handlers ////////////////////////////////////////////
    public void AddConnectionEventListners()
    {
        // On connection succeeded event method from M2MqttUnityClient class
        ConnectionSucceeded += SubscribeToCompasXRTopics;
        
        //On connection failed event method from M2MqttUnityClient class
        ConnectionFailed += UIFunctionalities.SignalMQTTConnectionFailed;
    }
    public void RemoveConnectionEventListners()
    {
        // On connection succeeded event method from M2MqttUnityClient class
        ConnectionSucceeded -= SubscribeToCompasXRTopics;
        
        //On connection failed event method from M2MqttUnityClient class
        ConnectionFailed -= UIFunctionalities.SignalMQTTConnectionFailed;
    }

}

