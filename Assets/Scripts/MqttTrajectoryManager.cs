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

    //Compas XR Topics Class
    public CompasXRTopics compasXRTopics;

    //Compas XR Service Manager
    public ServiceManager serviceManager = new ServiceManager();

    //Other Scripts
    public UIFunctionalities UIFunctionalities;
    public DatabaseManager databaseManager;
    public TrajectoryVisulizer trajectoryVisulizer;

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
        //Unsubscribe from Compas XR Topics
        UnsubscribeFromCompasXRTopics();

        //Remove event listners
        RemoveConnectionEventListners();

        //Disconnect from MQTT
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
            databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
            trajectoryVisulizer = GameObject.Find("TrajectoryVisulizer").GetComponent<TrajectoryVisulizer>();
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
        string message = "WARNING: MQTT connection has been lost. Please check your internet connection and restart the application.";
        UIFunctionalities.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref  UIFunctionalities.MQTTConnectionLostMessageObject, "MQTTConnectionLostMessage", UIFunctionalities.MessagesParent, message, "OnConnectionLost: MQTT Connection Lost");

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
    public void SubscribeToCompasXRTopics()
    {
        Debug.Log("MQTT: Subscribing to Compas XR Topics");

        //Subscribe to Compas XR Get Trajectory Topics
        SubscribeToTopic(compasXRTopics.subscribers.getTrajectoryRequestTopic);

        //Subscribe to Compas XR Get Trajectory Topics
        SubscribeToTopic(compasXRTopics.subscribers.getTrajectoryResultTopic);

        //Subscribe to Compas XR Approve Trajectory Topics
        SubscribeToTopic(compasXRTopics.subscribers.approveTrajectoryTopic);

        //Subscribe to compas XR Approval Counter Request Topic
        SubscribeToTopic(compasXRTopics.subscribers.approvalCounterRequestTopic);
    }
    public void UnsubscribeFromCompasXRTopics()
    {
        //Unsubscribe to Compas XR Get Trajectory Request Topic
        UnsubscribeFromTopic(compasXRTopics.subscribers.getTrajectoryRequestTopic);

        //Unsubscribe to Compas XR Get Trajectory Result Topic
        UnsubscribeFromTopic(compasXRTopics.subscribers.getTrajectoryResultTopic);

        //Unsubscribe to Compas XR Approve Trajectory Topics
        UnsubscribeFromTopic(compasXRTopics.subscribers.approveTrajectoryTopic);

        //Unsubscribe to compas XR Approval Counter Request Topic
        UnsubscribeFromTopic(compasXRTopics.subscribers.approvalCounterRequestTopic);
    }  

    //////////////////////////////////////////// Message Managers ////////////////////////////////////////////
    protected override void DecodeMessage(string topic, byte[] message)
    {
        //message 
        msg = System.Text.Encoding.UTF8.GetString(message);
        Debug.Log("MQTT: Received: " + msg + " from topic: " + topic);

        //Message Handler for compas XR topics
        CompasXRIncomingMessageHandler(topic, msg);

        //Store the message
        StoreMessage(msg);
    }
    private void CompasXRIncomingMessageHandler(string topic, string message)
    {
        //Get Trajectory Request Message
        if (topic == compasXRTopics.subscribers.getTrajectoryRequestTopic)
        {
            Debug.Log("MQTT: GetTrajectoryRequest Message Handeling");

            //Deserialize the message usign parse method //TODO: INCLUDE TRY EXCEPT BLOCK HERE?
            GetTrajectoryRequest getTrajectoryRequestmessage = GetTrajectoryRequest.Parse(message);

            //Get Trajectory Request Message Handler
            GetTrajectoryRequestReceivedMessageHandler(getTrajectoryRequestmessage);
        }

        //Get Trajectory Result Message.
        else if (topic == compasXRTopics.subscribers.getTrajectoryResultTopic)
        {
            Debug.Log("MQTT: GetTrajectoryResult Message Handeling");

            //Deserialize the message using parse method //TODO: INCLUDE TRY EXCEPT BLOCK HERE?
            GetTrajectoryResult getTrajectoryResultmessage = GetTrajectoryResult.Parse(message);

            //Get Trajectory Result Message Handler
            GetTrajectoryResultReceivedMessageHandler(getTrajectoryResultmessage); 
        }

        //Approve Trajectory Message
        else if (topic == compasXRTopics.subscribers.approveTrajectoryTopic)
        {
            Debug.Log("MQTT: ApproveTrajectory Message Handeling");

            //Deserialize the message usign parse method //TODO: INCLUDE TRY EXCEPT BLOCK HERE?
            ApproveTrajectory trajectoryApprovalMessage = ApproveTrajectory.Parse(message);

            //Approve Trajectory Message Handler
            ApproveTrajectoryMessageReceivedHandler(trajectoryApprovalMessage);
        }

        //Approval Counter Request Message
        else if (topic == compasXRTopics.subscribers.approvalCounterRequestTopic)
        {
            //Deserialize the message //TODO: INCLUDE TRY EXCEPT BLOCK HERE?
            ApprovalCounterRequest approvalCounterRequestMessage = ApprovalCounterRequest.Parse(message);
            
            //Approval Counter Request Message Handler
            ApprovalCounterRequestMessageReceivedHandler(approvalCounterRequestMessage);
        }

        //Approval Counter Result Message
        else if (topic == compasXRTopics.subscribers.approvalCounterResultTopic)
        {
            //Deserialize the message //TODO: INCLUDE TRY EXCEPT BLOCK HERE?
            ApprovalCounterResult approvalCounterResultMessage = ApprovalCounterResult.Parse(message);

            //Approval Counter Result Message Handler
            ApprovalCounterResultMessageReceivedHandler(approvalCounterResultMessage);
        }

        //Default
        else
        {
            Debug.LogWarning("MQTT: No message handler for topic: " + topic);
        }

    }
    private void GetTrajectoryRequestReceivedMessageHandler(GetTrajectoryRequest getTrajectoryRequestmessage)
    {
        //Set last request message of the Service Manager
        serviceManager.LastGetTrajectoryRequestMessage = getTrajectoryRequestmessage;

        //Check the device ID to see if it is mine, and if it is not apply Get Trajectory Request Transaction lock.
        if (getTrajectoryRequestmessage.Header.DeviceID != SystemInfo.deviceUniqueIdentifier)
        {
            Debug.Log($"MQTT: GetTrajectoryRequest from user {getTrajectoryRequestmessage.Header.DeviceID}");

            //Set Service Manager Transaction lock
            serviceManager.TrajectoryRequestTransactionLock = true;

        }
        else
        {
            Debug.Log("MQTT: GetTrajectoryRequest this request came from me");
        }
    }
    private void GetTrajectoryResultReceivedMessageHandler(GetTrajectoryResult getTrajectoryResultmessage)  //TODO: IS DIRTY BOOL SPECIFIC TO APPROVAL? AND TIME OUT FOR WAITNG FOR TRAJECTORY RESPONSE.
    {

        // Is dirty bool that is set when the for time outs and is used to ignore messages that are not needed.
        // if(serviceManager.IsDirty) //TODO: I THINK THIS SHOULD BE SPECIFIC TO SERVICE TIMEOUT... ex. serviceManager.IsDirtyGetTrajectoryResult vs. serviceManager.IsDirtyApproval
        // {
        //     if(serviceManager.IsDirtyMessageHeader.ResponseID == getTrajectoryResultmessage.Header.ResponseID && serviceManager.IsDirtyMessageHeader.SequenceID == getTrajectoryResultmessage.Header.SequenceID + 1)
        //     {
        //         Debug.Log("MQTT: GetTrajectoryResult: IsDirty is true and the message is the same as the dirty message. No action taken.");
        //         return;
        //     }
        //     else
        //     {
        //         Debug.Log("MQTT: GetTrajectoryResult: IsDirty is true but the message is not the same as the dirty message. Resetting IsDirty to false.");
        //         serviceManager.IsDirty = false;
        //     }
        // }

        //Set last result message of the Service Manager
        serviceManager.LastGetTrajectoryResultMessage = getTrajectoryResultmessage;
        
        if(serviceManager.LastGetTrajectoryRequestMessage != null)
        {
            //First Check if the message is the same as the last request message and if the trajectory count is greater then zero
            if(getTrajectoryResultmessage.Header.ResponseID != serviceManager.LastGetTrajectoryRequestMessage.Header.ResponseID 
            || getTrajectoryResultmessage.Header.SequenceID != serviceManager.LastGetTrajectoryRequestMessage.Header.SequenceID + 1
            || getTrajectoryResultmessage.ElementID != serviceManager.LastGetTrajectoryRequestMessage.ElementID)
            {
                if(serviceManager.PrimaryUser)
                {                    
                    Debug.LogWarning("MQTT: GetTrajectoryResult (PrimaryUser): ResponseID, SequenceID, or ElementID do not match the last GetTrajectoryRequestMessage. No action taken.");

                    string message = "WARNING: Trajectory Response did not match expectations. Returning to Request Service.";
                    UIFunctionalities.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref UIFunctionalities.TrajectoryResponseIncorrectWarningMessageObject, "TrajectoryResponseIncorrectWarningMessage", UIFunctionalities.MessagesParent, message, "GetTrajectoryResultReceivedMessageHandler: Message Structure incorrect.");
                    //TODO: MAKE SURE THIS WORKS.
                    // UIFunctionalities.SignalOnScreenMessageWithButton(UIFunctionalities.TrajectoryResponseIncorrectWarningMessageObject);

                    //Set Primary user back to false
                    serviceManager.PrimaryUser = false;

                    //Set Current Service to None
                    serviceManager.currentService = ServiceManager.CurrentService.None;

                    //Set visibility and interactibility of request trajectory button
                    UIFunctionalities.TrajectoryServicesUIControler(true, true, false, false, false, false);

                    return;
                }
                else
                {
                    //Release Trajectory Request Transaction lock
                    serviceManager.TrajectoryRequestTransactionLock = false;

                    //If the robot toggle is on set the TrajectoryRequest UI From my current step (allows me to freely request if I am on a robot element).
                    if(UIFunctionalities.RobotToggleObject.GetComponent<Toggle>().isOn)
                    {
                        //Set interactibility based on my current element.
                        UIFunctionalities.SetRoboticUIElementsFromKey(UIFunctionalities.CurrentStep);
                    }

                    Debug.LogWarning("MQTT: GetTrajectoryResult (!PrimaryUser): ResponseID, SequenceID, or ElementID do not match the last GetTrajectoryRequestMessage. Ignoring Message.");
                    return;
                }
            }
            //The message is the same as the last request messsage
            else
            {    
                
                //Check if the count is greater then Zero and start the time out dependant on if I am primary user or not.
                if(getTrajectoryResultmessage.Trajectory.Count > 0)
                {
                    //Create a new cancellation token source and storing it in the service manager
                    serviceManager.ApprovalTimeOutCancelationToken = new CancellationTokenSource();
                    
                    //Float duration dependent on if I am Primary user or not
                    float duration = 120; //TODO: DURATION FOR PRIMARY USER WAITING FOR APPROVALS.... NEEDS TO BE ADJUSTED W/ FABRICATION TIME.
                    if(!serviceManager.PrimaryUser)
                    {
                        duration = 240; //TODO: DURATION FOR NON PRIMARY USER WAITING FOR CONSENSUS.... NEEDS TO BE ADJUSTED W/ FABRICATION TIME.
                    }

                    // Trajectory approval time out
                    _= TrajectoryApprovalTimeout(getTrajectoryResultmessage.ElementID, duration, serviceManager.ApprovalTimeOutCancelationToken.Token);
                }
                else
                {
                    Debug.Log("MQTT: GetTrajectoryResult: Trajectory count is zero. No time out started.");
                }
                
                //If I am not the primary user checks
                if (!serviceManager.PrimaryUser)
                {
                    //If the trajectory count is greater then zero signal on screen message to request my review of trajectory
                    if (getTrajectoryResultmessage.Trajectory.Count > 0)
                    {
                        //Release Trajectory Request Transaction lock
                        serviceManager.TrajectoryRequestTransactionLock = false;

                        //Signal On Screen Message for trajectory review
                        UIFunctionalities.SignalTrajectoryReviewRequest(
                            getTrajectoryResultmessage.ElementID,
                            getTrajectoryResultmessage.RobotName,
                            serviceManager.ActiveRobotName,
                            () => trajectoryVisulizer.VisulizeRobotTrajectory(
                                getTrajectoryResultmessage.Trajectory,
                                trajectoryVisulizer.URDFLinkNames,
                                getTrajectoryResultmessage.RobotBaseFrame,
                                getTrajectoryResultmessage.TrajectoryID,
                                trajectoryVisulizer.ActiveRobot,
                                trajectoryVisulizer.ActiveTrajectoryParentObject,
                                true));

                        //Set curent trajectory of the Service Manager
                        serviceManager.CurrentTrajectory = getTrajectoryResultmessage.Trajectory;

                        //Set current Service to Approve Trajectory
                        serviceManager.currentService = ServiceManager.CurrentService.ApproveTrajectory;

                        Debug.Log("GetTrajectoryResult (!PrimaryUser): Trajectory count is greater then zero. I am moving on to trajectory review.");
                    }
                    else
                    {
                        //Set Service Manager Request Transaction lock
                        serviceManager.TrajectoryRequestTransactionLock = false;

                        //If the robot toggle is on set the TrajectoryRequest UI From my current step (allows me to freely request if I am on a robot element).
                        if(UIFunctionalities.RobotToggleObject.GetComponent<Toggle>().isOn)
                        {
                            //Set interactibility based on my current element.
                            UIFunctionalities.SetRoboticUIElementsFromKey(UIFunctionalities.CurrentStep);
                        }

                        Debug.Log("GetTrajectoryResult (!PrimaryUser): Trajectory count is zero. I am free to request.");
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

                        //Set the current trajectory of the Service Manager && Set current Service to Approve Trajectory
                        serviceManager.CurrentTrajectory = getTrajectoryResultmessage.Trajectory;
                        serviceManager.currentService = ServiceManager.CurrentService.ApproveTrajectory;

                        //If the requested robot is not the same as mine update mine.
                        if(getTrajectoryResultmessage.RobotName != serviceManager.ActiveRobotName)
                        {
                            UIFunctionalities.SignalActiveRobotUpdateFromPlanner(
                                getTrajectoryResultmessage.ElementID,
                                getTrajectoryResultmessage.RobotName,
                                serviceManager.ActiveRobotName,
                                () => trajectoryVisulizer.VisulizeRobotTrajectory(
                                    getTrajectoryResultmessage.Trajectory,
                                    trajectoryVisulizer.URDFLinkNames,
                                    getTrajectoryResultmessage.RobotBaseFrame,
                                    getTrajectoryResultmessage.TrajectoryID,
                                    trajectoryVisulizer.ActiveRobot,
                                    trajectoryVisulizer.ActiveTrajectoryParentObject,
                                    true));
                            
                            Debug.Log("MQTT: GetTrajectoryResult (PrimaryUser): Robot Name in the message is not the same as the active robot name signaling on screen control.");

                        }
                        else
                        {
                            trajectoryVisulizer.VisulizeRobotTrajectory(getTrajectoryResultmessage.Trajectory, trajectoryVisulizer.URDFLinkNames, getTrajectoryResultmessage.RobotBaseFrame, getTrajectoryResultmessage.TrajectoryID, trajectoryVisulizer.ActiveRobot, trajectoryVisulizer.ActiveTrajectoryParentObject, true);
                            Debug.Log("MQTT: GetTrajectoryResult (PrimaryUser): Robot Name in the message is the same as the active robot name.");
                        }
                        
                        //Publish request for approval counter and do not input header.
                        PublishToTopic(compasXRTopics.publishers.approvalCounterRequestTopic, new ApprovalCounterRequest(UIFunctionalities.CurrentStep).GetData());
                    }
                    //If the trajectory count is zero reset Service Manger elements, and Return to Request Trajectory Service (Maybe should signal Onscreen Message?)
                    else
                    {
                        Debug.Log("MQTT: GetTrajectoryResult (PrimaryUser): Trajectory count is zero resetting Service Manager and returning to Request Trajectory Service.");

                        //Set Primary user back to false
                        serviceManager.PrimaryUser = false;

                        //Set Current Service to None
                        serviceManager.currentService = ServiceManager.CurrentService.None;

                        //Set visibility and interactibility of request trajectory button
                        UIFunctionalities.TrajectoryServicesUIControler(true, true, false, false, false, false);

                        //Set on screen message and return to trajectory request service
                        string message = "WARNING: The robotic controler replied with a Null trajectory. You will be returned to trajectory request.";
                        UIFunctionalities.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref UIFunctionalities.TrajectoryNullWarningMessageObject, "TrajectoryNullWarningMessage", UIFunctionalities.MessagesParent, message, "GetTrajectoryResultReceivedMessageHandler: Received trajectory is null");
                    }
                }
            }
        }
        else
        {
            //TODO: Should this send a cancelation.... Basically the only time this could happen is if a user joins in the moments between the request and result.
            Debug.LogWarning("MQTT: GetTrajectoryResult LastGetTrajectoryRequestMessage is null. A request must be made before this code works.");
        }
    }
    private void ApproveTrajectoryMessageReceivedHandler(ApproveTrajectory trajectoryApprovalMessage) //TODO: IS DIRTY BOOL SPECIFIC TO APPROVAL?
    {
        //Is dirty bool that is set when the for time outs and is used to ignore messages that are not needed.
        if(serviceManager.IsDirtyApproval)
        {
            if(serviceManager.IsDirtyApprovalHeader.ResponseID == trajectoryApprovalMessage.Header.ResponseID && trajectoryApprovalMessage.ApprovalStatus != 3)
            {
                Debug.Log("MQTT: ApproveTrajectoryMessage: IsDirty is true and the message is the same as the dirty message. No action taken.");
                return;
            }
            else
            {
                Debug.Log("MQTT: ApproveTrajectoryMessage: IsDirty is true but the message is not the same as the dirty message. Resetting IsDirty to false.");
                serviceManager.IsDirtyApproval = false;
            }
        }

        //Approve trajectory approvalStatus rejction message received 
        if (trajectoryApprovalMessage.ApprovalStatus == 0)
        {
            //If I am primary user reset service manager items and unsubscribe from Approval Counter Result topic
            if (serviceManager.PrimaryUser)
            {
                //Unsubsribe from Approval Counter Result topic
                UnsubscribeFromTopic(compasXRTopics.subscribers.approvalCounterResultTopic);

                //Set Primary user back to false
                serviceManager.PrimaryUser = false;
            }

            //Time out Cancelation Token Update
            if(serviceManager.ApprovalTimeOutCancelationToken != null)
            {
                Debug.Log("ApproveTrajectoryMessageReceivedHandler: I SHOULD CANCLE THE TIME OUT. FOR REJECTION.");
                serviceManager.ApprovalTimeOutCancelationToken.Cancel();
            }
            
            //If the Active Trajectory child count is greater the 0 then destroy children
            if(trajectoryVisulizer.ActiveTrajectoryParentObject != null && trajectoryVisulizer.ActiveTrajectoryParentObject.transform.childCount > 0)
            {
                trajectoryVisulizer.DestroyActiveTrajectoryChildren();
            }
            else
            {
                Debug.LogWarning("ApproveTrajectoryMessageReceivedHandler: ActiveTrajectoryParentObject is null or has no children.");
            }
            //If the Active Robot is not active set it to active
            if(trajectoryVisulizer.ActiveRobot != null && !trajectoryVisulizer.ActiveRobot.activeSelf)
            {
                trajectoryVisulizer.ActiveRobot.SetActive(true);
            }
            
            //Reset ApprovalCount and UserCount This could be done inside of the PrimaryUser, but Also safe way of error catching
            serviceManager.ApprovalCount.Reset();
            serviceManager.UserCount.Reset();

            //Set Current Trajectory to null && Current Service to None
            serviceManager.CurrentTrajectory = null;
            serviceManager.currentService = ServiceManager.CurrentService.None;

            //No matter if I am Primary user or not: Set visibility and interactibility of Request Trajectory Button
            UIFunctionalities.TrajectoryServicesUIControler(true, true, false, false, false, false);
        }
        //ApproveTrajectoryMessage ApprovalStatus Trajectory approved message received
        else if (trajectoryApprovalMessage.ApprovalStatus == 1)
        {
            Debug.Log($"MQTT: ApproveTrajectory User {trajectoryApprovalMessage.Header.DeviceID} approved trajectory {trajectoryApprovalMessage.TrajectoryID}");

            //If I am the primary user...
            if (serviceManager.PrimaryUser)
            {
                //Increment the approval count
                serviceManager.ApprovalCount.Increment();

                Debug.Log($"MQTT: Message Handeling Counters UserCount == {serviceManager.UserCount.Value} and ApprovalCount == {serviceManager.ApprovalCount.Value}");

                //If the approval count is equal to the user count then move me on to service 3 as the primary User. //TODO: THINK ABOUT HOW THIS CAN BE SET DYNAMICALLY IN THE APPLICATION SETTINGS.
                if (serviceManager.ApprovalCount.Value == serviceManager.UserCount.Value)
                {
                    Debug.Log("MQTT: ApprovalCount == UserCount. Moving to Service 3 as Primary User.");
                    
                    //Unsubsribe from Approval Counter Result topic
                    UnsubscribeFromTopic(compasXRTopics.subscribers.approvalCounterResultTopic);

                    //Set Current Service to Execute Trajectory
                    serviceManager.currentService = ServiceManager.CurrentService.ExacuteTrajectory;

                    //Set visibility and interactibility of Execute Trajectory Button
                    UIFunctionalities.TrajectoryServicesUIControler(false, false, true, false, true, true);
                }
                else
                {
                    Debug.Log($"MQTT: Message Handeling Counters UserCount == {serviceManager.UserCount.Value} and ApprovalCount == {serviceManager.ApprovalCount.Value}");
                }
            }
        }
        //ApproveTrajectoryMessage ApprovalStatus Consensus message received
        else if (trajectoryApprovalMessage.ApprovalStatus == 2)
        {
            Debug.Log($"MQTT: ApproveTrajectory Consensus message received for trajectory {trajectoryApprovalMessage.TrajectoryID}");

            //If I am the primary user...
            if (serviceManager.PrimaryUser)
            {
                //Unsubsribe from Approval Counter Result topic
                UnsubscribeFromTopic(compasXRTopics.subscribers.approvalCounterResultTopic);

                //Set Primary user back to false
                serviceManager.PrimaryUser = false;
            }

            //Time out Cancelation Token: Cancel the time out because of consensus
            if(serviceManager.ApprovalTimeOutCancelationToken != null)
            {
                Debug.Log("ApproveTrajectoryMessageReceivedHandler: I SHOULD CANCLE THE TIME OUT. FOR CONSENSUS.");
                serviceManager.ApprovalTimeOutCancelationToken.Cancel();
            }

            //Just as a safety precaution reset approval counter, user count, current trajectory, and currentService to none for everyone
            serviceManager.ApprovalCount.Reset();
            serviceManager.UserCount.Reset();
            serviceManager.CurrentTrajectory = null;
            serviceManager.currentService = ServiceManager.CurrentService.None;

            //Set visibilty and interactibility of Request Trajectory Button... visible but not interactable
            UIFunctionalities.TrajectoryServicesUIControler(true, false, false, false, false, false);
        }
        
        //ApproveTrajectoryMessage ApprovalStatus Cancelation message received
        else if (trajectoryApprovalMessage.ApprovalStatus == 3)
        {
            Debug.Log($"MQTT: ApproveTrajectory Cancelation message received for trajectory {trajectoryApprovalMessage.TrajectoryID}");

            //Time out Cancelation Token: Cancel the time out because of consensus
            if(serviceManager.ApprovalTimeOutCancelationToken != null)
            {
                Debug.Log("ApproveTrajectoryMessageReceivedHandler: I SHOULD CANCLE THE TIME OUT. FOR TRAJECTORY CANCELATION FROM SOMEONE ELSE.");
                serviceManager.ApprovalTimeOutCancelationToken.Cancel();
            }

            //If I am not the primary user and should be reset to the request trajectory service
            if (!serviceManager.PrimaryUser)
            {
                //Just as a safety precaution reset approval counter, user count, current trajectory, and currentService to none for everyone
                serviceManager.ApprovalCount.Reset();
                serviceManager.UserCount.Reset();
                serviceManager.CurrentTrajectory = null;
                serviceManager.currentService = ServiceManager.CurrentService.None;
                            
                //If the Active Trajectory child count is greater the 0 then destroy children
                if(trajectoryVisulizer.ActiveTrajectoryParentObject != null && trajectoryVisulizer.ActiveTrajectoryParentObject.transform.childCount > 0)
                {
                    trajectoryVisulizer.DestroyActiveTrajectoryChildren();
                }
                //If the Active Robot is not active set it to active
                if(trajectoryVisulizer.ActiveRobot != null && !trajectoryVisulizer.ActiveRobot.activeSelf)
                {
                    trajectoryVisulizer.ActiveRobot.SetActive(true);
                }

                //Signal OnScreen message for Trajectory Approval Canceled
                if (trajectoryApprovalMessage.Header.DeviceID != SystemInfo.deviceUniqueIdentifier)
                {
                    string message = "WARNING : The trajectory approval has been canceled by another user. Returning to Request Trajectory Service.";
                    UIFunctionalities.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref  UIFunctionalities.TrajectoryCancledMessage, "TrajectoryCancledMessage", UIFunctionalities.MessagesParent, message, "ApproveTrajectoryMessageReceivedHandler: Trajectory Cancled by another user.");
                }

                //Set visibilty and interactibility of Request Trajectory Button... visible but not interactable
                UIFunctionalities.TrajectoryServicesUIControler(true, true, false, false, false, false);
            }
            else
            {
                Debug.Log("MQTT: ApproveTrajectory Cancelation message received for trajectory, but I am the primary user. No action taken.");
            }
        }
        
        //ApprovalStatus not recognized.
        else
        {
            Debug.LogWarning("MQTT: Approval Status is not recognized. Approval Status: " + trajectoryApprovalMessage.ApprovalStatus);
        }
    }
    private void ApprovalCounterRequestMessageReceivedHandler(ApprovalCounterRequest approvalCounterRequestMessage)
    {
        Debug.Log($"MQTT: ApprovalCounterRequset Message Received from User {approvalCounterRequestMessage.Header.DeviceID}");

        //No matter what publish with a reply on the approval counter result topic
        PublishToTopic(compasXRTopics.publishers.approvalCounterResultTopic, new ApprovalCounterResult(approvalCounterRequestMessage.ElementID).GetData());
    }
    private void ApprovalCounterResultMessageReceivedHandler(ApprovalCounterResult approvalCounterResultMessage)
    {
        Debug.Log($"MQTT: ApprovalCounterResult Message Received from User{approvalCounterResultMessage.Header.DeviceID} for step {approvalCounterResultMessage.ElementID}");

        //If I am the primary user...
        if (serviceManager.PrimaryUser)
        {
            //Increment the user count
            serviceManager.UserCount.Increment();
        }
    }
    private void StoreMessage(string eventMsg)
    {
        if (eventMessages.Count > 50) eventMessages.Clear();
        eventMessages.Add(eventMsg);
    }
    async Task TrajectoryApprovalTimeout(string elementID, float timeDurationSeconds, CancellationToken cancellationToken)
    {
        Debug.Log($"MQTT: TrajectoryApprovalTimeout: Started with a duration of {timeDurationSeconds} seconds.");
        
        //Time out controled by try and except block in order to catch the TaskCanceledException on completed
        try
        {
            //Wait for the time duration
            await Task.Delay(TimeSpan.FromSeconds(timeDurationSeconds), cancellationToken);

            //Throwing away if the cancelation is called.
            cancellationToken.ThrowIfCancellationRequested();

            if (serviceManager.CurrentTrajectory != null)
            {
                //If I am the Primary User
                if (serviceManager.PrimaryUser && serviceManager.currentService.Equals(ServiceManager.CurrentService.ExacuteTrajectory))
                {
                    Debug.Log("MQTT: TrajectoryApprovalTimeout: Primary User has already moved on to Service 3.");
                    return;
                }
                else
                {
                    Debug.Log("MQTT: TrajectoryApprovalTimeout: Primary User has not moved on to service 3 or Other user reached time out : Services will be reset.");

                    //Publish cancelation on ApproveTrajectory Topic
                    PublishToTopic(compasXRTopics.publishers.approveTrajectoryTopic, new ApproveTrajectory(elementID, serviceManager.ActiveRobotName, serviceManager.CurrentTrajectory, 3).GetData());
                    
                    //Unsubsribe from Approval Counter Result topic
                    if (serviceManager.PrimaryUser)
                    {
                        UnsubscribeFromTopic(compasXRTopics.subscribers.approvalCounterResultTopic);
                    }

                    //Set Primary user back to false && Current Service to None
                    serviceManager.PrimaryUser = false;
                    serviceManager.currentService = ServiceManager.CurrentService.None;

                    //Reset ApprovalCount and UserCount
                    serviceManager.ApprovalCount.Reset();
                    serviceManager.UserCount.Reset();

                    //Signal On Screen Message for Trajectory Approval Timeout
                    string message = "WARNING : Trajectory Approval has timed out. Returning to Request Trajectory Service.";
                    UIFunctionalities.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref  UIFunctionalities.TrajectoryCancledMessage, "TrajectoryCancledMessage", UIFunctionalities.MessagesParent, message, "TrajectoryApprovalTimeout: Trajectory Approval Cancled by Timeout.");

                    //Set visibility and interactibility of Request Trajectory Button
                    UIFunctionalities.TrajectoryServicesUIControler(true, true, false, false, false, false);

                    //Set is dirty to true and store the message header for comparison
                    serviceManager.IsDirtyApproval = true;
                    serviceManager.IsDirtyApprovalHeader = serviceManager.LastGetTrajectoryResultMessage.Header;
                }
            }
            else
            {
                Debug.Log("MQTT: TrajectoryApprovalTimeout: Current Trajectory is null and this method should not have been called. Either Cancelation did not happen properly, or there is a null trajectory on timeout.");
            }

        }

        catch (TaskCanceledException)
        {
            Debug.Log("MQTT: TrajectoryApprovalTimeout: Task was cancled before the time out duration meaning everything proved to be successful or was purposfully cancled.");
        }
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

