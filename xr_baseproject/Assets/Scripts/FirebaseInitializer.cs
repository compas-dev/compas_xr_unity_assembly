using UnityEngine;
using UnityEngine.UI;
using Firebase;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using TMPro;
using System.Collections;


public class FirebaseInitializer : MonoBehaviour
{
    public TMP_InputField applicationIdInput;
    public TMP_InputField apiKeyInput;
    public TMP_InputField databaseUrlInput;
    public TMP_InputField storageBucketInput;
    public TMP_InputField projectIdInput;

    public UserInputs userInputs;
    private MqttController mqttController;

    private void Awake()
    {
        // Find the MqttController in the scene and subscribe to its onUserInputsUpdated event
        mqttController = FindObjectOfType<MqttController>();
        if (mqttController != null)
        {
            mqttController.onUserInputsUpdated.AddListener(UpdateInputFields);
        }
    }

     private void Start()
    {
        applicationIdInput.text = userInputs.appId;
        apiKeyInput.text = userInputs.apiKey;
        databaseUrlInput.text = userInputs.databaseUrl;
        storageBucketInput.text = userInputs.storageBucket;
        projectIdInput.text = userInputs.projectId;
    }

    private void UpdateInputFields()
    {
        applicationIdInput.text = mqttController.userInputs.appId;
        apiKeyInput.text = mqttController.userInputs.apiKey;
        databaseUrlInput.text = mqttController.userInputs.databaseUrl;
        storageBucketInput.text = mqttController.userInputs.storageBucket;
        projectIdInput.text = mqttController.userInputs.projectId;
    }


    // private void Update()
    // {
    //     applicationIdInput.text = userInputs.appId;
    //     apiKeyInput.text = userInputs.apiKey;
    //     databaseUrlInput.text = userInputs.databaseUrl;
    //     storageBucketInput.text = userInputs.storageBucket;
    //     projectIdInput.text = userInputs.projectId;
    // }

    //   private void Start()
    // {
    //     userInputs.appId = applicationIdInput.text;
    //     userInputs.apiKey = apiKeyInput.text;
    //     userInputs.databaseUrl = databaseUrlInput.text;
    //     userInputs.storageBucket = storageBucketInput.text;
    //     userInputs.projectId = projectIdInput.text;
        
    //     Debug.Log(userInputs.appId);
    // }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void printsmth()
    {
        Debug.Log("test");
    }

    public void InitializeFirebase()
    {
        Debug.Log("We are starting to initialize Firebase");
        string appId = applicationIdInput.text;
        string apiKey = apiKeyInput.text;
        System.Uri databaseUrl = new System.Uri(databaseUrlInput.text);
        string storageBucket = storageBucketInput.text;
        string projectId = projectIdInput.text;

        AppOptions options = new AppOptions
        {
            AppId = appId,
            ApiKey = apiKey,
            DatabaseUrl = databaseUrl,
            StorageBucket = storageBucket,
            ProjectId = projectId,

            // AppId = userInputs.appId,
            // ApiKey = userInputs.apiKey,
            // DatabaseUrl = userInputs.databaseUrl,
            // StorageBucket = userInputs.storageBucket,
            // ProjectId = userInputs.projectId,
        };

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            Firebase.DependencyStatus dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.Create(options);

                if (app != null)
                {
                    Debug.Log("Firebase Initialized Successfully");
                    Debug.Log($"App Name: {app.Name}");

                    // Load the new scene
                    StartCoroutine(ChangeSceneAfterInitialization("Log"));
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

    private IEnumerator ChangeSceneAfterInitialization(string sceneName)
    {
        yield return null; // Wait for the next frame
        ChangeScene(sceneName);
    }
}