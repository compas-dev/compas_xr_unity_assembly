using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Firebase.Database;
using Firebase.Extensions;
using JSON;
using System.Linq;
using Instantiate;

//Notes: The entirity of the file is structured as a class that inherits from MonoBehaviour (Unity's base class for scripts)
//Notes: Script is used for managing databaseRefrences, pushing, pulling, and Storing of data from the database.

public class DatabaseManager : MonoBehaviour
{
    //Notes: Class Member Variables, used to store data and references ex. public DatabaseReference dbreferenceMyFramesData;
    //Notes: The public keyword is named an access modifier, it determines how Member variables, methods, and classes can be accessed from other classes.

    //TODO: MAS: Set Database Reference
    public DatabaseReference dbreferenceMyFramesData;

    //TODO: MAS: Create Dictionary to store frames.
    public Dictionary<string, Frame> MyFramesDataDict { get; private set; } = new Dictionary<string, Frame>();

    //TODO: MAS: Store the InstantiateObjects script
    public InstantiateObjects instantiateObjects;

    //Notes: Built in Unity methods (methods that come from the inheritance of the MonoBehaviour class)
    /*
        Notes: Awake is called when the script instance is being loaded. This occurs before any Start methods are called.
        Notes: Start is called on the frame when a script is enabled just before any of the Update methods are called the first time.
        Notes: Update is called every frame, if the MonoBehaviour is enabled.
        Notes: OnDestroy is called when the MonoBehaviour will be destroyed.
        Notes: OnEnable is called when the object becomes enabled and active.
    */
    void Awake()
    {
        //TODO: MAS: Call OnAwakeInitilization Method
        OnAwakeInitilization();
    }

    ////////////////// General Structureing and Organization ////////////////////

    //TODO:  MAS: Write a method that will run on awake to do file set up.
    public void OnAwakeInitilization()
    {
        //TODO: MAS: Set Database Reference
        dbreferenceMyFramesData = FirebaseDatabase.DefaultInstance.GetReference("MyFramesData");

        //TODO: MAS: Find Other Scripts
        instantiateObjects = GameObject.Find("Instantiate").GetComponent<InstantiateObjects>();

    }

    /////////////////////// FETCH AND PUSH DATA /////////////////////////////////

    //TODO: MAS: Write a method to fetch data from the Firebase Realtime Database
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

    //Notes: Example of a custom action for more flexibiliiy in what to do with the data.
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

    //TODO:  MAS: Write a method to push deserilize from the database.
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

    /////////////////////// PROCESS DATA /////////////////////////////////

    //TODO: MAS: Write a custom method that will deserilize the frame data from the snapshot.
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

    //TODO: MAS: Write a method to push data to the Firebase Realtime Database
    public void PushStringData(DatabaseReference db_ref, string data)
    {
        db_ref.SetRawJsonValueAsync(data);
    }

}