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


public class DataItemDictEventArgs : EventArgs
{
    public Dictionary<string, Step> BuildingPlanDataDict { get; set; }
}

public class TrackingDataDictEventArgs : EventArgs
{
    public Dictionary<string, QRcode> QRCodeDataDict { get; set; }
}

public class UpdateDataItemsDictEventArgs : EventArgs
{
    public Step NewValue { get; set; }
    public string Key { get; set; }
}

public class ApplicationSettingsEventArgs : EventArgs
{
    public ApplicationSettings Settings { get; set; }
}

public class UpdateDatabaseReferenceEventArgs: EventArgs
{
    public DatabaseReference Reference { get; set; }
}
    
public class DatabaseManager : MonoBehaviour
{
    // Firebase database references
    public DatabaseReference dbreference_assembly;
    public DatabaseReference dbreference_buildingplan;
    public DatabaseReference dbreference_qrcodes;
    public DatabaseReference dbrefernece_currentstep;
    public StorageReference storageReference;


    // Data structures to store nodes and steps
    public Dictionary<string, Node> DataItemDict { get; private set; } = new Dictionary<string, Node>();
    public Dictionary<string, Step> BuildingPlanDataDict { get; private set; } = new Dictionary<string, Step>();
    public Dictionary<string, QRcode> QRCodeDataDict { get; private set; } = new Dictionary<string, QRcode>();


    //Data Structure to Store Application Settings
    public ApplicationSettings applicationSettings;


    // Define event delegates and events
    public delegate void StoreDataDictEventHandler(object source, DataItemDictEventArgs e); 
    public event StoreDataDictEventHandler DatabaseInitializedDict;

    public delegate void TrackingDataDictEventHandler(object source, TrackingDataDictEventArgs e); 
    public event TrackingDataDictEventHandler TrackingDictReceived;

    public delegate void UpdateDataDictEventHandler(object source, UpdateDataItemsDictEventArgs e); 
    public event UpdateDataDictEventHandler DatabaseUpdate;

    public delegate void StoreApplicationSettings(object source, ApplicationSettingsEventArgs e);
    public event StoreApplicationSettings ApplicationSettingUpdate;

    
    //Define HTTP request response classes
    class ListFilesResponse
    {
        public List<object> prefix { get; set; }
        public List<FileMetadata> items { get; set; }
    }

    class FileMetadata
    {
        public string name { get; set; }
        public string bucket { get; set; }
    }

    //Other Scripts
    public UIFunctionalities UIFunctionalities;

    //Define bool to set object orientation
    public bool objectOrientation;
    public string TempDatabaseCurrentStep;

    void Awake()
    {
        OnAwakeInitilization();
    }

/////////////////////// FETCH AND PUSH DATA /////////////////////////////////
    private void OnAwakeInitilization()
    {
        //Set Persistence: Disables storing information on the device for when there is no internet connection.
        FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);

