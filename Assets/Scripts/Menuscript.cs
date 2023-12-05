using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menuscript : MonoBehaviour
{
    // Private variables
    private GameObject Reload_Button;
    private GameObject Editor_Button;
    private GameObject Info_Button;
    private GameObject Background;
    private GameObject Communication;

    // Start is called before the first frame update
    void Start()
    {
        // Find GameObjects by name
        Reload_Button = GameObject.Find("Reload_Button");
        Editor_Button = GameObject.Find("Editor_Button");
        Info_Button = GameObject.Find("Info_Button");
        Background = GameObject.Find("Background_Menu");
        Communication = GameObject.Find("Communication");

        // Ensure all GameObjects were found
        if (!Reload_Button || !Editor_Button || !Info_Button || !Background || !Communication)
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
        SetMenuButtonsActive(false);
    }

    // Toggle ON and OFF the dropdown submenu options
    private void Menu_Toggle()
    {
        // Toggle the active state of the buttons
        SetMenuButtonsActive(!Reload_Button.activeSelf);
    }

    // Helper method to set the active state of menu buttons
    private void SetMenuButtonsActive(bool isActive)
    {
        Reload_Button.SetActive(isActive);
        Editor_Button.SetActive(isActive);
        Info_Button.SetActive(isActive);
        Background.SetActive(isActive);
        Communication.SetActive(isActive);
    }
}
