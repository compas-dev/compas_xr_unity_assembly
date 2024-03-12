using UnityEngine;
using UnityEngine.XR.ARFoundation;


public class OcclusionManagerAR : MonoBehaviour
{
    private AROcclusionManager occlusionManager;

    void Awake()
    {
        // Attempt to find the AROcclusionManager component within the XR Origin hierarchy
        occlusionManager = GetComponentInChildren<AROcclusionManager>(true);
    }

    void Start()
    {
        if (occlusionManager != null)
        {
            if (OperatingSystemManager.GetCurrentOS() == OperatingSystem.iOS)
            {
                // Enable the AR Occlusion Manager for iOS devices
                occlusionManager.enabled = true;
                Debug.Log("AR Occlusion enabled for iOS.");
            }
            else
            {
                // Optionally, disable or leave the occlusion manager disabled for other platforms
                // occlusionManager.enabled = false;
                Debug.Log("AR Occlusion will not be Enabled because this is a non-iOS platforms.");
            }
        }
        else
        {
            Debug.LogWarning("AROcclusionManager not found in the XR Origin hierarchy.");
        }
    }
}