        //Find UI Functionalities
        UIFunctionalities = GameObject.Find("UIFunctionalities").GetComponent<UIFunctionalities>();

    }
    public async void FetchSettingsData(DatabaseReference settings_reference)
    {
        //TODO: Add Await?
        settings_reference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error fetching data from Firebase");
                print("Error Fetching Settings Data");
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                DeserializeSettingsData(snapshot);
            }
        });

    }    
    public async void FetchData(object source, ApplicationSettingsEventArgs e)
    {
        //Create DB Reference Always
        dbreference_assembly = FirebaseDatabase.DefaultInstance.GetReference(e.Settings.parentname).Child("assembly").Child("graph").Child("node");
        dbreference_buildingplan = FirebaseDatabase.DefaultInstance.GetReference(e.Settings.parentname).Child("building_plan").Child("data").Child("steps");
        dbreference_qrcodes = FirebaseDatabase.DefaultInstance.GetReference(e.Settings.parentname).Child("QRFrames");
        dbrefernece_currentstep = FirebaseDatabase.DefaultInstance.GetReference(e.Settings.parentname).Child("currentstep");
        
        //If there is nothing to download Storage=="None" then trigger Objects Secured event
        if (e.Settings.storagename == "None")
        {
            //Fetch QR Data no event trigger
            FetchRTDData(dbreference_qrcodes, snapshot => DesearializeQRSnapshot(snapshot), "TrackingDict");
            
            //Fetch Assembly Data no event trigger
            FetchRTDData(dbreference_assembly, snapshot => DeserializeDataSnapshot(snapshot));

            //Fetch Building plan data with event trigger
            FetchRTDData(dbreference_buildingplan, snapshot => DesearialaizeStepSnapshot(snapshot), "DataitemDict");

        }
        
        //Else trigger download.
        else
        {
            //Set Obj Orientation bool
            objectOrientation = e.Settings.objorientation;
            
            //Storage Reference from data fetched
            storageReference = FirebaseStorage.DefaultInstance.GetReference("obj_storage").Child(e.Settings.storagename);
            string basepath = storageReference.Path;
            string path = basepath.Substring(1);
            Debug.Log($"Path for download on FB Storage: {path}");

            //Get a list of files from the storage location
            List<FileMetadata> files = await GetFilesInFolder(path);

            //Fetch Data from both storage and Realtime Database.
            FetchAllData(files);
        }
    }
    private async void FetchAllData(List<FileMetadata> files)
    {
        //Fetch Storage Data
        await FetchStorageData(files);

        //Fetch QR Data no event trigger
        FetchRTDData(dbreference_qrcodes, snapshot => DesearializeQRSnapshot(snapshot), "TrackingDict");
        
        //Fetch Assembly Data no event trigger
        FetchRTDData(dbreference_assembly, snapshot => DeserializeDataSnapshot(snapshot));
        
        //Fetch Building plan data with event trigger
        FetchRTDData(dbreference_buildingplan, snapshot => DesearialaizeStepSnapshot(snapshot), "DataitemDict");
    }
    async Task<List<FileMetadata>> GetFilesInFolder(string path)
    {
        //This will need to change
        string storageBucket = FirebaseManager.Instance.storageBucket;
        string baseUrl = $"https://firebasestorage.googleapis.com/v0/b/{storageBucket}/o?prefix={path}/&delimiter=/";

        // const string baseUrl = "https://firebasestorage.googleapis.com/v0/b/test-project-94f41.appspot.com/o?prefix=obj_storage/buildingplan_test/&delimiter=/"; //Hardcoded value for example
 
        Debug.Log($"BaseUrl: {baseUrl}");

        // Send a GET request to the URL
        using (HttpClient client = new HttpClient())
        using (HttpResponseMessage response = await client.GetAsync(baseUrl))
        using (HttpContent content = response.Content)
        {
            
            // Read the response body
            string responseText = await content.ReadAsStringAsync();
            Debug.Log($"HTTP Client Response: {responseText}");

            // Deserialize the JSON response
            ListFilesResponse responseData = JsonConvert.DeserializeObject<ListFilesResponse>(responseText);

            // Return the list of files
            return responseData.items;
        }
    }
    async Task FetchStorageData(List<FileMetadata> files) 
    {
        List<Task> downloadTasks = new List<Task>();
        
        foreach (FileMetadata file in files)
        {    
            string baseurl = file.name;

            //Construct FirebaseStorage Reference from 
            StorageReference FileReference = FirebaseStorage.DefaultInstance.GetReference(baseurl);
            string basepath = Application.persistentDataPath;
            string filename = Path.GetFileName(baseurl);
            string folderpath = Path.Combine(basepath, "Object_Storage");
            string savefilepath = Path.Combine(folderpath, filename);
                                
            // Replace backslashes with forward slashes
            savefilepath = savefilepath.Replace('\\', '/');

            //Run all async tasks and add them to a task list we can wait for all of them to complete.            
            downloadTasks.Add(FileReference.GetFileAsync(savefilepath).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    foreach (var exception in task.Exception.InnerExceptions)
                    {
                        Debug.LogError("Error fetching data from Firebase: " + exception.Message);
                    }
                    return;
                }

                if (task.IsCompleted)
                {
                    Debug.Log($"Downloaded file to path '{savefilepath}'");
                    CheckPathExistance(savefilepath);
                }
            }));
        }
        
        //Await all download tasks are done before refreshing.
        await Task.WhenAll(downloadTasks);
    }      
    
    //TODO: CHANGED TO TASK FOR AWAITABLE... CURRENTLY NOT AWAITED IN MOST PLACES BECAUSE WE WERE AWAITING INSIDE THE METHOD, BUT STILL TOO SLOW FOR CURRENTSTEP.
    public async Task FetchRTDData(DatabaseReference dbreference, Action<DataSnapshot> customAction, string eventname = null)
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

        if (eventname != null && eventname == "DataitemDict")
        {
            OnDatabaseInitializedDict(BuildingPlanDataDict); 
        }

        if (eventname != null && eventname == "TrackingDict")
        {
            OnTrackingDataReceived(QRCodeDataDict);
        }
    }      
    public void PushAllData(DatabaseReference dbref, string Data)
    {
        dbref.SetRawJsonValueAsync(Data);
    }

