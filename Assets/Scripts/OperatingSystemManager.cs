using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CompasXR.Systems
{
    public enum OperatingSystem
    {
        Android,
        iOS,
        // Windows,
        // MacOS,
        // Linux,
        Unknown
    }

    public class OperatingSystemManager : MonoBehaviour
    {
        public static OperatingSystem GetCurrentOS()
        {
            #if UNITY_ANDROID
            Debug.Log("Operating System: Android");
            return OperatingSystem.Android;
            #elif UNITY_IOS
            Debug.Log("Operating System: iOS");
            return OperatingSystem.iOS;

            // #elif UNITY_STANDALONE_WIN
            // Debug.Log("Operating System: Windows");
            // return OperatingSystem.Windows;
            // #elif UNITY_STANDALONE_OSX
            // Debug.Log("Operating System: MacOS");
            // return OperatingSystem.MacOS;
            // #elif UNITY_STANDALONE_LINUX
            // Debug.Log("Operating System: Linux");
            // return OperatingSystem.Linux;

            #else
            Debug.Log("Operating System: Unknown");
            return OperatingSystem.Unknown; 
            #endif
        }
    }
}
