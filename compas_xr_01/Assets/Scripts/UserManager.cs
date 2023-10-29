using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Newtonsoft.Json;
using UnityEngine.SceneManagement; // Added namespace for scene management

public class UserManager : MonoBehaviour
{
    private string userID;
    private DatabaseReference dbReference_root;
    public TMPro.TMP_InputField Username;

    // Start is called before the first frame update
    void Start()
    {
        userID = SystemInfo.deviceUniqueIdentifier;
        dbReference_root = FirebaseDatabase.DefaultInstance.RootReference;
        
        if (dbReference_root == null)
        {
            Debug.LogError("Firebase Database reference is null!");
        }
        else
        {
            print("Firebase Database reference is initialized.");
        }
    }

    public void CreateUser()
    {
        if (dbReference_root == null)
        {
            Debug.LogError("dbReference_root is null! Make sure Firebase is initialized and the Start method is called.");
            return;
        }

        if (Username == null || string.IsNullOrWhiteSpace(Username.text))
        {
            Debug.LogError("Username is not assigned or empty!");
            return;
        }
        
        string time = System.DateTime.UtcNow.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
        string playerName = Username.text.ToLower();

        // Check if the user with the same playerName exists in the database
        dbReference_root.Child("Users").Child(playerName).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error occurred while reading data from the database.");
                print("root child error. Error occurred while reading data from the database.");
                return;
            }

            DataSnapshot snapshot = task.Result;

            if (snapshot.Exists)
            {   
                // User with the same playerName exists, check if device exists
                if (snapshot.Child("devices").Child(userID).Exists)
                {
                    // Device exists, update the "dates" node
                    List<string> currentDates = new List<string>();
                    foreach (var dateSnapshot in snapshot.Child("devices").Child(userID).Child("dates").Children)
                    {
                        string date = dateSnapshot.GetValue(true).ToString();
                        currentDates.Add(date);
                    }

                    currentDates.Add(time);
                    print("added time");
                    dbReference_root.Child("Users").Child(playerName).Child("devices").Child(userID).Child("dates").SetValueAsync(currentDates).ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            Debug.LogError("Failed to update user.");
                        }
                        else if (t.IsCompleted)
                        {
                            Debug.Log("User updated successfully.");
                            LoadNextScene(); // Change the scene here
                        }
                    });
                }
                else
                {   
                    // Device does not exist, create a new "devices" node
                    dbReference_root.Child("Users").Child(playerName).Child("devices").Child(userID).Child("dates").Child("0").SetValueAsync(time).ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            Debug.LogError("Failed to create device.");
                        }
                        else if (t.IsCompleted)
                        {
                            Debug.Log("Device created successfully.");
                            LoadNextScene(); // Change the scene here
                        }
                    });
                }
            }
            else
            {   
                // User with the same playerName doesn't exist, create a new node
                User newUser = new User();
                Device newDevice = new Device();
                newDevice.dates.Add(time);
                newUser.devices.Add(userID, newDevice);
                string json = JsonConvert.SerializeObject(newUser);
                dbReference_root.Child("Users").Child(playerName).SetRawJsonValueAsync(json).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Debug.LogError("Failed to create user.");
                    }
                    else if (t.IsCompleted)
                    {
                        Debug.Log("User created successfully.");
                        LoadNextScene(); // Change the scene here
                    }
                });
            }
        });
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene("MainGame"); // Replace with the name of your next scene
    }
}
