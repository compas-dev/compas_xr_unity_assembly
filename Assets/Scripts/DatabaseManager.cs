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

    // MAS: 1. Set Class Member Variables to store the database reference...
    public DatabaseReference MyFramesReference;

    //TODO: MAS: 3. Set Class Member Variables to store the dictionary...
    public Dictionary<string, Frame> MyFramesDictionary = new Dictionary<string, Frame>();

    //TODO: MAS: Create Class Member Varible to store the instantiate objects script
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
        // MAS: 1. Set Database Reference
        MyFramesReference = FirebaseDatabase.DefaultInstance.GetReference("MyFramesData");

        //TODO: MAS: 5. Find Other Scripts
        instantiateObjects = GameObject.Find("InstantiateObjects").GetComponent<InstantiateObjects>();

    }

    /////////////////////// FETCH AND PUSH DATA /////////////////////////////////

    //Notes: Method to fetch data from the Firebase Realtime Database
    //TODO: MAS: 3. Deserilize the frame data from the snapshot
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
                
                //Print Out the snapshot
                Debug.Log($"My Snapshot: {snapshot}");

                //Use the Deserlize Frame Method
                DeserilizeFrameData(snapshot, MyFramesDictionary);
            }
        });
    }

    //Notes: A method for deserilizing data from the database.
    private void DeserilizeFrameData(DataSnapshot snapshot, Dictionary<string, Frame> frameDict)
    {
        //Clear current Dictionary if it contains information
        if(frameDict.Count > 0)
        {
            frameDict.Clear();
        }

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

            Debug.Log($"DeserilizeFrameData: Frame {key} added to My Frames Dictionary");
        }
        
        //TODO: MAS: 5. Call the instantiation method from the InstantiateObjects script
        instantiateObjects.PlacePrefabsfromFrameDict(frameDict, instantiateObjects.MyPrefabObject, instantiateObjects.MyRuntimeParentStorageObject);

        Debug.Log("DeserilizeFrameData: Number of Frames Stored In the Dictionary = " + frameDict.Count);
    }

    /////////////////////// PROCESS DATA /////////////////////////////////

    //Method that will deserilize the frame data from the snapshot.
    public Frame FrameDeserilizer(object frameData)
    {
        //Cast jsondata to a dictionary
        Dictionary<string, object> frameDataDict = frameData as Dictionary<string, object>;

        //Create a new instance of the class
        Frame frame = Frame.Parse(frameDataDict);

        //Check if the frame is not null
        if (frame != null)
        {
            Debug.Log("FrameDeserilizer: Frame Deserilized");
        }
        else
        {
            Debug.LogError("FrameDeserilizer: One of the Frame lists is null");
        }
 
        return frame;
    }

    //Method to push string data to the database.
    public void PushStringData(DatabaseReference db_ref, string data)
    {
        db_ref.SetRawJsonValueAsync(data);
    }

}