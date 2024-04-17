using UnityEngine;
using Firebase;
using Firebase.Extensions;
using UnityEngine.SceneManagement;


public class FirebaseInitializer : MonoBehaviour
{
    public MqttFirebaseConfigManager mqttConfigManager;

    public void Start()
    {
        mqttConfigManager = FindObjectOfType<MqttFirebaseConfigManager>();
        if (mqttConfigManager == null)
        {
            Debug.LogError("MqttConfigManager not found in the scene.");
        }
    }

    public void InitializeFirebase()
    {
        Debug.Log("We are starting to initialize Firebase");
        Debug.Log("Test Print" + " " + FirebaseManager.Instance.appId);

        AppOptions options = new AppOptions
        {
            AppId = FirebaseManager.Instance.appId,
            ApiKey = FirebaseManager.Instance.apiKey,
            DatabaseUrl = new System.Uri(FirebaseManager.Instance.databaseUrl),
            StorageBucket = FirebaseManager.Instance.storageBucket,
            ProjectId = FirebaseManager.Instance.projectId,
        };

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            Firebase.DependencyStatus dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.Create(options);

                if (app != null)
                {
                    Debug.Log("Firebase Initialized Successfully");
                    Debug.Log($"App Name: {app.Name}");

                    //Disconnect from MQTT
                    mqttConfigManager.Disconnect();
                    Debug.Log("Disconnected from MQTT");

                    // Load the new scene
                    ChangeScene("Log");
                    
                }
                else
                {
                    Debug.LogError("Failed to create Firebase app. Please check your configuration.");
                }
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    private void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

}