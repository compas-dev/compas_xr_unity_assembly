using System;
using UnityEngine;
using Firebase.Database;
using CompasXR.Core;

public class Eventmanager : MonoBehaviour
{
    public GameObject Databasemanager;
    public GameObject Instantiateobjects;
    public GameObject Checkfirebase;
    public GameObject QRLocalization;
    public GameObject MqttTrajectoryReceiver;
    public GameObject TrajectoryVisulizer;
    public DatabaseReference settings_reference;
    DatabaseManager databaseManager;

    void Awake()
    {            
        //On Application Start clear data in Cache.
        Caching.ClearCache();

        //Set Persistence: Disables storing information on the device for when there is no internet connection.
        FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
        
        //Get Reference for the correct application settings. To dynamically connect to different RTDB and Storage.
        settings_reference =  FirebaseDatabase.DefaultInstance.GetReference("ApplicationSettings");
        
        //Add script components to objects in the scene
        databaseManager = Databasemanager.AddComponent<DatabaseManager>();  
        InstantiateObjects instantiateObjects = Instantiateobjects.AddComponent<InstantiateObjects>();
        CheckFirebase checkFirebase = Checkfirebase.AddComponent<CheckFirebase>();
        QRLocalization qrLocalization = QRLocalization.GetComponent<QRLocalization>();
        MqttTrajectoryManager mqttTrajectoryReceiver = MqttTrajectoryReceiver.GetComponent<MqttTrajectoryManager>();
        TrajectoryVisulizer trajectoryVisulizer = TrajectoryVisulizer.GetComponent<TrajectoryVisulizer>();
        
        //Initilize Connection to Firebase and Fetch Settings Data
        checkFirebase.FirebaseInitialized += DBInitializedFetchSettings;

        //Fetch data from realtime database
        databaseManager.ApplicationSettingUpdate += databaseManager.FetchData;

        //Set publisher and subscriber topic based on project name from application settings.
        databaseManager.ApplicationSettingUpdate += mqttTrajectoryReceiver.SetCompasXRTopics;

        //Initialize the database.. once the database is initialized the objects are instantiated
        databaseManager.DatabaseInitializedDict += instantiateObjects.OnDatabaseInitializedDict;

        //Start tracking Codes only once tracking information is received
        databaseManager.TrackingDictReceived += qrLocalization.OnTrackingInformationReceived;

        //Add listners after initial objects have been placed to avoid simultanous item placement
        instantiateObjects.PlacedInitialElements += databaseManager.AddListeners;

        //Trigger events for updates in the database.
        databaseManager.DatabaseUpdate += instantiateObjects.OnDatabaseUpdate;
        databaseManager.UserInfoUpdate += instantiateObjects.OnUserInfoUpdate;

    }

    public void DBInitializedFetchSettings(object sender, EventArgs e)
    {
        Debug.Log("Database Initilized: Safe to Fetch Settings Data.");
        databaseManager.FetchSettingsData(settings_reference);
    }  

}

