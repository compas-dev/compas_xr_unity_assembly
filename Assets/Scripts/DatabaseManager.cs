using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Storage;
using System.IO;
using UnityEngine.Networking;
using System.Linq;
using CompasXR.UI;
using CompasXR.Core.Data;
using CompasXR.AppSettings;
using CompasXR.Database.FirebaseManagment;
using Unity.IO.LowLevel.Unsafe;

namespace CompasXR.Core
{
    public class BuildingPlanDataDictEventArgs : EventArgs
    {
        public BuildingPlanData BuildingPlanDataItem { get; set; }
    }

    public class TrackingDataDictEventArgs : EventArgs
    {
        public Dictionary<string, Node> QRCodeDataDict { get; set; }
    }

    public class UpdateDataItemsDictEventArgs : EventArgs
    {
        public Step NewValue { get; set; }
        public string Key { get; set; }
    }
    public class UserInfoDataItemsDictEventArgs : EventArgs
    {
        public UserCurrentInfo UserInfo { get; set; }
        public string Key { get; set; }
    }

    public class ApplicationSettingsEventArgs : EventArgs
    {
        public ApplicationSettings Settings { get; set; }
    }

    public class DatabaseManager : MonoBehaviour
    {
        // Firebase database references
        public DatabaseReference dbReferenceAssembly;
        public DatabaseReference dbReferenceBuildingPlan;
        public DatabaseReference dbReferenceSteps;
        public DatabaseReference dbReferenceLastBuiltIndex;
        public DatabaseReference dbReferenceQRCodes;
        public DatabaseReference dbReferenceUsersCurrentSteps;
        public StorageReference dbRefrenceStorageDirectory;
        public DatabaseReference dbRefrenceProject;


        // Data structures to store nodes and steps
        public Dictionary<string, Node> AssemblyDataDict { get; private set; } = new Dictionary<string, Node>();
        public BuildingPlanData BuildingPlanDataItem { get; private set; } = new BuildingPlanData();
        public Dictionary<string, Node> QRCodeDataDict { get; private set; } = new Dictionary<string, Node>();
        public Dictionary<string, UserCurrentInfo> UserCurrentStepDict { get; private set; } = new Dictionary<string, UserCurrentInfo>();
        public Dictionary<string, List<string>> PriorityTreeDict { get; private set; } = new Dictionary<string, List<string>>();

        //Data Structure to Store Application Settings
        public ApplicationSettings applicationSettings;


        // Define event delegates and events
        public delegate void StoreDataDictEventHandler(object source, BuildingPlanDataDictEventArgs e); 
        public event StoreDataDictEventHandler DatabaseInitializedDict;

        public delegate void TrackingDataDictEventHandler(object source, TrackingDataDictEventArgs e); 
        public event TrackingDataDictEventHandler TrackingDictReceived;

        public delegate void UpdateDataDictEventHandler(object source, UpdateDataItemsDictEventArgs e); 
        public event UpdateDataDictEventHandler DatabaseUpdate;

        public delegate void StoreApplicationSettings(object source, ApplicationSettingsEventArgs e);
        public event StoreApplicationSettings ApplicationSettingUpdate;
        
        public delegate void UpdateUserInfoEventHandler(object source, UserInfoDataItemsDictEventArgs e);
        public event UpdateUserInfoEventHandler UserInfoUpdate;

        //Other Scripts
        public UIFunctionalities UIFunctionalities;

        //In script use objects.
        public bool z_remapped;
        public string TempDatabaseLastBuiltStep;

        public string CurrentPriority = null;

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
            await settings_reference.GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Error fetching data from Firebase");
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
            //Create DB References
            dbRefrenceProject = FirebaseDatabase.DefaultInstance.GetReference(e.Settings.project_name);
            dbReferenceAssembly = FirebaseDatabase.DefaultInstance.GetReference(e.Settings.project_name).Child("assembly").Child("graph").Child("node");
            dbReferenceBuildingPlan = FirebaseDatabase.DefaultInstance.GetReference(e.Settings.project_name).Child("building_plan").Child("data");
            dbReferenceSteps = FirebaseDatabase.DefaultInstance.GetReference(e.Settings.project_name).Child("building_plan").Child("data").Child("steps");
            dbReferenceLastBuiltIndex = FirebaseDatabase.DefaultInstance.GetReference(e.Settings.project_name).Child("building_plan").Child("data").Child("LastBuiltIndex");
            dbReferenceQRCodes = FirebaseDatabase.DefaultInstance.GetReference(e.Settings.project_name).Child("QRFrames").Child("graph").Child("node");
            dbReferenceUsersCurrentSteps = FirebaseDatabase.DefaultInstance.GetReference(e.Settings.project_name).Child("UsersCurrentStep");

