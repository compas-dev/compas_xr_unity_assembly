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

    private void Start()
    {
        Debug.Log("Hellooooo");
        applicationIdInput.text = "1:641027065982:android:ad03c691095c1507dab02f";
        apiKeyInput.text = "AIzaSyCrvr3FD8TxvdxTFqPvRM4z-Qs4841-s7A";
        databaseUrlInput.text = "https://cdf-project-f570f-default-rtdb.europe-west1.firebasedatabase.app";
        storageBucketInput.text = "cdf-project-f570f.appspot.com";
        projectIdInput.text = "cdf-project-f570f";
    }

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