/////////////////////////// DATA DESERIALIZATION ///////////////////////////////////////
    private void DeserializeSettingsData(DataSnapshot snapshot)
    {
        CleanObjectStorageFolder();
        string AppData = snapshot.GetRawJsonValue();

        if (!string.IsNullOrEmpty(AppData))
        {
            Debug.Log("Application Settings:" + AppData);
            applicationSettings = JsonConvert.DeserializeObject<ApplicationSettings>(AppData);
        }
        else
        {
            Debug.LogWarning("You did not set your settings data properly");
        }
    
        OnSettingsUpdate(applicationSettings);
    } 
    private void DeserializeDataSnapshot(DataSnapshot snapshot)
    {
        DataItemDict.Clear();

        foreach (DataSnapshot childSnapshot in snapshot.Children)
        {
            string key = childSnapshot.Key;
            var json_data = childSnapshot.GetValue(true);
            Node node_data = NodeDeserializer(key, json_data);
            
            if (IsValidNode(node_data))
            {
                DataItemDict[key] = node_data;
                DataItemDict[key].type_id = key;
            }
            else
            {
                Debug.LogWarning($"Invalid Node structure for key '{key}'. Not added to the dictionary.");
            }
        }
        
        Debug.Log("Number of nodes stored as a dictionary = " + DataItemDict.Count);

    }
    private void DesearialaizeStepSnapshot(DataSnapshot snapshot)
    {
        BuildingPlanDataDict.Clear();

        foreach (DataSnapshot childSnapshot in snapshot.Children)
        {
            string key = childSnapshot.Key;
            var json_data = childSnapshot.GetValue(true);
            Step step_data = StepDeserializer(key, json_data);
            
            if (IsValidStep(step_data))
            {
                BuildingPlanDataDict[key] = step_data;
            }
            else
            {
                Debug.LogWarning($"Invalid Step structure for key '{key}'. Not added to the dictionary.");
            }
        }

        Debug.Log("Number of steps stored as a dictionary = " + BuildingPlanDataDict.Count);
    }
    private void DesearializeQRSnapshot(DataSnapshot snapshot)
    {
        foreach (DataSnapshot childSnapshot in snapshot.Children)
        {    
            string key = childSnapshot.Key;
            string jsondatastring = childSnapshot.GetRawJsonValue();
            
            if (!string.IsNullOrEmpty(jsondatastring))
            {
                Debug.Log("QR Code Data:" + jsondatastring);
                QRcode newValue = JsonConvert.DeserializeObject<QRcode>(jsondatastring);
                QRCodeDataDict[key] = newValue;
                QRCodeDataDict[key].Key = key;
            }
            else
            {
                Debug.LogWarning("You did not set your QR Code data properly");
            }
        }

    }
    private void DesearializeCurrentStep(DataSnapshot snapshot)
    {
        Debug.Log("I am inside the Current Element Changed Event");
  
        string jsondatastring = snapshot.GetRawJsonValue();
        Debug.Log("Current Element Data:" + jsondatastring);
        
        if (!string.IsNullOrEmpty(jsondatastring))
        {
            TempDatabaseCurrentStep = jsondatastring;
        }
        else
        {
            Debug.LogWarning("Current Element Did not produce a value");
            TempDatabaseCurrentStep = null;
        }
    }

