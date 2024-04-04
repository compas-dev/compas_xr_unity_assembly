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
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Threading;

public class MqttTrajectoryManager : M2MqttUnityClient
{
    [Header("MQTT Settings")]
    [Tooltip("Set the topic to publish")]

    //Controller Name
    public string controllerName = "MQTT Trajectory Controller";

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


    //MAS Modifiers

    //TODO: MAKE TOPICS TO SUBSCRIBE AND PUBLISH TO.
    public string topicToSubscribe = "MyCustomTopicToSubscribeName";
    public string topicToPublish = "MyCustomTopicToPublishName";


    protected override void Start()
    {
        //Calling Base Start is a property of Inheritance from M2MqttUnityClient. Ensures that the base class is called First.
        base.Start();

        //On Start Initialization
        OnStartorRestartInitilization();

    }
    protected override void Update()
    {
        //Calling Base Update is a property of Inheritance from M2MqttUnityClient. Ensures that the base class is called First.
        base.Update();
    }
    public void OnDestroy()
    {
        //Remove event listners
        RemoveConnectionEventListners();

        //Disconnect from MQTT
        Disconnect();
    }
    
    //////////////////////////////////////////// General Methods ////////////////////////////////////////////
    public void OnStartorRestartInitilization(bool Restart = false)
    {
        //Connect to MQTT Broker on start with default settings.
        Connect();

        //Add event listners
        AddConnectionEventListners();
    }

    //////////////////////////////////////////// Connection Managers ////////////////////////////////////////
    protected override void OnConnected()
    {
        base.OnConnected();
    }
    protected override void OnDisconnected()
    {
        base.OnDisconnected();
        Debug.Log("MQTT: OnDisconnected method.");

    }
    protected override void OnConnectionLost()
    {
        //Call the base class method
        base.OnConnectionLost();
        Debug.Log("MQTT: CONNECTION LOST");
    }

    //////////////////////////////////////////// Topic Managers /////////////////////////////////////////////
    private void SubscribeToTopic(string topicToSubscribe)
    {
        if (!string.IsNullOrEmpty(topicToSubscribe) && client != null)
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
        if (!string.IsNullOrEmpty(topicToUnsubscribe) && client != null)
        {
            client.Unsubscribe(new string[] { topicToUnsubscribe });
            Debug.Log("Unsubscribed from topic: " + topicToUnsubscribe);
        }
        else
        {
            Debug.LogWarning("MQTT: Topic to unsubscribe is empty or client is null.");
        }
    }

    //////////////////////////////////////////// Message Managers ////////////////////////////////////////////
    protected override void DecodeMessage(string topic, byte[] message)
    {
        //message 
        msg = System.Text.Encoding.UTF8.GetString(message);
        Debug.Log("MQTT: Received: " + msg + " from topic: " + topic);
        Debug.Log("MqttMessageType: " + msg.GetType() + " MqttMessage: " + msg);

        //TODO: Write Message Handler method to be called here
        MessageHandler(msg, topic);

        //Store the message
        StoreMessage(msg);
    }

    //TODO: Write Message Handler
    private void MessageHandler(string msg, string topic)
    {
        //Deserialize the message
        var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(msg);

        //Fetch the string data from the message
        string data = jsonObject["result"].ToString();

        //TODO: WRITE LOGIC TO DO SOMETHING WITH THE DATA

        Debug.Log("MQTT: MessageHandler: " + data);
        Debug.Log("MQTT: MessageHandler type: " + data.GetType());
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
        ConnectionSucceeded += () => SubscribeToTopic(topicToSubscribe);

    }
    public void RemoveConnectionEventListners()
    {
        // On connection succeeded event method from M2MqttUnityClient class
        ConnectionSucceeded -= () => SubscribeToTopic(topicToSubscribe);

    }

}