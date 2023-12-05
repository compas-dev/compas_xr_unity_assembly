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

    FirebaseManager() {
      appId = "6:641027065982:android:ad03c691095c1507dab02f";
      apiKey = "AIzaSyCrvr3FD8TxvdxTFqPvRM4z-Qs4841-s7A";
      databaseUrl = "https://cdf-project-f570f-default-rtdb.europe-west1.firebasedatabase.app";
      storageBucket = "cdf-project-f570f.appspot.com";
      projectId = "cdf-project-f570f";
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