/////////////////////////// INTERNAL DATA MANAGERS //////////////////////////////////////
    private void CleanObjectStorageFolder()
    {
        //Construct storage folder path
        string path = Application.persistentDataPath;
        string folderpath = Path.Combine(path, "Object_Storage");
        folderpath = folderpath.Replace('\\', '/');

        //If the folder exists delete all files in the folder. If not create the folder.
        if (Directory.Exists(folderpath))
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(folderpath);
            foreach (FileInfo file in di.GetFiles())
            {
                Debug.Log("Deleted Files: " + file);
                file.Delete();
            }
        }
        else
        {
            Directory.CreateDirectory(folderpath);
            print($"Created Directory for Object Storage @ {folderpath}");
        }

    }
    private bool IsValidNode(Node node)
    {   
        // Basic validation: Check if the required properties are present or have valid values
        if (node != null &&
            !string.IsNullOrEmpty(node.type_id) &&
            !string.IsNullOrEmpty(node.type_data) &&
            node.part != null &&
            node.part.frame != null &&
            node.attributes.length != null &&
            node.attributes.width != null &&
            node.attributes.height != null)
        {
            // Set default values for properties that may be null
            return true;
        }
        Debug.Log($"node.type_id is: '{node.type_id}'");
        return false;
    }
    private bool IsValidStep(Step step)
    {
        // Basic validation: Check if the required properties are present or have valid values
        if (step != null &&
            step.data.element_ids != null &&
            !string.IsNullOrEmpty(step.data.actor) &&
            step.data.location != null &&
            step.data.geometry != null &&
            step.data.instructions != null &&
            step.data.is_built != null &&
            step.data.is_planned != null &&
            step.data.elements_held != null &&
            step.data.priority != null)
        {
            // Set default values for properties that may be null
            return true;
        }
        Debug.Log($"node.key is: '{step.data.element_ids[0]}'");
        return false;
    }
    public string print_out_data(DatabaseReference dbreference_assembly)
    {
        string jsondata = "";
        dbreference_assembly.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("error");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot dataSnapshot = task.Result;

                // Raw JSON string of everything inside node
                jsondata = dataSnapshot.GetRawJsonValue(); 
                Debug.Log("all nodes" + jsondata);
            }
        });
        return jsondata;
    }
    public void CheckPathExistance(string path)
    {       
        // Replace backslashes with forward slashes
        path = path.Replace('\\', '/');

        if (File.Exists(path))
        {
            Debug.Log($"File Exists @ {path}");
        }
        else
        {
            Debug.Log($"File does not exist @ {path}");
        }

    }

