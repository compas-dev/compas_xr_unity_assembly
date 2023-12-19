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
using System.Diagnostics;
using UnityEngine.Networking;
using System.Linq;
using System.Linq.Expressions;

public class DataItemDictEventArgs : EventArgs
{
    public Dictionary<string, Step> BuildingPlanDataDict { get; set; }
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
    public GameObject TagsPrefab;

    // Firebase database references
    private DatabaseReference dbreference_assembly;
    private DatabaseReference dbreference_buildingplan;
    private StorageReference storageReference;


    // Data structures to store nodes and steps
    public Dictionary<string, Node> DataItemDict { get; private set; } = new Dictionary<string, Node>();
    public Dictionary<string, Step> BuildingPlanDataDict { get; private set; } = new Dictionary<string, Step>();


    //Data Structure to Store Application Settings
    public ApplicationSettings applicationSettings;

    // Define event delegates and events
    public delegate void StoreDataDictEventHandler(object source, DataItemDictEventArgs e); 
    public event StoreDataDictEventHandler DatabaseInitializedDict;
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

    void Awake()
    {
        //Set Persistence: Disables storing information on the device for when there is no internet connection.
        FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
    }

/////////////////////// FETCH AND PUSH DATA /////////////////////////////////
    public async void FetchSettingsData(DatabaseReference settings_reference)
    {
        //TODO: Add Await?
        settings_reference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                UnityEngine.Debug.LogError("Error fetching data from Firebase");
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

        //If there is nothing to download Storage=="None" then trigger Objects Secured event
        if (e.Settings.storagename == "None")
        {
            //Fetch Assembly Data no event trigger
            FetchRTDData(dbreference_assembly, snapshot => DeserializeDataSnapshot(snapshot), false);

            //Fetch Building plan data with event trigger
            FetchRTDData(dbreference_buildingplan, snapshot => DesearialaizeStepSnapshot(snapshot), true);
        }
        
        //Else trigger download.
        else
        {
            //Storage Reference from data fetched
            storageReference = FirebaseStorage.DefaultInstance.GetReference("obj_storage").Child(e.Settings.storagename);
            string basepath = storageReference.Path;
            string path = basepath.Substring(1);
            UnityEngine.Debug.Log($"Path for download on FB Storage: {path}");

            //Get a list of files from the storage location
            List<FileMetadata> files = await GetFilesInFolder(path);

            //Fetch Data from both storage and Realtime Database.
            FetchAllData(files, dbreference_assembly);
        }
    }
    private async void FetchAllData(List<FileMetadata> files, DatabaseReference dbref)
    {
        //Fetch Storage Data
        await FetchStorageData(files);

        //Fetch Assembly Data no event trigger
        FetchRTDData(dbref, snapshot => DeserializeDataSnapshot(snapshot), false);
        
        //Fetch Building plan data with event trigger
        FetchRTDData(dbreference_buildingplan, snapshot => DesearialaizeStepSnapshot(snapshot), true);
    }
    async Task<List<FileMetadata>> GetFilesInFolder(string path)
    {
        //This will need to change
        string storageBucket = FirebaseManager.Instance.storageBucket;
        string baseUrl = $"https://firebasestorage.googleapis.com/v0/b/{storageBucket}/o?prefix={path}/&delimiter=/";

        // const string baseUrl = "https://firebasestorage.googleapis.com/v0/b/test-project-94f41.appspot.com/o?prefix=obj_storage/buildingplan_test/&delimiter=/"; //Hardcoded value for example
 
        UnityEngine.Debug.Log($"BaseUrl: {baseUrl}");

        // Send a GET request to the URL
        using (HttpClient client = new HttpClient())
        using (HttpResponseMessage response = await client.GetAsync(baseUrl))
        using (HttpContent content = response.Content)
        {
            
            // Read the response body
            string responseText = await content.ReadAsStringAsync();
            UnityEngine.Debug.Log($"HTTP Client Response: {responseText}");

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
                        UnityEngine.Debug.LogError("Error fetching data from Firebase: " + exception.Message);
                    }
                    return;
                }

