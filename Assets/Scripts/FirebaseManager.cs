using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using CompasXR.Systems;


namespace CompasXR.Database.FirebaseManagment
{
    /*
    * CompasXR.Database.FirebaseManagement : A namespace to define and controll various Firebase connection,
    * configuration information, user record, and general database management.
    */

    public sealed class FirebaseManager
    {
        /*
        * FirebaseManager : Sealed class using the Singleton Pattern.
        * This class is used to manage the Firebase configuration settings.
        */

        private static FirebaseManager instance = null;
        private static readonly object padlock = new object();
        public static FirebaseManager Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new FirebaseManager();
                    }
                    return instance;
                }
            }
        }
        public string appId;
        public string apiKey;
        public string databaseUrl;
        public string storageBucket;
        public string projectId;

        FirebaseManager() 
        {
            apiKey = "AIzaSyBuZrrhXNZ19bMHIEbM3lDjFT5QZ9ASnPA";
            databaseUrl = "https://mas-t2-compas-xr-default-rtdb.europe-west1.firebasedatabase.app";
            storageBucket = "mas-t2-compas-xr.appspot.com";
            projectId = "mas-t2-compas-xr";

            CompasXR.Systems.OperatingSystem currentOS = OperatingSystemManager.GetCurrentOS();
            switch (currentOS)
            {
                case CompasXR.Systems.OperatingSystem.iOS:
                    appId = "1:825413694819:ios:ea365dc53356ff2f064f56";
                    break;
                case CompasXR.Systems.OperatingSystem.Android: 
                    appId = "1:825413694819:android:ea365dc53356ff2f064f56";
                    break;
                default:
                    appId = "1:825413694819:android:ea365dc53356ff2f064f56";
                    break;
            }
        }


    }
}