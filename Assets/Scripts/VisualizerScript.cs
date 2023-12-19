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

    //private bool isActorViewActive = false; // Track the current view state
    private InstantiateObjects instantiateObjectsScript; 
    private DatabaseManager databaseManager;


    // Start is called before the first frame update
    void Start()
    {
        // Find and assign the InstantiateObjects script
        instantiateObjectsScript = FindObjectOfType<InstantiateObjects>();
        if (instantiateObjectsScript == null)
        {
            Debug.LogError("InstantiateObjects script not found in the scene.");
            return;
        }

        // Find and assign the DatabaseManager script
        databaseManager = FindObjectOfType<DatabaseManager>();
        if (databaseManager == null)
        {
            Debug.LogError("DatabaseManager script not found in the scene.");
            return;
        }

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

    // Helper method to set the active state of menu buttons
    private void SetMenuButtonsActive(bool isActive)
    {
        Preview_Builder.SetActive(isActive);
        ID_Button.SetActive(isActive);
        ObjectLength_Button.SetActive(isActive);
        Robot_Button.SetActive(isActive);
        Background.SetActive(isActive);
    }



    public void ToggleVisualizationMode()
    {
        // Check the current mode and toggle it
        if (instantiateObjectsScript.currentMode != InstantiateObjects.VisualizationMode.ActorView)
        {
            // If current mode is BuiltUnbuilt, switch to ActorView
            instantiateObjectsScript.currentMode = InstantiateObjects.VisualizationMode.ActorView;

         
            instantiateObjectsScript.ApplyColorBasedOnActor();
        }
        else if (instantiateObjectsScript.currentMode != InstantiateObjects.VisualizationMode.BuiltUnbuilt)
        {
            // If current mode is not BuiltUnbuilt switch to BuiltUnbuilt
            instantiateObjectsScript.currentMode = InstantiateObjects.VisualizationMode.BuiltUnbuilt;

            instantiateObjectsScript.ApplyColorBasedOnBuildState();
        }
        else 
        {
        Debug.Log("Error: Visualization Mode not found");
        }
    }


    // ID BUTTON
    // public void IDText_Toggle()
    // {
    //     isTextAndImageVisible = !isTextAndImageVisible;
        
    //     if (instantiateObjectsScript != null && instantiateObjectsScript.Elements != null)
    //     {
    //         foreach (Transform child in instantiateObjectsScript.Elements.transform)
    //         {
    //             // Toggle Text Object
    //             Transform textChild = child.Find(child.name + " Text");
    //             if (textChild != null)
    //                 {
    //                     textChild.gameObject.SetActive(!textChild.gameObject.activeSelf);
    //                 }

    //             // Toggle Circle Image Object
    //             Transform circleImageChild = child.Find("circleImage(Clone)");
    //             if (circleImageChild != null)
    //                 {
    //                     circleImageChild.gameObject.SetActive(!circleImageChild.gameObject.activeSelf);
    //                 }
    //             }
    //     }
    //     else
    //     {
    //         Debug.LogError("InstantiateObjects script or Elements object not set.");
    //     }
    // }

    public void IDText_Toggle()
    {
        if (instantiateObjectsScript != null && instantiateObjectsScript.Elements != null)
        {
            // Update the visibility state
            instantiateObjectsScript.isTextAndImageVisible = !instantiateObjectsScript.isTextAndImageVisible;

            foreach (Transform child in instantiateObjectsScript.Elements.transform)
            {
                // Toggle Text Object
                Transform textChild = child.Find(child.name + " Text");
                if (textChild != null)
                {
                    textChild.gameObject.SetActive(instantiateObjectsScript.isTextAndImageVisible);
                }
                // Toggle Circle Image Object
                Transform circleImageChild = child.Find("circleImage(Clone)");
                if (circleImageChild != null)
                {
                    circleImageChild.gameObject.SetActive(instantiateObjectsScript.isTextAndImageVisible);
                }
            }
        }
        else
        {
            Debug.LogError("InstantiateObjects script or Elements object not set.");
        }
    }

 

}
