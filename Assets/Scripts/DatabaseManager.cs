using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using JSON;
using ApplicationInfo;
using Firebase;
using Firebase.Storage;
using Firebase.Auth;
using System.IO;
using UnityEngine.Networking;
using System.Linq;
using System.Linq.Expressions;
using Unity.VisualScripting;
using Instantiate;
using Google.MiniJSON;
using Helpers;
using UnityEngine.InputSystem;


public class DatabaseManager : MonoBehaviour
{

    //MAS Class Modifiers

    // Firebase database references
    //TODO: 1. Set Database Reference
    public DatabaseReference dbreferenceMyFramesData;

    //TODO: Create Dictionary to store frames.
    public Dictionary<string, Frame> MyFramesDataDict { get; private set; } = new Dictionary<string, Frame>();

    //TODO: Store the InstantiateObjects script
    public InstantiateObjects instantiateObjects;

    void Awake()
    {
        //TODO: 1. Set Database Reference
        dbreferenceMyFramesData = FirebaseDatabase.DefaultInstance.GetReference("MyFramesData");

        //TODO: Find Other Scripts
        instantiateObjects = GameObject.Find("Instantiate").GetComponent<InstantiateObjects>();

        if (instantiateObjects == null)
        {
            Debug.LogError("InstantiateObjects script not found");
        }
    }

/////////////////////// FETCH AND PUSH DATA /////////////////////////////////

    //MAS METHODS

    //TODO: Write a method to fetch data from the Firebase Realtime Database
    public async Task FetchRealTimeDatabaseData(DatabaseReference dbreference)
    {
        //Fetch data task initiation
        await dbreference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            //Check task status
            if (task.IsFaulted)
            {
                Debug.LogError("Error fetching data from Firebase");
                return;
            }

            if (task.IsCompleted)
            {
                //Get the data snapshot from the DataSnapshot
                DataSnapshot snapshot = task.Result;
                
                Debug.Log("Data Fetched from Firebase: " + JsonConvert.SerializeObject(snapshot.GetRawJsonValue()));
                
                //Deserilize the frame data from the snapshot
                DeserilizeFrameData(snapshot, MyFramesDataDict);

            }
        });
    }

    //Example of a custom action for more flexibiliiy in what to do with the data.
    public async Task FetchRealTimeDatabaseDataCustomAction(DatabaseReference dbreference, Action<DataSnapshot> customAction, string eventname = null)
    {
        await dbreference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error fetching data from Firebase");
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (customAction != null)
                {
                    customAction(snapshot);
                }
            }
        });
    }

    //TODO: Write a method to push deserilize from the database.
    private void DeserilizeFrameData(DataSnapshot snapshot, Dictionary<string, Frame> frameDict)
    {
        //Clear current Dictionary if it contains information
        frameDict.Clear();

        //Desearialize individual data items from the snapshots
        foreach (DataSnapshot childSnapshot in snapshot.Children)
        {
            Debug.Log("Snapshot Children Count" + snapshot.ChildrenCount);
            //Get the key and value of the snapshot
            string key = childSnapshot.Key;

            //Get the json data from the snapshot
            var json_data = childSnapshot.GetValue(true);

            //Create a new instance of the class Frame
            Frame frame = FrameDeserilizer(json_data);
            
            //Add Frame to the dictionary
            frameDict[key] = frame; 
        }
        
        //Call the instantiation method from the InstantiateObjects script
        instantiateObjects.PlacePrefabsfromFrameDict(frameDict, instantiateObjects.PrefabObject, instantiateObjects.RuntimeObjectStorageObject);

        Debug.Log("DeserilizeFrameData: Number of Frames Stored In the Dictionary = " + frameDict.Count);
    }
    public Frame FrameDeserilizer(object frameData) //TODO: JOSEPH: MAYBE USE PARSE METHOD INSTEAD OF THIS JUST TO PLAY IT SAFE.
    {
        //Cast jsondata to a dictionary
        Dictionary<string, object> frameDataDict = frameData as Dictionary<string, object>;

        //Create a new instance of the class
        Frame frame = new Frame();

        //Cast values to list Object so they can be converted to float arrays
        List<object> pointslist = frameDataDict["point"] as List<object>;
        List<object> xaxislist = frameDataDict["xaxis"] as List<object>;
        List<object> yaxislist = frameDataDict["yaxis"] as List<object>;
        
        if (pointslist != null && xaxislist != null && yaxislist != null)
        {
            frame.point = pointslist.Select(Convert.ToSingle).ToArray();
            frame.xaxis = xaxislist.Select(Convert.ToSingle).ToArray();
            frame.yaxis = yaxislist.Select(Convert.ToSingle).ToArray();
        }
        else
        {
            Debug.LogError("FrameDeserilizer: One of the Frame lists is null");
        }
 
        return frame;
    }

    //TODO: Write a method to push data to the Firebase Realtime Database
    public void PushStringData(DatabaseReference db_ref, string data)
    {
        db_ref.SetRawJsonValueAsync(data);
    }

}