using System.Collections.Generic;
using UnityEngine;
using M2MqttUnity;
using uPLibrary.Networking.M2Mqtt.Messages;
using Newtonsoft.Json;

//Notes: The entirity of the file is structured as a class that inherits from MonoBehaviour (Unity's base class for scripts)
//Notes: Script is used for connecting to MQTT Broker and managing messages..
public class MqttTrajectoryManager : M2MqttUnityClient
{
    //Notes: Class Member Variables, used to store data and references ex. public DatabaseReference dbreferenceMyFramesData;
    //Notes: The public keyword is named an access modifier, it determines how Member variables, methods, and classes can be accessed from other classes.

    ////////////////// All below is given ///////////////

    [Header("MQTT Settings")]
    [Tooltip("Set the topic to publish")]

    //Controller Name
    public string controllerName = "MQTT Controller";

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

    ////////////////// All above is given ///////////////

    //TODO: MAS: Make topic name to subscribe to.
    public string topicToSubscribe = "MyCustomTopicToSubscribeName";
    
    //TODO: MAS: Make topic name to publish on.
    public string topicToPublish = "MyCustomTopicToPublishName";

    //Notes: Built in Unity methods (methods that come from the inheritance of the MonoBehaviour class)
    /*
        Notes: Awake is called when the script instance is being loaded. This occurs before any Start methods are called.
        Notes: Start is called on the frame when a script is enabled just before any of the Update methods are called the first time.
        Notes: Update is called every frame, if the MonoBehaviour is enabled.
        Notes: OnDestroy is called when the MonoBehaviour will be destroyed.
        Notes: OnEnable is called when the object becomes enabled and active.
    */
    protected override void Start()
    {
        //Notes: Given: Calling Base Start is a property of Inheritance from M2MqttUnityClient. Ensures that the base class is called First.
        base.Start();

        //Notes: On Start Initialization
        OnStartInitilization();

    }
    protected override void Update()
    {
        //Notes: Given: Calling Base Update is a property of Inheritance from M2MqttUnityClient. Ensures that the base class is called First.
        base.Update();
    }
    public void OnDestroy()
    {
        //Remove event listners
        RemoveConnectionEventListners();

        //Notes: Given: Based on Methods inherited from M2MqttUnityClient (Disconnect from broker)
        Disconnect();
    }
    
    //////////////////////////////////////////// Start Methods ////////////////////////////////////////////
    
    //Notes: Given: On Start Initialization Method
    public void OnStartInitilization(bool Restart = false)
    {
        //Notes: Given: Based on Methods inherited from M2MqttUnityClient (Connect to broker)
        Connect();

        //Notes: Given: Based on Methods inherited from M2MqttUnityClient
        AddConnectionEventListners();
    }

    //////////////////////////////////////////// Connection Managers ////////////////////////////////////////
    
    //Notes: Given: Based on Methods inherited from M2MqttUnityClient
    protected override void OnConnected()
    {
        base.OnConnected();
    }

    //Notes: Given: Based on Methods inherited from M2MqttUnityClient
    protected override void OnDisconnected()
    {
        base.OnDisconnected();
        Debug.Log("MQTT: OnDisconnected method.");

    }

    //Notes: Given: Based on Methods inherited from M2MqttUnityClient
    protected override void OnConnectionLost()
    {
        //Call the base class method
        base.OnConnectionLost();
        Debug.Log("MQTT: CONNECTION LOST");
    }

    //////////////////////////////////////////// Topic Managers /////////////////////////////////////////////

    //Notes: Given: Based on Methods inherited from M2MqttUnityClient
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

    //Notes: Given: Based on Methods inherited from M2MqttUnityClient
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

    //Notes: Given: Based on Methods inherited from M2MqttUnityClient
    public void PublishToTopic(string publishingTopic,  Dictionary<string, object> message)
    {   
        if (client != null && client.IsConnected)
        {
            string messagePublish = JsonConvert.SerializeObject(message);
            client.Publish(publishingTopic, System.Text.Encoding.UTF8.GetBytes(messagePublish), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        }
        else
        {
            Debug.LogWarning("MQTT: Client is null or not connected. Cannot publish message.");
        }
    }

    //////////////////////////////////////////// Message Managers ////////////////////////////////////////////

    //Notes: Given: Based on Methods inherited from M2MqttUnityClient
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

    //TODO: MAS: Write Message Handler
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

    //Notes: Given: Based on Methods inherited from M2MqttUnityClient
    private void StoreMessage(string eventMsg)
    {
        if (eventMessages.Count > 50) eventMessages.Clear();
        eventMessages.Add(eventMsg);
    }

    //////////////////////////////////////////// Event Handlers ////////////////////////////////////////////

    //Notes: Given: Based on Methods inherited from M2MqttUnityClient
    public void AddConnectionEventListners()
    {
        // On connection succeeded event method from M2MqttUnityClient class
        ConnectionSucceeded += () => SubscribeToTopic(topicToSubscribe);

    }

    //Notes: Given: Based on Methods inherited from M2MqttUnityClient
    public void RemoveConnectionEventListners()
    {
        // On connection succeeded event method from M2MqttUnityClient class
        ConnectionSucceeded -= () => SubscribeToTopic(topicToSubscribe);

    }

}