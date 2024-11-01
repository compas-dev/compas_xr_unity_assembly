using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using M2MqttUnity;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Newtonsoft.Json;
using TMPro;

namespace CompasXR.Database.FirebaseManagment
{
    public class MqttFirebaseConfigManager : M2MqttUnityClient
    {
        [Header("MQTT Settings")]
        [Tooltip("Set the topic to publish")]

        public string nameController = "Controller 1";
        public string topicPublish = ""; // topic to publish
        public string messagePublish = ""; // message to publish

        private FirebaseConfigSettings saveFirebaseConfigSettingsScript;

        public GameObject greenScreenPanel; // Assign this in the Unity Editor
        private float flashDuration = 1.0f; // Duration of the flash

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
            
            GameObject firebaseManager = GameObject.Find("Firebase_Manager");
            if (firebaseManager != null)
            {
                saveFirebaseConfigSettingsScript = firebaseManager.GetComponent<FirebaseConfigSettings>();
                if (saveFirebaseConfigSettingsScript == null)
                {
                    Debug.LogError("SaveAppSettings script not found on Firebase_Manager.");
                }
            }
            else
            {
                Debug.LogError("Firebase_Manager GameObject not found in the scene.");
            }
            
        Connect();
        }

        public void OnConnectButtonClicked()
        {
            if (client != null && client.IsConnected)
            {
                string topicToSubscribe = saveFirebaseConfigSettingsScript.topicSubscribeInput.text;

                // Check if the topic to subscribe is different from the current topic
                if (!string.IsNullOrEmpty(topicToSubscribe) && topicToSubscribe != currentTopic)
                {
                    // Unsubscribe from the current topic
                    UnsubscribeCurrentTopic();

                    // Update the current topic
                    currentTopic = topicToSubscribe;

                    // Subscribe to the new topic
                    SubscribeToTopic();

                    FlashGreenScreen();
                }
            }
        else
            {
                Debug.LogError("MQTT client is not connected.");
            }
        }
        
        private void SubscribeToTopic()
        {
            string topicToSubscribe = saveFirebaseConfigSettingsScript.topicSubscribeInput.text;
            if (!string.IsNullOrEmpty(topicToSubscribe))
            {
                client.Subscribe(new string[] { topicToSubscribe }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
                Debug.Log("Subscribed to topic: " + topicToSubscribe);
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

        private void FlashGreenScreen()
        {
            greenScreenPanel.SetActive(true); // Show the green screen
            Invoke("HideGreenScreen", flashDuration); // Schedule to hide the green screen
        }

        private void HideGreenScreen()
        {
            greenScreenPanel.SetActive(false); // Hide the green screen
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

        protected override void Update()
        {
            base.Update(); // call ProcessMqttEvents()

        }

        public void OnDestroy()
        {
            Disconnect();
        }
    }
}

