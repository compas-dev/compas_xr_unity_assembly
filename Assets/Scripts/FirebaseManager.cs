using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public sealed class FirebaseManager
{
    private static FirebaseManager instance = null;
    private static readonly object padlock = new object();

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

        OperatingSystem currentOS = OperatingSystemManager.GetCurrentOS();
        switch (currentOS)
        {
            case OperatingSystem.iOS:
            appId = "1:825413694819:ios:ea365dc53356ff2f064f56";
            break;
            case OperatingSystem.Android: 
            appId = "1:825413694819:android:ea365dc53356ff2f064f56";
            break;
            default:
            appId = "1:825413694819:android:ea365dc53356ff2f064f56";
            break;
        }
    }

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
}