/////////////////////////////// Input Data Handlers //////////////////////////////////
    public Node NodeDeserializer(string key, object jsondata)
    {
        Dictionary<string, object> jsonDataDict = jsondata as Dictionary<string, object>;

        // Access nested values 
        Dictionary<string, object> partDict = jsonDataDict["part"] as Dictionary<string, object>;
        Dictionary<string, object> dataDict = partDict["data"] as Dictionary<string, object>;
        Dictionary<string, object> frameDataDict = dataDict["frame"] as Dictionary<string, object>;

        //Create class instances of node elements
        Node node = new Node();
        node.part = new Part();
        node.attributes = new Attributes();
        node.part.frame = new Frame();

        //Try get value type to ignore joints
        if (jsonDataDict.TryGetValue("type", out object type))
        {
            if((string)jsonDataDict["type"] == "joint")
            {
                node.type_id = key; 
                Debug.Log("This is a joint");
                return node;
            }
            Debug.Log($"type is: {type}");
        }

        //Set values for base node class //TODO: Add try get value for safety?
        node.type_id = jsonDataDict["type_id"].ToString();
        node.type_data = jsonDataDict["type_data"].ToString();

        //Convert System.double items to float for use in instantiation
        List<object> pointslist = frameDataDict["point"] as List<object>;
        List<object> xaxislist = frameDataDict["xaxis"] as List<object>;
        List<object> yaxislist = frameDataDict["yaxis"] as List<object>;

        if (pointslist != null && xaxislist != null && yaxislist != null)
        {
            node.part.frame.point = pointslist.Select(Convert.ToSingle).ToArray();
            node.part.frame.xaxis = xaxislist.Select(Convert.ToSingle).ToArray();
            node.part.frame.yaxis = yaxislist.Select(Convert.ToSingle).ToArray();
        }
        else
        {
            Debug.Log("One of the Frame lists is null");
        }
                
        //Add Items to the attributes dictionary depending on the type of geometry
        GeometricDesctiptionSelector(node.type_data, dataDict, node);

        //Set Attributes Class Values
        node.attributes.is_built = (bool)jsonDataDict["is_built"];
        node.attributes.is_planned =  (bool)jsonDataDict["is_planned"];
        node.attributes.placed_by = (string)jsonDataDict["placed_by"];
        
        return node;
    }
    private void GeometricDesctiptionSelector(string type_data, Dictionary<string, object> jsonDataDict, Node node)
    {
        switch (type_data)
        {
            case "0.Cylinder":
                // Accessing different parts of json data to make common attributes dictionary
                float height = Convert.ToSingle(jsonDataDict["height"]);
                float radius = Convert.ToSingle(jsonDataDict["radius"]);

                //Add Items to the attributes dictionary remapping name to length, width, height
                node.attributes.length = radius;
                node.attributes.width = radius;
                node.attributes.height = height;
                break;

            case "1.Box":
                // Accessing different parts of json data to make common attributes dictionary
                float xsize = Convert.ToSingle(jsonDataDict["xsize"]);
                float ysize = Convert.ToSingle(jsonDataDict["ysize"]);
                float zsize = Convert.ToSingle(jsonDataDict["zsize"]);

                //Add Items to the attributes dictionary remapping name to length, width, height
                node.attributes.length = xsize;
                node.attributes.width = ysize;
                node.attributes.height = zsize;
                break;

            case "2.ObjFile":
                //TODO: THIS ONLY WORKS BECAUSE IT IS SPECIFICALLY SET FOR TIMBERS. NEED TO MAKE THIS MORE GENERIC
                // Accessing different parts of json data to make common attributes dictionary
                float objLength = Convert.ToSingle(jsonDataDict["length"]);
                float objWidth = Convert.ToSingle(jsonDataDict["width"]);
                float objHeight = Convert.ToSingle(jsonDataDict["height"]);

                //Add Items to the attributes dictionary remapping name to length, width, height
                node.attributes.length = objLength;
                node.attributes.width = objWidth;
                node.attributes.height = objHeight;
                break;
                
            case "3.Mesh":
                Debug.Log("Mesh");
                break;

            default:
                Debug.Log("Default");
                break;
        }
    }
    public Step StepDeserializer(string key, object jsondata)
    {
        Dictionary<string, object> jsonDataDict = jsondata as Dictionary<string, object>;

        //Create class instances of node elements
        Step step = new Step();
        step.data = new Data();
        step.data.location = new Frame();

        //Set values for base node class to keep data structure consistent
        step.dtype = (string)jsonDataDict["dtype"];
        step.guid = (string)jsonDataDict["guid"];

        //Access nested information
        Dictionary<string, object> dataDict = jsonDataDict["data"] as Dictionary<string, object>;
        Dictionary<string, object> locationDataDict = dataDict["location"] as Dictionary<string, object>;

        //Set values for step
        step.data.actor = (string)dataDict["actor"];
        step.data.geometry = (string)dataDict["geometry"];
        step.data.is_built = (bool)dataDict["is_built"];
        step.data.is_planned = (bool)dataDict["is_planned"];
        step.data.priority = (System.Int64)dataDict["priority"];

        //TODO: CHECKING DIVICE ID
        // if ((string)dataDict["device_id"] == "GH-PC"))
        // {
        //     step.data.device_id = SystemInfo.deviceUniqueIdentifier;
        // }    
        // else
        // {
        //     step.data.device_id = (string)dataDict["device_id"];
        // }
        
        //List Conversions System.double items to float for use in instantiation & Int64 to int & Object to string
        List<object> pointslist = locationDataDict["point"] as List<object>;
        List<object> xaxislist = locationDataDict["xaxis"] as List<object>;
        List<object> yaxislist = locationDataDict["yaxis"] as List<object>;
        List<object> element_ids = dataDict["element_ids"] as List<object>;
        List<object> instructions = dataDict["instructions"] as List<object>;
        List<object> elements_held = dataDict["elements_held"] as List<object>;

        if (pointslist != null &&
            xaxislist != null &&
            yaxislist != null &&
            element_ids != null &&
            instructions != null &&
            elements_held != null)
        {
            step.data.location.point = pointslist.Select(Convert.ToSingle).ToArray();
            step.data.location.xaxis = xaxislist.Select(Convert.ToSingle).ToArray();
            step.data.location.yaxis = yaxislist.Select(Convert.ToSingle).ToArray();
            step.data.elements_held = elements_held.Select(Convert.ToInt32).ToArray();
            step.data.element_ids = element_ids.Select(x => x.ToString()).ToArray();
            step.data.instructions = instructions.Select(x => x.ToString()).ToArray();
        }
        else
        {
            Debug.Log("One of the Location lists is null");
        }

        return step;
    }

