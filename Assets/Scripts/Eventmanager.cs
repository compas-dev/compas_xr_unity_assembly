using System;
using UnityEngine;
using Firebase.Database;
using CompasXR.Database.FirebaseManagment;
using CompasXR.Robots;

namespace CompasXR.Core
{
    public class EventManager : MonoBehaviour
    {
        //GameObjects for Script Storage
        public GameObject databaseManagerObject;
        public GameObject instantiateObjectsObject;
        public GameObject checkFirebaseObject;
        public GameObject qrLocalizationObject;
        public GameObject mqttTrajectoryReceiverObject;
        public GameObject trajectoryVisualizerObject;

        //Database Reference
        public DatabaseReference dbReferenceSettings;

        //Other Script Components
        public DatabaseManager databaseManager;

        void Awake()
        {            
            //On Application Start clear data in Cache.
            Caching.ClearCache();

            //Set Persistence: Disables storing information on the device for when there is no internet connection.
            FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
            
            //Get Reference for the correct application settings. To dynamically connect to different RTDB and Storage.
            dbReferenceSettings =  FirebaseDatabase.DefaultInstance.GetReference("ApplicationSettings");
            
            //Add script components to objects in the scene
            databaseManager = databaseManagerObject.AddComponent<DatabaseManager>();  
            InstantiateObjects instantiateObjects = instantiateObjectsObject.AddComponent<InstantiateObjects>();
            CheckFirebase checkFirebase = checkFirebaseObject.AddComponent<CheckFirebase>();
            QRLocalization qrLocalization = qrLocalizationObject.GetComponent<QRLocalization>();
            MqttTrajectoryManager mqttTrajectoryReceiver = mqttTrajectoryReceiverObject.GetComponent<MqttTrajectoryManager>();
            TrajectoryVisualizer trajectoryVisualizer = trajectoryVisualizerObject.GetComponent<TrajectoryVisualizer>();
            
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
            databaseManager.FetchSettingsData(dbReferenceSettings);
        }  

    }
}

