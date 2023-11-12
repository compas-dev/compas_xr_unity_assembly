using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonsUnfold : MonoBehaviour
{
    // Public array to hold menu items
    public GameObject[] menuItems;

    // Start is called before the first frame update
    void Start()
    {
        // Assigning OnClick Action to the button this script is attached to
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(ToggleMenuItems);

        // Initially hide all menu items and the background
        SetMenuItemsActive(false);
    }

    // Method to toggle the visibility of menu items
    private void ToggleMenuItems()
    {
        // Determine the current state based on the first item in the array
        bool areItemsActive = menuItems.Length > 0 && menuItems[0].activeSelf;

        // Set the opposite state for all menu items
        SetMenuItemsActive(!areItemsActive);
    }

    // Helper method to set the active state of all menu items
    private void SetMenuItemsActive(bool state)
    {
        foreach (GameObject item in menuItems)
        {
            item.SetActive(state);
        }
    }
}
