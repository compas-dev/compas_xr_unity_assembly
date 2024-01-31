using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.Events;

//TODO: I DONT THINK I NEED THIS SCRIPT AT ALL. THE OTHER ONE SHOULD BE GOOD AND SHOULD BE RENAMED.
public class MqttTrajectoryController : MonoBehaviour
{
    public string nameController = "MQTT Trajectory Controller";
    private MqttTrajectoryReceiver _eventSender;

    public SaveAppSettings saveAppSettings;

  void Start()
  {
    //This is because they are on the same component and is the reason that it works.
    _eventSender = GetComponent<MqttTrajectoryReceiver>();

    //Subscribe to event.
    _eventSender.OnMessageArrived += OnMessageArrivedHandler;
  }

  public void OnMessageArrivedHandler(string newMsg)
  {
    Debug.Log("MQTT: Event Fired. The message, from Object " + nameController +" is = " + newMsg);

    // Dictionary<string, string> resultDataDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(newMsg);

    // FirebaseManager.Instance.appId = resultDataDict["appId"];
    // FirebaseManager.Instance.apiKey = resultDataDict["apiKey"];
    // FirebaseManager.Instance.databaseUrl = resultDataDict["databaseUrl"];
    // FirebaseManager.Instance.storageBucket = resultDataDict["storageBucket"];
    // FirebaseManager.Instance.projectId = resultDataDict["projectId"];

    // saveAppSettings.UpdateInputFields();

  }

  
}

  