                if (task.IsCompleted)
                {
                    UnityEngine.Debug.Log($"Downloaded file to path '{savefilepath}'");
                    CheckPathExistance(savefilepath);
                }
            }));
        }
        
        //Await all download tasks are done before refreshing.
        await Task.WhenAll(downloadTasks);
    }      
    public async void FetchRTDData(DatabaseReference dbreference, Action<DataSnapshot> customAction, bool eventtrigger)
    {
        await dbreference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                UnityEngine.Debug.LogError("Error fetching data from Firebase");
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

        if (eventtrigger)
        {
            OnDatabaseInitializedDict(BuildingPlanDataDict); 
        }
    }      
    public void PushAllData(Dictionary<string, Step> BuildingPlanDataDict) 
    {
        dbreference_buildingplan.SetRawJsonValueAsync(JsonConvert.SerializeObject(BuildingPlanDataDict));
    }

/////////////////////////// DATA DESERIALIZATION ///////////////////////////////////////
    private void DeserializeSettingsData(DataSnapshot snapshot)
    {
        CleanObjectStorageFolder();
        string AppData = snapshot.GetRawJsonValue();

        if (!string.IsNullOrEmpty(AppData))
        {
            UnityEngine.Debug.Log("Application Settings:" + AppData);
            applicationSettings = JsonConvert.DeserializeObject<ApplicationSettings>(AppData);
        }
        else
        {
            UnityEngine.Debug.LogWarning("You did not set your settings data properly");
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
            Node node_data = NodeDesearializer(key, json_data);
            
            if (IsValidNode(node_data))
            {
                DataItemDict[key] = node_data;
                DataItemDict[key].type_id = key;
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Invalid Node structure for key '{key}'. Not added to the dictionary.");
            }
        }
        
        UnityEngine.Debug.Log("Number of nodes stored as a dictionary = " + DataItemDict.Count);

    }
    private void DesearialaizeStepSnapshot(DataSnapshot snapshot)
    {
        BuildingPlanDataDict.Clear();

        foreach (DataSnapshot childSnapshot in snapshot.Children)
        {
            string key = childSnapshot.Key;
            var json_data = childSnapshot.GetValue(true);
            Step step_data = StepDesearilizer(key, json_data);
            
            if (IsValidStep(step_data))
            {
                BuildingPlanDataDict[key] = step_data;
                BuildingPlanDataDict[key].data.element_ids[0] = key;
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Invalid Step structure for key '{key}'. Not added to the dictionary.");
            }
        }

        UnityEngine.Debug.Log("Number of steps stored as a dictionary = " + BuildingPlanDataDict.Count);
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
                UnityEngine.Debug.Log("Deleted Files: " + file);
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
        UnityEngine.Debug.Log($"node.key is: '{node.type_id}'");
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
        UnityEngine.Debug.Log($"node.key is: '{step.data.element_ids}'");
        return false;
    }
    public string print_out_data(DatabaseReference dbreference_assembly)
    {
        string jsondata = "";
        dbreference_assembly.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                UnityEngine.Debug.Log("error");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot dataSnapshot = task.Result;

                // Raw JSON string of everything inside node
                jsondata = dataSnapshot.GetRawJsonValue(); 
                UnityEngine.Debug.Log("all nodes" + jsondata);
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
            UnityEngine.Debug.Log($"File Exists @ {path}");
        }
        else
        {
            UnityEngine.Debug.Log($"File does not exist @ {path}");
        }

    }

