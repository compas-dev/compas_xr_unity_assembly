using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisualizerScript : MonoBehaviour
{
    // Private variables
    private GameObject Preview_Builder;
    private GameObject ID_Button;
    private GameObject ObjectLength_Button;
    private GameObject Robot_Button;
    private GameObject Background;

    private bool isActorViewActive = false; // Track the current view state
    private InstantiateObjects instantiateObjectsScript; // Reference to InstantiateObjects script

    // Start is called before the first frame update
    void Start()
    {
        // Find GameObjects by name
        Preview_Builder = GameObject.Find("Preview_Builder");
        ID_Button = GameObject.Find("ID_Button");
        ObjectLength_Button = GameObject.Find("ObjectLength_Button");
        Robot_Button = GameObject.Find("Robot_Button");
        Background = GameObject.Find("Background_Visualizer");

        // Ensure all GameObjects were found
        if (!Preview_Builder || !ID_Button || !ObjectLength_Button || !Robot_Button || !Background)
        {
            Debug.LogError("One or more GameObjects were not found. Please check their names.");
            return;
        }

        // Find and assign the InstantiateObjects script
        instantiateObjectsScript = FindObjectOfType<InstantiateObjects>();
        if (instantiateObjectsScript == null)
        {
            Debug.LogError("InstantiateObjects script not found in the scene.");
            return;
        }

        // For each button, define OnClick Action and prefab
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(Visualizer_Toggle);
        }
        else
        {
            Debug.LogError("Button component not found on the GameObject.");
        }

        // Initially hide all menu buttons
        SetMenuButtonsActive(false);
    }
    // Toggle ON and OFF the dropdown submenu options
    private void Visualizer_Toggle()
    {
        // Toggle the active state of the buttons
        SetMenuButtonsActive(!Robot_Button.activeSelf);
    }

    public void PreviewBuilder_Toggle()
    {
        // Toggle the view state for Preview Builder
        isActorViewActive = !isActorViewActive;

        // Apply the appropriate coloring based on the current view state
        if (isActorViewActive)
        {
            // Apply color based on actor
            instantiateObjectsScript.ApplyColorBasedOnActor();
        }
        else
        {
            // Apply color based on built/unbuilt state
            instantiateObjectsScript.ApplyColorBasedOnBuildState();
        }
    }

    // Helper method to set the active state of menu buttons
    private void SetMenuButtonsActive(bool isActive)
    {
        Preview_Builder.SetActive(isActive);
        ID_Button.SetActive(isActive);
        ObjectLength_Button.SetActive(isActive);
        Robot_Button.SetActive(isActive);
        Background.SetActive(isActive);
    }
}
