using System;
using System.IO;
using UnityEngine;

namespace CompasXR.Systems
{
    public class LogManager : MonoBehaviour
    {
        private string logDirectoryPath;
        private string logFilePath;

        void Awake()
        {
            //Make the GameObject persistent across scene changes
            DontDestroyOnLoad(gameObject);

            //Create the directory path reference
            string persistentDataPath = Application.persistentDataPath;
            logDirectoryPath = Path.Combine(persistentDataPath, "CompasXRLogStorage");

            //Create the file path reference
            string dateString = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            logFilePath = Path.Combine(logDirectoryPath, $"{dateString}_{SystemInfo.deviceUniqueIdentifier}_log.txt");

            Debug.Log($"Log file path: {logFilePath}"); 

            //Manage the log directory
            ManageLogDirectory(logDirectoryPath);

            //Subscribe to the log message event
            Application.logMessageReceived += HandleLogMessage;
        }

        public void ManageLogDirectory(string directoryPath)
        {

            //Create the directory if it doesn't exist
            if (!Directory.Exists(logDirectoryPath))
            {
                Directory.CreateDirectory(logDirectoryPath);
            }

            //Get the list of files in the directory
            string [] files = Directory.GetFiles(directoryPath);

            //If the number of files in the directory is greater than or equal to 50, delete all the files
            if(files.Length >= 50)
            {
                foreach (string file in files)
                {
                    File.Delete(file);
                }
            }
        }


        private void HandleLogMessage(string logString, string stackTrace, LogType type)
        {
            //Get the active Scene name to know what scene is currnetly running
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            //Write the log message to the file with the current time, SceneName, type, and stack trace
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]: {sceneName}: {type}: {logString}");
                writer.WriteLine(stackTrace);
                writer.WriteLine();
            }
        }

        private void OnDestroy()
        {
            //Unsubscribe from the log message event
            Application.logMessageReceived -= HandleLogMessage;

            Debug.Log($"OnDestroy: LogManager saved log file to {logFilePath}");    
        }
    }
}
