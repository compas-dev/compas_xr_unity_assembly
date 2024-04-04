using System.Collections;
using System.Collections.Generic;
using Helpers;
using UnityEngine;
using UnityEngine.UI;
using Instantiate;
using JSON;
using Newtonsoft.Json;
using System;
using System.Linq;
using TMPro;
using System.Xml.Linq;
using UnityEngine.XR.ARFoundation;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARSubsystems;
using System.Globalization;
using ApplicationModeControler;
using MQTTDataCompasXR;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine.Events;
using Google.MiniJSON;


public class UIFunctionalities : MonoBehaviour
{

    //MAS INTRO ITEMS

    // Other Scripts
    public DatabaseManager databaseManager; //Example 1 of finding an object in the scene (searching)
    public InstantiateObjects instantiateObjects;

    public GameObject RuntimeObjectStorage; //Example 2 of linking an object in the scene to the script (via editor)
    public Toggle ObjectMovementToggle;

    // UI Objects
    // public Button FetchDataButton; // ONLY If not linking VIA UNITY INSPECTOR
    // public Button PublishDataButton; // ONLY If not linking VIA UNITY INSPECTOR

    
    void Start()
    {
        //TODO: MASIntro: 0. Find Other Script References 
        databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>(); //Example 1 of finding and setting a refrence to another object in the scene (searching)
        instantiateObjects = GameObject.Find("Instantiate").GetComponent<InstantiateObjects>();

        //TODO: On Start Find the toggle component on the canvas
        ObjectMovementToggle = GameObject.Find("ObjectMovementToggle").GetComponent<Toggle>(); //Example of finding a specific component on an object
    }

    void Update()
    {
        //TODO: Call the movement toggle in the update method
        ToggleObjectMovement(ObjectMovementToggle);
    }

    /////////////////////////////////// UI Control & OnStart methods ////////////////////////////////////////////////////

    //MAS METHODS
    public void FetchDataButtonMethod()
    {
        Debug.Log("FetchDataButtonMethod: Fetch Data Button Pressed");

        //Call the Database Manager Method and Reference to fetch the data
        databaseManager.FetchRealTimeDatabaseData(databaseManager.dbreferenceMyFramesData);

    }
    
    //TODO: Write a method that we can link to a button for publishing to the firebase
    public void PublishDataButtonMethod()
    {
        //Utilize instantiate Objects method to create a rhino compatible dictionary for objects
        Dictionary<string, Frame> temporaryFrameDict = instantiateObjects.ConvertParentChildrenToRhinoFrameData(instantiateObjects.RuntimeObjectStorageObject, databaseManager.MyFramesDataDict);

        //Call the Database Manager Method and Reference to publish the data
        databaseManager.PushStringData(databaseManager.dbreferenceMyFramesData, JsonConvert.SerializeObject(temporaryFrameDict));
    }

    //TODO: Write a method that will reset everything (destroy game objects, and clear the dictionary)
    public void ResetDataButtonMethod()
    {
        Debug.Log("ResetDataButtonMethod: Reset Data Button Pressed");

        //Call the Database Manager Method and Reference to reset the data
        databaseManager.MyFramesDataDict.Clear();

        //Destroy all child objects of the runtime object storage object
        instantiateObjects.DestroyAllChildren(instantiateObjects.RuntimeObjectStorageObject);
    }

    //TODO: Write a method that will toggle on and off movement for the objects (tip: design it for the update method)
    public void ToggleObjectMovement(Toggle toggle)
    {
        if(toggle.isOn)
        {
            Debug.Log("ToggleObjectMovement: Objects should be moving now.");

            //Call Method from instantiateObjects to start moving objects
            instantiateObjects.MoveAllChildrenByVector(instantiateObjects.RuntimeObjectStorageObject);
        }
        else
        {
            Debug.Log("ToggleObjectMovement: Object Movement is now off.");
        }
    }

}