            //If there is nothing to download Storage=="None" then trigger Objects Secured event
            if (e.Settings.storage_folder == "None")
            {
                //Fetch QR Data no event trigger
                FetchRTDDatawithEventHandler(dbReferenceQRCodes, snapshot => DeserializeAssemblyDataSnapshot(snapshot, QRCodeDataDict), "TrackingDict");
                
                //Fetch Assembly Data no event trigger
                FetchRTDDatawithEventHandler(dbReferenceAssembly, snapshot => DeserializeAssemblyDataSnapshot(snapshot, AssemblyDataDict));

                //Fetch Building plan data with event trigger
                FetchRTDDatawithEventHandler(dbReferenceBuildingPlan, snapshot => DesearializeBuildingPlanDataSnapshot(snapshot), "BuildingPlanDataDict");

            }
            
            //Else trigger download.
            else
            {
                //Set Obj Orientation bool
                z_remapped = e.Settings.z_to_y_remap;
                
                //Storage Reference from data fetched
                dbRefrenceStorageDirectory = FirebaseStorage.DefaultInstance.GetReference("obj_storage").Child(e.Settings.storage_folder);
                string basepath = dbRefrenceStorageDirectory.Path;
                string folderPath = basepath.Substring(1);
                string storageBucket = FirebaseManager.Instance.storageBucket;
                string firebaseUrl = $"https://firebasestorage.googleapis.com/v0/b/{storageBucket}/o?prefix={folderPath}/&delimiter=/";
                List<DataHandlers.FileMetadata> files = await DataHandlers.GetFilesInWebFolder(firebaseUrl);
                List<DataHandlers.FileMetadata> filesWithUri = await GetDownloadUriFromFilesMedata(files);

                //Fetch Data from both storage and Realtime Database.
                FetchAllData(filesWithUri);
            }
        }
        private async void FetchAllData(List<DataHandlers.FileMetadata> files)
        {
            //Fetch Storage Data
            string directoryPath = System.IO.Path.Combine(Application.persistentDataPath, "Object_Storage");
            await FetchAndDownloadFilesFromStorage(files, directoryPath);

            //Fetch QR Data with "TrackingDict" event trigger
            FetchRTDDatawithEventHandler(dbReferenceQRCodes, snapshot => DeserializeAssemblyDataSnapshot(snapshot, QRCodeDataDict), "TrackingDict");
            
            //Fetch Assembly Data no event trigger
            FetchRTDDatawithEventHandler(dbReferenceAssembly, snapshot => DeserializeAssemblyDataSnapshot(snapshot, AssemblyDataDict));
            
            //Fetch Building plan data with "BuildingPlandataDict" event trigger
            FetchRTDDatawithEventHandler(dbReferenceBuildingPlan, snapshot => DesearializeBuildingPlanDataSnapshot(snapshot), "BuildingPlanDataDict");
        }
        private async Task<List<DataHandlers.FileMetadata>> GetDownloadUriFromFilesMedata(List<DataHandlers.FileMetadata> filesMetadata) //TODO: Dificulty Moving with OnScreenMessage
        {
            List<Task> fetchUriTasks = new List<Task>();
            
            foreach (var fileMetadata in filesMetadata)
            {
                // Set the reference to the file in Firebase Storage
                var fileRef = FirebaseStorage.DefaultInstance.GetReference(fileMetadata.name);

                //Add Fetch Uri Task to the list
                fetchUriTasks.Add(fileRef.GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted)
                    {
                        //If there is an error when fetching an object signal and on screen message.
                        string message = $"ERROR: Application unable to fetch URL for {fileMetadata.name}. Please review the associated file and try again.";
                        UserInterface.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref UIFunctionalities.ErrorFetchingDownloadUriMessageObject, "ErrorFetchingDownloadUriMessage", UIFunctionalities.MessagesParent, message, "GetDownloadUriFromFilesMedata: Error Fetching Download URL");
                        
                        Debug.LogError("Error fetching download URL from Firebase Storage");
                        return;
                    }
                    if (task.IsCompleted)
                    {
                        Uri downloadUrlUri = task.Result;

                        //Convert URI to string and Add the download URL to the file metadata
                        string downloadUrl = downloadUrlUri.ToString();
                        fileMetadata.uri = downloadUrl;
                    }
                }));
            }
            //Await all download tasks are done before refreshing.
            await Task.WhenAll(fetchUriTasks);

            return filesMetadata;
        }
        public async Task FetchAndDownloadFilesFromStorage(List<DataHandlers.FileMetadata> filesMetadata, string localDirectoryPath) //TODO: Can't Move Because DownloadFile cant move
        {
            List<Task> downloadTasks = new List<Task>();
            
            foreach (var fileMetadata in filesMetadata)
            {
                // Retrieve the download URL from the files metadata information
                string downloadUrl = fileMetadata.uri;

                // Construct the local file path
                string localFilePath = System.IO.Path.Combine(localDirectoryPath, System.IO.Path.GetFileName(fileMetadata.name));
                
                // Ensure the directory exists
                DataHandlers.CreateDirectory(localDirectoryPath);

                //Add the Download task to the tasklist
                downloadTasks.Add(DownloadFile(downloadUrl, localFilePath));
            }
            //Await all download tasks are done before refreshing.
            await Task.WhenAll(downloadTasks);
        }
        private async Task DownloadFile(string downloadUrl, string saveFilePath) //TODO: Dificulty Moving with OnScreenMessage
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(downloadUrl))
            {
                webRequest.downloadHandler = new DownloadHandlerFile(saveFilePath);
                await webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    //If there is an error when downloading an object signal and on screen message.
                    string message = $"DownloadFile: ERROR: Application failed download file for object {Path.GetFileName(saveFilePath)}. Please review the associated file and try again.";
                    UserInterface.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref UIFunctionalities.ErrorDownloadingObjectMessageObject, "ErrorDownloadingObjectMessage", UIFunctionalities.MessagesParent, message, "DownloadFile: Error Downloading Object");

                    Debug.LogError("DownloadFile: File download error: " + webRequest.error);
                }
                else
                {
                    Debug.Log("DownloadFile: File successfully downloaded and saved to " + saveFilePath);
                }
            }
        }
        public async Task FetchRTDDatawithEventHandler(DatabaseReference dbreference, Action<DataSnapshot> deserilizationMethod, string eventname = null)
        {
            await DataHandlers.FetchDataFromDatabaseReference(dbreference, deserilizationMethod);

            if (eventname != null && eventname == "BuildingPlanDataDict")
            {
                OnDatabaseInitializedDict(BuildingPlanDataItem); 
            }

            if (eventname != null && eventname == "TrackingDict")
            {
                OnTrackingDataReceived(QRCodeDataDict);
            }
        }      
        public void PushAllDataBuildingPlan(string key)
        {        
            //Find step that I changed in the building plan and add my custom device id.
            Step specificstep = BuildingPlanDataItem.steps[key];
            specificstep.data.device_id = SystemInfo.deviceUniqueIdentifier;

            //Searilize the data for push to firebase
            string data = JsonConvert.SerializeObject(BuildingPlanDataItem);
            
            //Push the data to firebase
            dbReferenceBuildingPlan.SetRawJsonValueAsync(data);
        }

    /////////////////////////// DATA DESERIALIZATION ///////////////////////////////////////
        private void DeserializeSettingsData(DataSnapshot snapshot)
        {
            //Delete Objects from the storage directory path
            string path = Application.persistentDataPath;
            string storageFolderPath = Path.Combine(path, "Object_Storage");
            DataHandlers.DeleteFilesFromDirectory(storageFolderPath);
            DataHandlers.CreateDirectory(storageFolderPath);

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
        private void DeserializeAssemblyDataSnapshot(DataSnapshot snapshot, Dictionary<string, Node> dataDict)
        {
            //Clear current Dictionary if it contains information
            dataDict.Clear();

            //Desearialize individual data items from the snapshots
            foreach (DataSnapshot childSnapshot in snapshot.Children)
            {
                string key = childSnapshot.Key;
                var json_data = childSnapshot.GetValue(true);
                Node node_data = Node.Parse(key, json_data); //TODO: NODE DESERIALIZER
                
                if (node_data.IsValidNode())
                {
                    dataDict[key] = node_data;
                    dataDict[key].type_id = key;
                }
                else
                {
                    Debug.LogWarning($"Invalid Node structure for key '{key}'. Not added to the dictionary.");
                }
            }
            
            Debug.Log("Number of nodes stored as a dictionary = " + dataDict.Count);

        }
        private void DesearializeStringItem(DataSnapshot snapshot, ref string tempStringStorage)
        {  
            string jsondatastring = snapshot.GetRawJsonValue();
            Debug.Log("DesearializeStringItem: String Item Data:" + jsondatastring);
            
            if (!string.IsNullOrEmpty(jsondatastring))
            {
                tempStringStorage = JsonConvert.DeserializeObject<string>(jsondatastring);
            }
            else
            {
                Debug.LogWarning("DesearializeStringItem: String Item Did not produce a value");
                tempStringStorage = null;
            }
        }
        private void DesearializeBuildingPlanDataSnapshot(DataSnapshot snapshot)
        {
            //Set Buliding plan to a null value
            if (BuildingPlanDataItem.steps != null && BuildingPlanDataItem.LastBuiltIndex != null)
            {
                BuildingPlanDataItem.LastBuiltIndex = null;
                BuildingPlanDataItem.steps.Clear();
            }
            if (PriorityTreeDict != null)
            {
                PriorityTreeDict.Clear();
            }

            var jsondata = snapshot.GetValue(true);
            Dictionary<string, List<string>> priorityTreeDict = new Dictionary<string, List<string>>();
            
            (BuildingPlanData buildingPlanData, 
            Dictionary<string, List<string>> priorityTreeDictionary) = BuildingPlanData.Parse(jsondata, priorityTreeDict);

            //Assign the data to the PriorityTreeDict Class Variable
            PriorityTreeDict = priorityTreeDictionary;

            if (buildingPlanData != null && buildingPlanData.steps != null)
            {
                BuildingPlanDataItem = buildingPlanData;
            }
            else
            {
                Debug.LogWarning("You did not set your building plan data properly");
            }
            
        }

    /////////////////////////// INTERNAL DATA MANAGERS //////////////////////////////////////
        public void FindInitialElement()
        {
            //ITERATE THROUGH THE BUILDING PLAN DATA DICT IN ORDER.
            for (int i =0 ; i < BuildingPlanDataItem.steps.Count; i++)
            {
                //Set data items
                Step step = BuildingPlanDataItem.steps[i.ToString()];

                //Find the first unbuilt element
                if(step.data.is_built == false)
                {
                    //Set Current Priority as the priority of the first this current step. This needs to be done before setting the current step.
                    UIFunctionalities.SetCurrentPriority(step.data.priority.ToString());

                    //Set Current Element
                    UIFunctionalities.SetCurrentStep(i.ToString());

                    break;
                }
            }
        }
        public int OtherUserPriorityChecker(Step step, string stepKey)
        {        
            //If the priority of the current priority then check if it is complete.
            if (CurrentPriority == step.data.priority.ToString())
            {
                //New empty list to store unbuilt elements
                List<string> UnbuiltElements = new List<string>();
                
                //Find the current priority in the dictionary for iteration
                List<string> PriorityDataItem = PriorityTreeDict[CurrentPriority];

                //Iterate through the Priority tree dictionary to check the elements and if the priority is complete
                foreach(string element in PriorityDataItem)
                {
                    //Find the step in the dictoinary
                    Step stepToCheck = BuildingPlanDataItem.steps[element];

                    //Check if the element is built
                    if(!stepToCheck.data.is_built)
                    {
                        UnbuiltElements.Add(element);
                    }
                }

                //If the list is empty return 2 because all elements of that priority are built, and we want to move on to the next priority.
                if(UnbuiltElements.Count == 0)
                {
                    Debug.Log($"OtherUser Priority Check: Current Priority is complete. Unlocking Next Priority.");
                    
                    //Return 2, to move on to the next priority.
                    return 2;
                }
                
                //If the list is not empty return 0 so do nothing.
                else
                {
                    Debug.Log($"OtherUser Priority Check: Current Priority is not complete. Incomplete Priority");
                    
                    //Return 0 to not push data.
                    return 0;
                }
            }
            else
            {
                //If the priority is not the same as the current priority (meaning it is a lower priority) then we should set it.
                Debug.Log($"OtherUser Priority Check: Current Priority is not the same as the priority of the step. Set this priority.");
                
                //Return set this item to the current priority.
                return 1;
            }

        }

    /////////////////////////////// EVENT HANDLING ////////////////////////////////////////

        // Add listeners and remove them for firebase child events
        public void AddListeners(object source, EventArgs args)
        {        
            Debug.Log("Adding Listners");
            
            //Add listners for building plan steps
            dbReferenceSteps.ChildAdded += OnStepsChildAdded;
            dbReferenceSteps.ChildChanged += OnStepsChildChanged;
            dbReferenceSteps.ChildRemoved += OnStepsChildRemoved;
            
            //Add Listners for Users Current Step
            dbReferenceUsersCurrentSteps.ChildAdded += OnUserChildAdded; 
            dbReferenceUsersCurrentSteps.ChildChanged += OnUserChildChanged;
            dbReferenceUsersCurrentSteps.ChildRemoved += OnUserChildRemoved;

            //Add Listner for building plan last built index
            dbReferenceLastBuiltIndex.ValueChanged += OnLastBuiltIndexChanged;

            //Add Listners to the Overall project to list for data changes in assembly, qrcodes, and additional Info.
            dbRefrenceProject.ChildAdded += OnProjectInfoChangedUpdate;
            dbRefrenceProject.ChildChanged += OnProjectInfoChangedUpdate;
            dbRefrenceProject.ChildRemoved += OnProjectInfoChangedUpdate;
        }
        public void RemoveListners()
        {        
            Debug.Log("Removing the listeners");

            //Remove listners for building plan steps
            dbReferenceSteps.ChildAdded += OnStepsChildAdded;
            dbReferenceSteps.ChildChanged += OnStepsChildChanged;
            dbReferenceSteps.ChildRemoved += OnStepsChildRemoved;
            
            //Remove Listners for Users Current Step
            dbReferenceUsersCurrentSteps.ChildAdded += OnUserChildAdded; 
            dbReferenceUsersCurrentSteps.ChildChanged += OnUserChildChanged;
            dbReferenceUsersCurrentSteps.ChildRemoved += OnUserChildRemoved;

            //Remove Listner for building plan last built index
            dbReferenceLastBuiltIndex.ValueChanged += OnLastBuiltIndexChanged;

            //Remove Listners to the Overall project to list for data changes in assembly, qrcodes, and additional Info.
            dbRefrenceProject.ChildAdded += OnProjectInfoChangedUpdate;
            dbRefrenceProject.ChildChanged += OnProjectInfoChangedUpdate;
            dbRefrenceProject.ChildRemoved += OnProjectInfoChangedUpdate;
        }

        // Event handler for BuildingPlan child changes
        public void OnStepsChildAdded(object sender, Firebase.Database.ChildChangedEventArgs args) 
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
                Step newValue = Step.Parse(childSnapshot); //TODO: STEP DESERIALIZER
            
                //make a new entry in the dictionary if it doesnt already exist
                if (newValue.IsValidStep())
                {
                    if (BuildingPlanDataItem.steps.ContainsKey(key))
                    {
                        Debug.Log("The key already exists in the dictionary");
                    }
                    else
                    {
                        Debug.Log($"The key '{key}' does not exist in the dictionary");
                        BuildingPlanDataItem.steps.Add(key, newValue);

                        //Check if the steps priority is one that I already have in the priority tree dictionary
                        if (PriorityTreeDict.ContainsKey(newValue.data.priority.ToString()))
                        {
                            //If the priority already exists add the key to the list
                            PriorityTreeDict[newValue.data.priority.ToString()].Add(key);
                            Debug.Log($"Step {key} successfully added to the priority tree dictionary item {newValue.data.priority.ToString()}");
                        }
                        else
                        {
                            //If not create a new list and add the key to the list
                            PriorityTreeDict[newValue.data.priority.ToString()] = new List<string>();
                            PriorityTreeDict[newValue.data.priority.ToString()].Add(key);
                            Debug.Log($"Step {key} added a new priority {newValue.data.priority.ToString()} to the priority tree dictionary");
                        }

                        //Instantiate new object
                        OnDatabaseUpdate(newValue, key);
                        
                    }
                }
                else
                {
                    Debug.LogWarning($"Invalid Step structure for key '{key}'. Not added to the dictionary.");
                }

                //Print out the priority tree as a check
                Debug.Log("THIS IS THE PRIORITY TREE DICTIONARY: " + JsonConvert.SerializeObject(PriorityTreeDict));
            }
        } 
        public void OnStepsChildChanged(object sender, Firebase.Database.ChildChangedEventArgs args) 
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
            if (key == null) {
                Debug.LogWarning("Snapshot key is null. Ignoring the child change.");
            }

            var childSnapshot = args.Snapshot.GetValue(true);

            if (childSnapshot != null)
            {
                Step newValue = Step.Parse(childSnapshot); //TODO: STEP DESERILIZER
                
                //Check: if the step is equal to the one that I have in the dictionary
                if (!Step.AreEqualSteps(newValue, BuildingPlanDataItem.steps[key]))
                {    
                    Debug.Log($"On Child Changed: This key actually changed {key}");

                    if (newValue.data.device_id != null)
                    {
                        //Check: if the change is from me or from someone else could possibly get rid of this because we check every step, but still safety check for now.
                        if (newValue.data.device_id == SystemInfo.deviceUniqueIdentifier && Step.AreEqualSteps(newValue, BuildingPlanDataItem.steps[key]))
                        {
                            Debug.Log($"I changed element {key}");
                            return;
                        }
                        //This means that the change was specifically from another device.
                        else
                        {    
                            if(newValue.IsValidStep())
                            {
                                //First check if the new steps priorty is different from the old one then remove the old one and add a new one.
                                if (newValue.data.priority != BuildingPlanDataItem.steps[key].data.priority)
                                {
                                    //Remove the old key from the priority tree dictionary
                                    PriorityTreeDict[BuildingPlanDataItem.steps[key].data.priority.ToString()].Remove(key);

                                    //Check if the priority tree value item is null and if so remove it from the dictionary
                                    if (PriorityTreeDict[BuildingPlanDataItem.steps[key].data.priority.ToString()].Count == 0)
                                    {
                                        PriorityTreeDict.Remove(BuildingPlanDataItem.steps[key].data.priority.ToString());
                                    }

                                    //Check if the steps priority is one that I already have in the priority tree dictionary
                                    if (PriorityTreeDict.ContainsKey(newValue.data.priority.ToString()))
                                    {
                                        //If the priority already exists add the key to the list
                                        PriorityTreeDict[newValue.data.priority.ToString()].Add(key);
                                        Debug.Log($"Step {key} successfully added to the priority tree dictionary item {newValue.data.priority.ToString()}");
                                    }
                                    else
                                    {
                                        //If not create a new list and add the key to the list
                                        PriorityTreeDict[newValue.data.priority.ToString()] = new List<string>();
                                        PriorityTreeDict[newValue.data.priority.ToString()].Add(key);
                                        Debug.Log($"Step {key} added a new priority {newValue.data.priority.ToString()} to the priority tree dictionary");
                                    }
                                }
                                else
                                {
                                    Debug.Log($"The priority of the step {key} did not change");
                                }

                                //Add the new value to the Building plan dictionary
                                BuildingPlanDataItem.steps[key] = newValue;
                            }
                            else
                            {
                                Debug.LogWarning($"Invalid Node structure for key '{key}'. Not added to the dictionary.");
                            }

                            Debug.Log($"HandleChildChanged - The key of the changed Step is {key}");
                            Debug.Log("newData[key] = " + BuildingPlanDataItem.steps[key]);
                            
                            //Instantiate new object
                            OnDatabaseUpdate(newValue, key);
                        }
                    }
                    //Check: This change happened either manually or from grasshopper. To an object that doesn't have a device id.
                    else
                    {
                        Debug.LogWarning($"Device ID is null: the change for key {key} happened from gh or manually.");

                        if(newValue.IsValidStep())
                        {

                            //First check if the new steps priorty is different from the old one then remove the old one and add a new one.
                            if (newValue.data.priority != BuildingPlanDataItem.steps[key].data.priority)
                            {                            
                                //Remove the old key from the priority tree dictionary
                                PriorityTreeDict[BuildingPlanDataItem.steps[key].data.priority.ToString()].Remove(key);

                                //Check if the priority tree value item is null and if so remove it from the dictionary
                                if (PriorityTreeDict[BuildingPlanDataItem.steps[key].data.priority.ToString()].Count == 0)
                                {
                                    PriorityTreeDict.Remove(BuildingPlanDataItem.steps[key].data.priority.ToString());
                                }

                                //Check if the steps priority is one that I already have in the priority tree dictionary
                                if (PriorityTreeDict.ContainsKey(newValue.data.priority.ToString()))
                                {
                                    //If the priority already exists add the key to the list
                                    PriorityTreeDict[newValue.data.priority.ToString()].Add(key);
                                    Debug.Log($"Step {key} successfully added to the priority tree dictionary item {newValue.data.priority.ToString()}");
                                }
                                else
                                {
                                    //If not create a new list and add the key to the list
                                    PriorityTreeDict[newValue.data.priority.ToString()] = new List<string>();
                                    PriorityTreeDict[newValue.data.priority.ToString()].Add(key);
                                    Debug.Log($"Step {key} added a new priority {newValue.data.priority.ToString()} to the priority tree dictionary");
                                }
                                
                                BuildingPlanDataItem.steps[key] = newValue;
                            }
                            else
                            {
                                Debug.Log($"The priority of the step {key} did not change");
                            }

                        }
                        else
                        {
                            Debug.LogWarning($"Invalid Node structure for key '{key}'. Not added to the dictionary.");
                        }

                        Debug.Log($"HandleChildChanged - The key of the changed Step is {key}");
                        Debug.Log("newData[key] = " + BuildingPlanDataItem.steps[key]);
                        
                        //Instantiate new object
                        OnDatabaseUpdate(newValue, key);

                    }

                    //Print out the priority tree as a check
                    Debug.Log("THIS IS THE PRIORITY TREE DICTIONARY: " + JsonConvert.SerializeObject(PriorityTreeDict));
                }

            }
        }
        public void OnStepsChildRemoved(object sender, Firebase.Database.ChildChangedEventArgs args)
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
                if (BuildingPlanDataItem.steps.ContainsKey(key))
                {
                    Step newValue = null;
                    Debug.Log("The key exists in the dictionary and is going to be removed");
                    
                    //First check thee steps priorty Remove the steps key from the priority tree dictionary
                    PriorityTreeDict[BuildingPlanDataItem.steps[key].data.priority.ToString()].Remove(key);

                    //Check if the priority tree value item is null and if so remove it from the dictionary
                    if (PriorityTreeDict[BuildingPlanDataItem.steps[key].data.priority.ToString()].Count == 0)
                    {
                        PriorityTreeDict.Remove(BuildingPlanDataItem.steps[key].data.priority.ToString());
                    }
                    
                    //Remove the step from the building plan dictionary
                    BuildingPlanDataItem.steps.Remove(key);

                    //Trigger database event
                    OnDatabaseUpdate(newValue, key);
                }
                else
                {
                    Debug.Log("The key does not exist in the dictionary");
                }
            }
        }  
        public async void OnLastBuiltIndexChanged(object sender, Firebase.Database.ValueChangedEventArgs args)
        {

            if (args.DatabaseError != null) {
            Debug.LogError($"Database error: {args.DatabaseError}");
            return;
            }

            if (args.Snapshot == null) {
                Debug.LogWarning("Snapshot is null. Ignoring the child change.");
                return;
            }
            
            Debug.Log("Last Built Index Changed");
            
            //Set Temp Current Element to null so that everytime an event is triggered it becomes null again and doesnt keep old data.
            TempDatabaseLastBuiltStep = null;
            
            await FetchRTDDatawithEventHandler(dbReferenceLastBuiltIndex, snapshot => DesearializeStringItem(snapshot, ref TempDatabaseLastBuiltStep));
        
            if (TempDatabaseLastBuiltStep != null)
            {
                if(TempDatabaseLastBuiltStep != BuildingPlanDataItem.LastBuiltIndex)
                {
                    // Update Last Built Index
                    BuildingPlanDataItem.LastBuiltIndex = TempDatabaseLastBuiltStep;
                    Debug.Log($"Last Built Index is now {BuildingPlanDataItem.LastBuiltIndex}");

                    // Update On Screen Text
                    UIFunctionalities.SetLastBuiltText(BuildingPlanDataItem.LastBuiltIndex);
                
                    // Check Priority to see if its complete.
                    Step step = BuildingPlanDataItem.steps[BuildingPlanDataItem.LastBuiltIndex];
                    int priorityCheck = OtherUserPriorityChecker(step, BuildingPlanDataItem.LastBuiltIndex);

                    // If the priority checker == 1 then set priority from this step. If it == 2 then set priority from the next step. (THIS COULD BE PRIORITY + 1, but safest to use the next step because it avoids errors from if a priority is missing)
                    if (priorityCheck == 1)
                    {
                        UIFunctionalities.SetCurrentPriority(step.data.priority.ToString());
                    }
                    else if (priorityCheck == 2 && Convert.ToInt32(BuildingPlanDataItem.LastBuiltIndex) < BuildingPlanDataItem.steps.Count - 1)
                    {
                        //Get my current priority
                        string localCurrentPriority = CurrentPriority;

                        //convert to int and add 1
                        int nextPriority = Convert.ToInt32(localCurrentPriority) + 1;

                        //Set this as my new current priority
                        UIFunctionalities.SetCurrentPriority(nextPriority.ToString());

                        //If my CurrentStep Priority is the same as New Current Priority then update UI graphics
                        Step localCurrentStep = BuildingPlanDataItem.steps[UIFunctionalities.CurrentStep];

                        //If my CurrentStep Priority is the same as New Current Priority then update UI graphics
                        if(localCurrentStep.data.priority.ToString() == CurrentPriority)
                        {    
                            UIFunctionalities.IsBuiltButtonGraphicsControler(localCurrentStep.data.is_built, localCurrentStep.data.priority);
                        }

                    }
                    else
                    {
                        Debug.Log($"OtherUser Priority Check: returned {priorityCheck} and is not 1 or 2 and shouldn't do anything");
                    }
                }
                else
                {
                    Debug.Log("Last Built Index is the same your current Last Built Index");
                }

            }
        }    
        
        // Event handlers for User Current Step
        public void OnUserChildAdded(object sender, Firebase.Database.ChildChangedEventArgs args)
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

            if (childSnapshot != null)
            {
                UserCurrentInfo newValue = UserCurrentInfo.Parse(childSnapshot);
                
                //make a new entry in the dictionary if it doesnt already exist
                if (newValue != null)
                {
                    if (UserCurrentStepDict.ContainsKey(key))
                    {
                        Debug.Log($"This User {key} already exists in the dictionary");
                    }
                    else
                    {
                        Debug.Log($"The key '{key}' does not exist in the dictionary");
                        UserCurrentStepDict.Add(key, newValue);

                        //Instantiate new object
                        OnUserInfoUpdated(newValue, key);
                    }
                }
                else
                {
                    Debug.LogWarning($"Invalid Step structure for key '{key}'. Not added to the dictionary.");
                }
            }
        }
        public void OnUserChildChanged(object sender, Firebase.Database.ChildChangedEventArgs args)
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

            if (childSnapshot != null)
            {
                UserCurrentInfo newValue = UserCurrentInfo.Parse(childSnapshot);
                
                //Check: if the current step update was from me or not.
                if (key != SystemInfo.deviceUniqueIdentifier)
                {    
                    Debug.Log($"User {key} updated their current step");


                    Debug.Log($"I entered here for key {key}");
                    if(newValue != null)
                    {
                        UserCurrentStepDict[key] = newValue;
                    }
                    else
                    {
                        Debug.LogWarning($"Invalid Node structure for key '{key}'. Not added to the dictionary.");
                    }

                    Debug.Log($"Handle Changed User INfo - User {key} updated their current step to {newValue.currentStep}");
                    
                    //Instantiate new object
                    OnUserInfoUpdated(newValue, key);
                }
                else
                {
                    Debug.Log("I updated my current key");
                }

            }
        }
        public void OnUserChildRemoved(object sender, Firebase.Database.ChildChangedEventArgs args)
        {
            if (args.DatabaseError != null) {
            Debug.LogError($"Database error: {args.DatabaseError}");
            return;
            }
            
            string key = args.Snapshot.Key;
            string childSnapshot = args.Snapshot.GetRawJsonValue();
            
            if (!string.IsNullOrEmpty(childSnapshot))
            {
                //remove an entry in the dictionary
                if (UserCurrentStepDict.ContainsKey(key))
                {
                    UserCurrentInfo newValue = null;
                    Debug.Log("The key exists in the dictionary and is going to be removed");
                    UserCurrentStepDict.Remove(key);
                    OnUserInfoUpdated(newValue, key);
                }
                else
                {
                    Debug.Log("The key does not exist in the dictionary");
                }
            }

        }

        // Event Handlers for Additional Project Information, Assembly, Parts, QRFrames, & Joints
        public async void OnProjectInfoChangedUpdate(object sender, Firebase.Database.ChildChangedEventArgs args)
        {
            if (args.DatabaseError != null) {
            Debug.LogError($"Database error: {args.DatabaseError}");
            return;
            }

            if (args.Snapshot == null) {
                Debug.LogWarning("Snapshot is null. Ignoring the child change.");
                return;
            }

            //Get the child changed key and value
            string key = args.Snapshot.Key;
            var childSnapshot = args.Snapshot.GetValue(true);

            //If Snapshot and Key are not null check for where the change needs to happen.
            if (childSnapshot != null && key != null)
            {
                if(key == "assembly")
                {
                    Debug.Log("Project Changed: Assembly Changed");
                    
                    //If the assembly changed then fetch new assembly data
                    await FetchRTDDatawithEventHandler(dbReferenceAssembly, snapshot => DeserializeAssemblyDataSnapshot(snapshot, AssemblyDataDict));
                }
                else if(key == "QRFrames")
                {
                    Debug.Log("Project Changed: QRFrames Changed");

                    //If the qrcodes changed then fetch new qrcode data
                    await FetchRTDDatawithEventHandler(dbReferenceQRCodes, snapshot => DeserializeAssemblyDataSnapshot(snapshot, QRCodeDataDict), "TrackingDict");
                }
                else if(key == "beams")
                {
                    Debug.Log("Project Changed: Beams");
                }
                else if(key == "joints")
                {
                    Debug.Log("Project Changed: Joints");
                }
                else if(key == "building_plan")
                {
                    Debug.Log("Project Changed: BuildingPlan and should be handled by other listners");
                }
                else if(key == "UsersCurrentStep")
                {
                    Debug.Log("Project Changed: User Current Step Changed this should be handled by other listners");
                }
                else
                {
                    Debug.LogWarning($"Project Changed: The key: {key} did not match the expected project keys");
                }
            }
        }
        
        // Event handling for database initialization
        protected virtual void OnDatabaseInitializedDict(BuildingPlanData BuildingPlanDataItem)
        {
            UnityEngine.Assertions.Assert.IsNotNull(DatabaseInitializedDict, "Database dict is null!");
            Debug.Log("Building Plan Data Received");
            DatabaseInitializedDict(this, new BuildingPlanDataDictEventArgs() {BuildingPlanDataItem = BuildingPlanDataItem});
        }
        protected virtual void OnTrackingDataReceived(Dictionary<string, Node> QRCodeDataDict)
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
        protected virtual void OnUserInfoUpdated(UserCurrentInfo newValue, string key)
        {
            UnityEngine.Assertions.Assert.IsNotNull(UserInfoUpdate, "new dict is null!");
            UserInfoUpdate(this, new UserInfoDataItemsDictEventArgs() {UserInfo = newValue, Key = key });
        }
        protected virtual void OnSettingsUpdate(ApplicationSettings settings)
        {
            ApplicationSettingUpdate(this, new ApplicationSettingsEventArgs(){Settings = settings});
        }

        // Event Handeling to take care of App Clean up. When the GameObject is destroyed it cleans up everything.     
        protected virtual void OnDestroy()
        {
            //Remove my name from the UserCurrentStep list
            dbReferenceUsersCurrentSteps.Child(SystemInfo.deviceUniqueIdentifier).RemoveValueAsync();
            
            //Remove Listners
            RemoveListners();
        }

    }

    public static class DataHandlers
    {
        //Define HTTP request response classes
        class ListFilesResponse
        {
            public List<object> prefix { get; set; }
            public List<FileMetadata> items { get; set; }
        }

        public class FileMetadata
        {
            public string name { get; set; }
            public string bucket { get; set; }
            public string uri { get; set; }
        }
        public static void DeleteFilesFromDirectory(string directoryPath)
        {
            string folderpath = directoryPath.Replace('\\', '/');

            //If the folder exists delete all files in the folder. If not create the folder.
            if (Directory.Exists(folderpath))
            {
                System.IO.DirectoryInfo di = new DirectoryInfo(folderpath);
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }

                Debug.Log($"DeleteObjectsFromDirectory: Deleted all files in the directory @ {folderpath}");
            }
        }
        public static void CreateDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Debug.Log($"CreateDirectory: Created Directory for Object Storage @ {directoryPath}");
            }
            else
            {
                Debug.Log($"CreateDirectory: Directory @ path {directoryPath} already exists.");
            }
        }
        public static void PushStringDataToDatabaseReference(DatabaseReference databaseReference, string data)
        {
            databaseReference.SetRawJsonValueAsync(data);
        }
        public static string PrintDataFromDatabaseRefrerence(DatabaseReference databaseReference)
        {
            string jsondata = "";
            databaseReference.GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.Log("error");
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot dataSnapshot = task.Result;
                    jsondata = dataSnapshot.GetRawJsonValue(); 
                    Debug.Log("all nodes" + jsondata);
                }
            });
            return jsondata;
        }
        public static void CheckLocalPathExistance(string path)
        {       
            // Replace backslashes with forward slashes
            path = path.Replace('\\', '/');

            if (File.Exists(path))
            {
                Debug.Log($"CheckLocalPathExistance: File Exists @ {path}");
            }
            else
            {
                Debug.Log($"CheckLocalPathExistance: File does not exist @ {path}");
            }

        }
        public static async Task<List<FileMetadata>> GetFilesInWebFolder(string webFolderUrl)
        {
            //Building the storage url dynamically
            Debug.Log($"GetFilesInFolder: BaseUrl: {webFolderUrl}");

            // Send a GET request to the URL
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(webFolderUrl))
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
        public static async Task FetchDataFromDatabaseReference(DatabaseReference dbreference, Action<DataSnapshot> deserilizationMethod)
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
                    if (deserilizationMethod != null)
                    {
                        deserilizationMethod(snapshot);
                    }
                }
            });

        }
    }

}