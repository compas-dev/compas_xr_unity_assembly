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
            apiKey = "AIzaSyBg2ES85_rL6Aeu76MXKsI4b6RYWW5V2hg";
            databaseUrl = "https://test-project-94f41-default-rtdb.europe-west1.firebasedatabase.app";
            storageBucket = "test-project-94f41.appspot.com";
            projectId = "test-project-94f41";

            CompasXR.Systems.OperatingSystem currentOS = OperatingSystemManager.GetCurrentOS();
            switch (currentOS)
            {
                case CompasXR.Systems.OperatingSystem.iOS:
                    appId = "1:116159730378:ios:a99ce204d214df3c0b5a33";
                    break;
                case CompasXR.Systems.OperatingSystem.Android: 
                    appId = "1:116159730378:android:a99ce204d214df3c0b5a33";
                    break;
                default:
                    appId = "1:116159730378:android:a99ce204d214df3c0b5a33";
                    break;
            }
        }


    }
}