/////////////////////////////// Input Data Handlers //////////////////////////////////
  public Node NodeDesearializer(string key, object jsondata)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add(key, jsondata);

        Dictionary<string, object> jsonDataDict = dict[key] as Dictionary<string, object>;

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
                UnityEngine.Debug.Log("This is a joint");
                return node;
            }
            UnityEngine.Debug.Log($"type is: {type}");
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
            UnityEngine.Debug.Log("One of the Frame lists is null");
        }
                
        //Add Items to the attributes dictionary depending on the type of geometry
        GeometricDesctiptionSelector(node.type_data, dataDict, node);

        //Set Attributes Class Values //TODO: Add try get value for safety?
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
                UnityEngine.Debug.Log("Mesh");
                break;

            default:
                UnityEngine.Debug.Log("Default");
                break;
        }
    }
    public Step StepDesearilizer(string key, object jsondata)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add(key, jsondata);

        Dictionary<string, object> jsonDataDict = dict[key] as Dictionary<string, object>;

        //Create class instances of node elements
        Step step = new Step();
        step.data = new Data();
        step.data.location = new Frame();

        //Access nested information
        Dictionary<string, object> dataDict = jsonDataDict["data"] as Dictionary<string, object>;
        Dictionary<string, object> locationDataDict = dataDict["location"] as Dictionary<string, object>;

        //Set values for step //TODO: Add try get value for safety?
        step.data.actor = (string)dataDict["actor"];
        step.data.geometry = (string)dataDict["geometry"];
        step.data.is_built = (bool)dataDict["is_built"];
        step.data.is_planned = (bool)dataDict["is_planned"];
        step.data.priority = (System.Int64)dataDict["priority"];

        
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
            UnityEngine.Debug.Log("One of the Location lists is null");
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
        
        //Add Listners for the Assembly
        dbreference_assembly.ChildAdded += OnAssemblyChanged;
        dbreference_assembly.ChildChanged += OnAssemblyChanged;
        dbreference_assembly.ChildRemoved += OnAssemblyChanged;

    }

    // Event handler for child changes    
    public void OnChildAdded(object sender, Firebase.Database.ChildChangedEventArgs args) 
    {
        if (args.DatabaseError != null)
        {
            UnityEngine.Debug.LogError(args.DatabaseError.Message);
            return;
        }

        var key = args.Snapshot.Key;
        var childSnapshot = args.Snapshot.GetValue(true);

        if (childSnapshot != null)
        {
            Step newValue = StepDesearilizer(key, childSnapshot);
           
            //make a new entry in the dictionary if it doesnt already exist
            if (IsValidStep(newValue))
            {
                if (DataItemDict.ContainsKey(key))
                {
                    UnityEngine.Debug.Log("The key already exists in the dictionary");
                }
                else
                {
                    UnityEngine.Debug.Log($"The key '{key}' does not exist in the dictionary");
                    BuildingPlanDataDict.Add(key, newValue);
                    BuildingPlanDataDict[key].data.element_ids[0] = key;

                    //Instantiate new object
                    OnDatabaseUpdate(newValue, key);
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Invalid Step structure for key '{key}'. Not added to the dictionary.");
            }
        }
    }
    public void OnChildChanged(object sender, Firebase.Database.ChildChangedEventArgs args) 
    {
        if (args.DatabaseError != null) {
        UnityEngine.Debug.LogError($"Database error: {args.DatabaseError}");
        return;
        }

        if (args.Snapshot == null) {
            UnityEngine.Debug.LogWarning("Snapshot is null. Ignoring the child change.");
            return;
        }

        string key = args.Snapshot.Key;
        var childSnapshot = args.Snapshot.GetValue(true);

        if (childSnapshot != null)
        {
            Step newValue = StepDesearilizer(key, childSnapshot);
            
            if(IsValidStep(newValue))
            {
                BuildingPlanDataDict[key] = newValue;
                BuildingPlanDataDict[key].data.element_ids[0] = key;
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Invalid Node structure for key '{key}'. Not added to the dictionary.");
            }

            UnityEngine.Debug.Log($"HandleChildChanged - The key of the changed Step is {key}");
            UnityEngine.Debug.Log("newData[key] = " + BuildingPlanDataDict[key]);
            
            //Instantiate new object
            OnDatabaseUpdate(newValue, key);
        }
    }
    public void OnChildRemoved(object sender, Firebase.Database.ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            UnityEngine.Debug.LogError(args.DatabaseError.Message);
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
                UnityEngine.Debug.Log("The key exists in the dictionary and is going to be removed");
                BuildingPlanDataDict.Remove(key);
                OnDatabaseUpdate(newValue, key);
            }
            else
            {
                UnityEngine.Debug.Log("The key does not exist in the dictionary");
            }
        }
    }  
    public void OnAssemblyChanged(object sender, Firebase.Database.ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null) {
        UnityEngine.Debug.LogError($"Database error: {args.DatabaseError}");
        return;
        }

        if (args.Snapshot == null) {
            UnityEngine.Debug.LogWarning("Snapshot is null. Ignoring the child change.");
            return;
        }
        
        UnityEngine.Debug.Log("Assembly Changed");
        FetchRTDData(dbreference_assembly, snapshot => DeserializeDataSnapshot(snapshot), false);
    }

    // Event handling for database initialization
    protected virtual void OnDatabaseInitializedDict(Dictionary<string, Step> BuildingPlanDataDict)
    {
        UnityEngine.Assertions.Assert.IsNotNull(DatabaseInitializedDict, "Database dict is null!");
        DatabaseInitializedDict(this, new DataItemDictEventArgs() { BuildingPlanDataDict = BuildingPlanDataDict });
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