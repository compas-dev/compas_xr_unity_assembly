using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Firebase.Database;
using JSON;
using Firebase.Auth;
using TMPro;

public class Eventmanager : MonoBehaviour
{
    public GameObject Databasemanager;
    public GameObject Instantiateobjects;
    public GameObject Checkfirebase;
    public DatabaseReference dbreference_design;

    public DatabaseReference settings_reference;
    DatabaseManager databaseManager;

    void Awake()
    {            
        //On Application Start clear data in Cache.
        Caching.ClearCache();

        //Set Persistence: Disables storing information on the device for when there is no internet connection.
        FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
        
        //Get Reference for the correct application settings. To dynamically connect to different RTDB and Storage.
        settings_reference =  FirebaseDatabase.DefaultInstance.GetReference("ApplicationSettings2");
        
        //Add script components to objects in the scene
        databaseManager = Databasemanager.AddComponent<DatabaseManager>();  
        InstantiateObjects instantiateObjects = Instantiateobjects.AddComponent<InstantiateObjects>();
        CheckFirebase checkFirebase = Checkfirebase.AddComponent<CheckFirebase>();

        //Initialize Firebase 
        checkFirebase.FirebaseInitialized += Initialized;
        
        //Fetch Settings Design Reference and Storage Reference from ApplicationSettings Reference.
        databaseManager.FetchSettingsData(settings_reference);

        //Fetch data from realtime database
        databaseManager.ApplicationSettingUpdate += databaseManager.FetchData;

        //Initialize the database.. once the database is initialized the objects are instantiated
        databaseManager.DatabaseInitializedDict += instantiateObjects.OnDatabaseInitializedDict;

        //Add listners after initial objects have been placed to avoid simultanous item placement
        instantiateObjects.PlacedInitialElements += databaseManager.AddListeners;

        //Trigger events for updates in the database.
        databaseManager.DatabaseUpdate += instantiateObjects.OnDatabaseUpdate;

    }


    public void Initialized(object sender, EventArgs e)
    {
        Debug.Log("I am evoked through database initializiation");	
    }  
    
}

