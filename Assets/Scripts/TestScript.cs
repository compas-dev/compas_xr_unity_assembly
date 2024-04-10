using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.UI;

public class TestScript : MonoBehaviour
{

    // Class member Varible to store my button.
    public GameObject myButton;

    public Toggle myToggle;

    // Start is called before the first frame update
    void Start()
    {
        //Find the button in the scene
        myButton = GameObject.Find("MyButton");

        //Get the Button Component from the object
        Button button = myButton.GetComponent<Button>();

        //Setting up an onClick Action.
        button.onClick.AddListener(PrintHelloOnClick);


        //Find toggle gameObject in the scene
        GameObject myToggleObject = GameObject.Find("MyToggle");

        //Get toggle component from the gameObject
        myToggle = myToggleObject.GetComponent<Toggle>();

        //Add OnValueChanged action
        myToggle.onValueChanged.AddListener(ToggleTestMethod);
    }
   
    // Update is called once per frame
    void Update()
    {
        //Toggle State printing
        CheckToggleState(myToggle);
    }
    
    //Print on button click action
    public void PrintHelloOnClick()
    {
        Debug.Log("HELLO EVERYONE :)");
    }

    //Print toggle state method
    public void ToggleTestMethod(bool toggleState)
    {
        if(toggleState)
        {
            Debug.Log("Toggle is currently ON");
        }
        else
        {
            Debug.Log("Toggle is currently OFF");
        }
    }

    //Check state of the toggle
    public void CheckToggleState(Toggle myRandomlyNamedToggle)
    {
        if(myRandomlyNamedToggle.isOn)
        {
            Debug.Log("My toggle state is on");
        }
        else
        {
            Debug.Log("My toggle state is off");
        }
    }
}
