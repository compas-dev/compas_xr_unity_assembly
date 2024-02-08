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

public class MqttTrajectoryReceiver : M2MqttUnityClient
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

    //Compas XR Topics Class
    public CompasXRTopics compasXRTopics;

    //Compas XR Service Manager
    public ServiceManager serviceManager = new ServiceManager();

    //Other Scripts
    public UIFunctionalities UIFunctionalities;

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
        Disconnect();
    }
    
    //////////////////////////////////////////// General Methods ////////////////////////////////////////////
    public void OnStartorRestartInitilization(bool Restart = false)
    {
        //Recnnect bool, allowing this method to be additionally called inside of the update method.
        if (!Restart)
        {
            //Find UI Functionalities
            UIFunctionalities = GameObject.Find("UIFunctionalities").GetComponent<UIFunctionalities>();
        }

        //Connect to MQTT Broker on start with default settings.
        Connect();

        //Add event listners
        AddConnectionEventListners();
    }

    //////////////////////////////////////////// Connection Managers ////////////////////////////////////////
    protected override void OnConnected()
    {
        base.OnConnected();
        Debug.Log("MQTT: ON CONNECTED INTERNAL METHOD");

        //Set UI Object Color to green if the communication toggle is on.
        if (UIFunctionalities.CommunicationToggleObject.GetComponent<Toggle>().isOn)
        {
            UIFunctionalities.SetUIObjectColor(UIFunctionalities.MqttConnectButtonObject, Color.green);
        }
    }
    protected override void OnDisconnected()
    {
        base.OnDisconnected();
        Debug.Log("MQTT: ON DISCONNECTED INTERNAL METHOD.");
        //I dont think we need the is connected bool.
        // isConnected=false;

    }
    protected override void OnConnectionLost()
    {
        //Call the base class method
        base.OnConnectionLost();

        //Signal On screen message that connection has been lost
        UIFunctionalities.SignalOnScreenMessageWithButton(UIFunctionalities.MQTTConnectionLostMessageObject);

        Debug.Log("MQTT: CONNECTION LOST INTERNAL METHOD!");
    }
    public async void DisconnectandReconnectAsyncRoutine()
    {
        //Disconnect from MQTT
        Disconnect();

        //Wait until MQTT is disconnected
        StartCoroutine(ReconnectAfterDisconect());
    }
    private IEnumerator ReconnectAfterDisconect()
    {
        // Wait for MQTT to be disconnected
        yield return new WaitUntil(() => !mqttClientConnected);

        // Wait for a moment to ensure MQTT has fully disconnected (you can adjust the duration)
        yield return new WaitForSeconds(0.5f);

        //Reconnect to MQTT
        OnStartorRestartInitilization(true);
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

        //Subscribe to compas XR Approval Counter Request Topic
        SubscribeToTopic(compasXRTopics.subscribers.approvalCounterRequestTopic);
    }
    public void UnsubscribeFromCompasXRTopics()
    {
        //Unsubscribe to Compas XR Get Trajectory Topics
        UnsubscribeFromTopic(compasXRTopics.subscribers.getTrajectoryResultTopic);

        //Unsubscribe to Compas XR Approve Trajectory Topics
        UnsubscribeFromTopic(compasXRTopics.subscribers.approveTrajectoryTopic);

        //Unsubscribe to compas XR Approval Counter Request Topic
        UnsubscribeFromTopic(compasXRTopics.subscribers.approvalCounterRequestTopic);
    }  

    //////////////////////////////////////////// Message Managers ////////////////////////////////////////////
    protected override void DecodeMessage(string topic, byte[] message)
    {
        //TODO: CAN I DO SOMETHING OTHER THEN A STRING?
        msg = System.Text.Encoding.UTF8.GetString(message);
        Debug.Log("MQTT: Received: " + msg + " from topic: " + topic);

        //TODO: ADD MESSAGE HANDLER HERE BASED ON TOPIC NAME: Input topic name and message.
        CompasXRIncomingMessageHandler(topic, msg);

        //Store the message
        StoreMessage(msg);
    }
    private void CompasXRIncomingMessageHandler(string topic, string message)
    {
        //TODO: TURN INTO A SWITCH STATEMENT?
        //Get Trajectory Result Message.
        if (topic == compasXRTopics.subscribers.getTrajectoryResultTopic)
        {
            //Deserialize the message
            GetTrajectoryResult getTrajectoryResultmessage = GetTrajectoryResult.Parse(message);
            
            //If I am not the primary user checks
            if (!serviceManager.PrimaryUser)
            {
                //If the trajectory count is greater then zero signal on screen message to request my review of trajectory
                if (getTrajectoryResultmessage.Trajectory.Count > 0)
                {
                    //Signal the UI
                    // UIFunctionalities.SignalOnScreenMessageWithButton(UIFunctionalities.RequestReviewTrajectoryMessageObject);

                    //TODO: Signal On Screen Message with Button

                    //TODO: Set current element to the element from the message.

                    Debug.Log("GetTrajectoryResult (!PrimaryUser): YOU SHOULD SIGNAL AN ONSCREEN MESSAGE TO REQUEST REVIEW OF TRAJECTORY");
                }
            }
            //I am the primary user
            else
            {
                //If the trajectory count is greater then zero move to trajectory review set Review Options to true.
                if (getTrajectoryResultmessage.Trajectory.Count > 0)
                {
                    //Subscribe to Approval Counter Result topic
                    SubscribeToTopic(compasXRTopics.subscribers.approvalCounterResultTopic);

                    //Set visibility and interactibility of trajectory review elements
                    UIFunctionalities.TrajectoryServicesUIControler(false, false, true, true, false, false);

                    //Set the current trajectory of the Service Manager
                    serviceManager.CurrentTrajectory = getTrajectoryResultmessage.Trajectory;

                    //Publish request for approval counter and do not input header.
                    PublishToTopic(compasXRTopics.publishers.approvalCounterRequestTopic, new ApprovalCounterRequest(UIFunctionalities.CurrentStep).GetData());
                }
                //If the trajectory count is zero reset Service Manger elements, and Return to Request Trajectory Service (Maybe should signal Onscreen Message?)
                else
                {
                    //Set Primary user back to false
                    serviceManager.PrimaryUser = false;

                    //Set visibility and interactibility of request trajectory button
                    UIFunctionalities.TrajectoryServicesUIControler(true, true, false, false, false, false);
                }
            }

            //Signal the UI
            // UIFunctionalities.SignalGetTrajectoryResult(getTrajectoryResult);
        }

        //Approve Trajectory Message
        else if (topic == compasXRTopics.subscribers.approveTrajectoryTopic)
        {
            //Deserialize the message
            // ApproveTrajectory approveTrajectory = JsonConvert.DeserializeObject<ApproveTrajectory>(message);

            //Signal the UI
            // UIFunctionalities.SignalApproveTrajectory(approveTrajectory);
        }

        //Approval Counter Request Message
        else if (topic == compasXRTopics.subscribers.approvalCounterRequestTopic)
        {
            //Deserialize the message
            // ApprovalCounterRequest approvalCounterRequest = JsonConvert.DeserializeObject<ApprovalCounterRequest>(message);

            //Signal the UI
            // UIFunctionalities.SignalApprovalCounterRequest(approvalCounterRequest);
        }

        //Approval Counter Result Message
        else if (topic == compasXRTopics.subscribers.approvalCounterResultTopic)
        {
            //Deserialize the message
            // ApprovalCounterResponse approvalCounterResponse = JsonConvert.DeserializeObject<ApprovalCounterResponse>(message);

            //Signal the UI
            // UIFunctionalities.SignalApprovalCounterResponse(approvalCounterResponse);
        }

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

