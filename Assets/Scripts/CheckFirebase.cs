using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Firebase;
using Firebase.Extensions;
using Firebase.Database;

public class CheckFirebase : MonoBehaviour
{
    //Check connection to the firebase in the start method to determine if the connection is successful & established
    public void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError(message: $"Failed to initialize Firebase with {task.Exception}");
                return;
            }
            else if (task.IsCompleted)
            {
                Debug.Log("Firebase Successfully Initilized :)");
            }
        });
    }

}
