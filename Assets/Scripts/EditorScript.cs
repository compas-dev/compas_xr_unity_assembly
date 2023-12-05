using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorScript : MonoBehaviour
{
    // Private variables
    private GameObject BuilderEditorButton;
    private GameObject BuildStatusEditorButton;
    private GameObject Background;

    // Start is called before the first frame update
    void Start()
    {
        // Find GameObjects by name
        BuilderEditorButton = GameObject.Find("Builder Editor");
        BuildStatusEditorButton = GameObject.Find("Build Status Editor");
        Background = GameObject.Find("Background_Editor");

        // Ensure all GameObjects were found
        if (!BuilderEditorButton || !BuildStatusEditorButton || !Background)
        {
            Debug.LogError("One or more GameObjects were not found. Please check their names.");
            return;
        }

        // For each button, define OnClick Action and prefab
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(Menu_Toggle);
        }
        else
        {
            Debug.LogError("Button component not found on the GameObject.");
        }

        // Initially hide all menu buttons
        SetEditorButtonsActive(false);
    }

    // Toggle ON and OFF the dropdown submenu options
    private void Menu_Toggle()
    {
        // Toggle the active state of the buttons
        SetEditorButtonsActive(!BuilderEditorButton.activeSelf);
    }

    // Helper method to set the active state of menu buttons
    private void SetEditorButtonsActive(bool isActive)
    {
        BuilderEditorButton.SetActive(isActive);
        BuildStatusEditorButton.SetActive(isActive);
        Background.SetActive(isActive);
    }
}