/////////////////////////////// EVENT HANDLING ////////////////////////////////////////

    // Add a listener for firebase child events
    public void AddListeners(object source, EventArgs args)
    {        
        //Add Listners for the building plan
        dbreference_buildingplan.ChildAdded += OnChildAdded;
        dbreference_buildingplan.ChildChanged += OnChildChanged;
        dbreference_buildingplan.ChildRemoved += OnChildRemoved;
        
        //Add Listners for the Assembly //TODO: NEED TO FIND A WAY TO NOT PULL THE INFORMATION ON THE UNAVOIDABLE FIRST CALL
        dbreference_assembly.ChildAdded += OnAssemblyChanged;
        dbreference_assembly.ChildChanged += OnAssemblyChanged;
        dbreference_assembly.ChildRemoved += OnAssemblyChanged;

        //Add Listners for the Assembly //TODO: NEED TO FIND A WAY TO NOT PULL THE INFORMATION ON THE UNAVOIDABLE FIRST CALL
        // dbreference_qrcodes.ChildAdded += OnQRChanged;
        // dbreference_qrcodes.ChildChanged += OnQRChanged;
        // dbreference_qrcodes.ChildRemoved += OnQRChanged;

        //Add Listners for the Current Element //TODO: CHECK IF THIS GETS ADDED ON THE FIRST TIME THROUGH
        dbrefernece_currentstep.ValueChanged += OnCurrentElementChanged;
        // dbrefernece_currentstep.ChildAdded += OnCurrentElementChanged;
        // dbrefernece_currentstep.ChildChanged += OnCurrentElementChanged;
        // dbrefernece_currentstep.ChildRemoved += OnCurrentElementChanged;

    }

    // Event handler for child changes    
    public void OnChildAdded(object sender, Firebase.Database.ChildChangedEventArgs args) 
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        var key = args.Snapshot.Key;
        var childSnapshot = args.Snapshot.GetValue(true);
        Debug.Log($"ON CHILD ADDED {key}");

        if (childSnapshot != null)
        {
            Step newValue = StepDeserializer(key, childSnapshot);
           
            //make a new entry in the dictionary if it doesnt already exist
            if (IsValidStep(newValue))
            {
                if (DataItemDict.ContainsKey(key))
                {
                    Debug.Log("The key already exists in the dictionary");
                }
                else
                {
                    Debug.Log($"The key '{key}' does not exist in the dictionary");
                    BuildingPlanDataDict.Add(key, newValue);
                    // BuildingPlanDataDict[key].data.element_ids[0] = key;

                    //Instantiate new object
                    OnDatabaseUpdate(newValue, key);
                }
            }
            else
            {
                Debug.LogWarning($"Invalid Step structure for key '{key}'. Not added to the dictionary.");
            }
        }
    }
    
    public void OnChildChanged(object sender, Firebase.Database.ChildChangedEventArgs args) 
    {
        if (args.DatabaseError != null) {
        Debug.LogError($"Database error: {args.DatabaseError}");
        return;
        }

        if (args.Snapshot == null) {
            Debug.LogWarning("Snapshot is null. Ignoring the child change.");
            return;
        }

        string key = args.Snapshot.Key;
        var childSnapshot = args.Snapshot.GetValue(true);
        Debug.Log($"ON CHILD CHANGED {key}");

        if (childSnapshot != null)
        {
            Step newValue = StepDeserializer(key, childSnapshot);
            int keyInt = Convert.ToInt32(key);
            int currentStepint = Convert.ToInt32(UIFunctionalities.CurrentStep);
            Debug.Log($"HERE: Key: {keyInt} Current Step: {currentStepint}");
            
            //SAFETY CHECKS FOR BUTTON FUNCTIONS WRITING.
            //If key is lower then current element and the next button was pressed then do nothing: COULD DO JUST BOOL.
            //TODO: THIS CAN BE SIMPLIFIED BY DEVICE ID CHECKING
            if (keyInt < currentStepint && UIFunctionalities.NextButtonPressed == true)
            {
                UIFunctionalities.NextButtonPressed = false;
                Debug.Log("Key is lower then the current step and you pressed the next button. So i will not place another object.");
                return;
            }
            //If key is higher then current element and the previous button was pressed then do nothing: COULD DO JUST BOOL.
            else if (keyInt == currentStepint && UIFunctionalities.PreviousButtonClicked == true)
            {
                UIFunctionalities.PreviousButtonClicked = false;
                Debug.Log("Key is == to the current step and previous button was pressed. So i will not place another object.");
                return;
            } 

            else
            {    
                if(IsValidStep(newValue))
                {
                    //TODO: NewValue.device_id = this device id;
                    BuildingPlanDataDict[key] = newValue;
                }
                else
                {
                    Debug.LogWarning($"Invalid Node structure for key '{key}'. Not added to the dictionary.");
                }

                Debug.Log($"HandleChildChanged - The key of the changed Step is {key}");
                Debug.Log("newData[key] = " + BuildingPlanDataDict[key]);
                
                //Instantiate new object
                OnDatabaseUpdate(newValue, key);
            }
        }
    }
    public void OnChildRemoved(object sender, Firebase.Database.ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        string key = args.Snapshot.Key;
        string childSnapshot = args.Snapshot.GetRawJsonValue();

        if (!string.IsNullOrEmpty(childSnapshot))
        {
            //remove an entry in the dictionary
            if (BuildingPlanDataDict.ContainsKey(key))
            {
                Step newValue = null;
                Debug.Log("The key exists in the dictionary and is going to be removed");
                BuildingPlanDataDict.Remove(key);
                OnDatabaseUpdate(newValue, key);
            }
            else
            {
                Debug.Log("The key does not exist in the dictionary");
            }
        }
    }  
    public void OnAssemblyChanged(object sender, Firebase.Database.ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null) {
        Debug.LogError($"Database error: {args.DatabaseError}");
        return;
        }

        if (args.Snapshot == null) {
            Debug.LogWarning("Snapshot is null. Ignoring the child change.");
            return;
        }
        
        Debug.Log("Assembly Changed");
        FetchRTDData(dbreference_assembly, snapshot => DeserializeDataSnapshot(snapshot));
    }
    public void OnQRChanged(object sender, Firebase.Database.ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null) {
        Debug.LogError($"Database error: {args.DatabaseError}");
        return;
        }

        if (args.Snapshot == null) {
            Debug.LogWarning("Snapshot is null. Ignoring the child change.");
            return;
        }
        
        Debug.Log("QRCodes Changed");
        FetchRTDData(dbreference_qrcodes, snapshot => DesearializeQRSnapshot(snapshot), "TrackingDict");
    }

    public async void OnCurrentElementChanged(object sender, Firebase.Database.ValueChangedEventArgs args)
    {
       
        if (args.DatabaseError != null) {
        Debug.LogError($"Database error: {args.DatabaseError}");
        return;
        }

        if (args.Snapshot == null) {
            Debug.LogWarning("Snapshot is null. Ignoring the child change.");
            return;
        }
        
        Debug.Log("Current Element Changed");
        
        //Set Temp Current Element to null so that everytime an event is triggered it becomes null again and doesnt keep old data.
        TempDatabaseCurrentStep = null;
        
        await FetchRTDData(dbrefernece_currentstep, snapshot => DesearializeCurrentStep(snapshot));
    
        if (TempDatabaseCurrentStep != null && TempDatabaseCurrentStep != UIFunctionalities.CurrentStep)
        {
            Debug.Log($"Current Element Changed and is now: {TempDatabaseCurrentStep}");

            //Update Current Step method
            UIFunctionalities.OnCurrentStepChanged(TempDatabaseCurrentStep);
        }
        if(TempDatabaseCurrentStep == UIFunctionalities.CurrentStep)
        {
            Debug.Log($"Current Element Changed but is the same as the current step: {TempDatabaseCurrentStep}");
        }
        else
        {
            Debug.Log($"Retreived Element is: {TempDatabaseCurrentStep} and Current Element in app is: {UIFunctionalities.CurrentStep}");
            Debug.LogWarning("Current Element is null");
        }
    }
    

    // Event handling for database initialization
    protected virtual void OnDatabaseInitializedDict(Dictionary<string, Step> BuildingPlanDataDict)
    {
        UnityEngine.Assertions.Assert.IsNotNull(DatabaseInitializedDict, "Database dict is null!");
        DatabaseInitializedDict(this, new DataItemDictEventArgs() {BuildingPlanDataDict = BuildingPlanDataDict});
    }
    protected virtual void OnTrackingDataReceived(Dictionary<string, QRcode> QRCodeDataDict)
    {
        UnityEngine.Assertions.Assert.IsNotNull(TrackingDictReceived, "Tracking Dict is null!");
        Debug.Log("Tracking Data Received");
        TrackingDictReceived(this, new TrackingDataDictEventArgs() {QRCodeDataDict = QRCodeDataDict});
    }
    protected virtual void OnDatabaseUpdate(Step newValue, string key)
    {
        UnityEngine.Assertions.Assert.IsNotNull(DatabaseInitializedDict, "new dict is null!");
        DatabaseUpdate(this, new UpdateDataItemsDictEventArgs() {NewValue = newValue, Key = key });
    }
    protected virtual void OnSettingsUpdate(ApplicationSettings settings)
    {
        ApplicationSettingUpdate(this, new ApplicationSettingsEventArgs(){Settings = settings});
    }
}