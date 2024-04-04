using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using M2MqttUnity;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Newtonsoft.Json;
using TMPro;

public class MqttManager : M2MqttUnityClient
{
    [Header("MQTT Settings")]
    [Tooltip("Set the topic to publish")]

    public string nameController = "MqttManager";

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

    private string currentTopic = "";

    protected override void Start()
    {
        base.Start();
        
        Connect();
    }

    protected override void Update()
    {
        base.Update(); // call ProcessMqttEvents()

    }

    public void OnDestroy()
    {
        Disconnect();
    }
    
    private void SubscribeToTopic(string topicname)
    {

        if (!string.IsNullOrEmpty(topicname))
        {
            client.Subscribe(new string[] { topicname }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            Debug.Log("Subscribed to topic: " + topicname);
        }
        else
        {
            Debug.LogError("Topic to subscribe is empty.");
        }
    }

    private void UnsubscribeCurrentTopic()
    {
        string topicToUnsubscribe = saveFirebaseConfigSettingsScript.topicSubscribeInput.text;
        Debug.Log($"Client: {client}");
        if (!string.IsNullOrEmpty(topicToUnsubscribe))
        {
         client.Unsubscribe(new string[] { topicToUnsubscribe });
         Debug.Log("Unsubscribed from topic: " + topicToUnsubscribe);
        }
        
    }
    
    protected override void DecodeMessage(string topic, byte[] message)
    {
        msg = System.Text.Encoding.UTF8.GetString(message);
        Debug.Log("Received: " + msg + " from topic: " + topic);
        OnMessageArrivedHandler(msg);
        StoreMessage(msg);
    }

    public void OnMessageArrivedHandler(string newMsg)
    {
        Debug.Log("Event Fired. The message, from Object " + nameController +" is = " + newMsg);

        Dictionary<string, string> resultDataDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(newMsg);

        FirebaseManager.Instance.appId = resultDataDict["appId"];
        FirebaseManager.Instance.apiKey = resultDataDict["apiKey"];
        FirebaseManager.Instance.databaseUrl = resultDataDict["databaseUrl"];
        FirebaseManager.Instance.storageBucket = resultDataDict["storageBucket"];
        FirebaseManager.Instance.projectId = resultDataDict["projectId"];

        saveFirebaseConfigSettingsScript.UpdateInputFields();

    }

    private void StoreMessage(string eventMsg)
    {
        if (eventMessages.Count > 50) eventMessages.Clear();
        eventMessages.Add(eventMsg);
    }


}

