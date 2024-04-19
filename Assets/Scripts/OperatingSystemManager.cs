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

            #else
            Debug.Log("Operating System: Unknown");
            return OperatingSystem.Unknown; 
            #endif
        }
    }
}
