using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.Events;


public class MqttController : MonoBehaviour
{
    public string nameController = "Controller 1";
    public string tagOfTheMQTTReceiver="";
    public MqttReceiver _eventSender;

    //public FirebaseInitializer firebaseInitializer;
    public UserInputs userInputs;
    public UnityEvent onUserInputsUpdated;
  
    
  void Start()
  {
    _eventSender=GameObject.FindGameObjectsWithTag(tagOfTheMQTTReceiver)[0].gameObject.GetComponent<MqttReceiver>();
    _eventSender.OnMessageArrived += OnMessageArrivedHandler;
  }

  public void OnMessageArrivedHandler(string newMsg)
  {
    Debug.Log("Event Fired. The message, from Object " + nameController +" is = " + newMsg);

    // Dictionary<string, string> resultDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(newMsg);
    // resultJson = resultDict["result"];
    // Dictionary<string, string> resultDataDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(resultJson); 

    Dictionary<string, string> resultDataDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(newMsg);

    userInputs.appId = resultDataDict["appId"];
    userInputs.apiKey = resultDataDict["apiKey"];
    userInputs.databaseUrl = resultDataDict["databaseUrl"];
    userInputs.storageBucket = resultDataDict["storageBucket"];
    userInputs.projectId = resultDataDict["projectId"];

    // Notify all listeners that the user inputs have been updated
    if (onUserInputsUpdated != null)
    {
        onUserInputsUpdated.Invoke();
    }

    //Debug.Log(userInputs.apiKey);
    
   }
    //string apiKey = resultDataDict["apiKey"];
    //instantiate class & overwrite parameters
    //Debug.Log("API Key: " + apiKey); 

  }
