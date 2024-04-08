using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Instantiate;
using JSON;
using Newtonsoft.Json;
using UnityEngine.Events;

//Notes: The entirity of the file is structured as a class that inherits from MonoBehaviour (Unity's base class for scripts)
//Notes: Script is used for connecting onScreen events User Interaction with results or actions.

public class UIFunctionalities : MonoBehaviour
{

    //Notes: Class Member Variables, used to store data and references ex. public DatabaseReference dbreferenceMyFramesData;
    //Notes: The public keyword is named an access modifier, it determines how Member variables, methods, and classes can be accessed from other classes.

    //TODO: MAS: Create references to store other scripts
    //......


    //TODO: MAS: 2. Create a reference to store the Fetch Data button object
    //......

    //TODO: MAS: 6. Create a reference to store the Move Objects Toggle object
    //......

    //TODO: MAS: 7. Create a reference to store the Reset Data Button object
    //......

    //TODO: MAS: 8. Create a reference to store the Publish Data button object
    //......

    //Notes: Built in Unity methods (methods that come from the inheritance of the MonoBehaviour class)
    /*
        Notes: Awake is called when the script instance is being loaded. This occurs before any Start methods are called.
        Notes: Start is called on the frame when a script is enabled just before any of the Update methods are called the first time.
        Notes: Update is called every frame, if the MonoBehaviour is enabled.
        Notes: OnDestroy is called when the MonoBehaviour will be destroyed.
        Notes: OnEnable is called when the object becomes enabled and active.
    */
    void Start()
    {
        //Method wrapper for all things that need to happen in the on start method.
        OnStartInitilization();
    }
    void Update()
    {
        //TODO: MAS: 6. Call the movement toggle in the update method
        //.....
    }

    /////////////////////////////// Initilization and Set up Actions //////////////////////////////

    private void OnStartInitilization()
    {
        //TODO: MAS: Find Other Script References....
        //...


        //TODO: MAS: 2.Fetch Data Button Set up....

        //Find the game object that the button is associated with in the scene
        //....

        //Get the button component from the game object
        //....
        
        //Add an onClick event listner to the button
        //....



        //TODO: MAS: 6. Object Movement Toggle Set up.....

        //Find the game object that the toggle is associated with in the scene
        //...

        //Find and store the toggle component from the game object
        //....


        //TODO: MAS: 7. Set the Reset Data Button....

        //Find the game object that the button is associated with in the scene
        //....

        //Get the button component from the game object
        //....

        //Add an onClick event listner to the button
        //....


        // TODO: MAS: 8. Publish Data Button Method...

        //Find the game object that the button is associated with in the scene
        //....

        //Get the button component from the game object
        //....

        //Add an onClick event listner to the button
        //....

    }

    /////////////////////////////////// UI Button and Toggle Actions /////////////////////////////

    //TODO: MAS: 2.Write a method that we can link to a button for publishing to the firebase
    public void FetchDataButtonMethod()
    {
        Debug.Log("FetchDataButtonMethod: Fetch Data Button Pressed");

        //Call the Database Manager Method and Reference to fetch the data

    }
    
    //TODO: MAS: 6. Write a method that will toggle on and off movement for the objects (tip: design it for the update method)
    public void ToggleObjectMovement(Toggle toggle)
    {
        Debug.Log("ToggleObjectMovement: Objects should be moving now.");

        //Call Method from instantiateObjects to start moving objects

    }

    //TODO: MAS: 7. Write a method that will reset everything (destroy game objects, and clear the dictionary)
    public void ResetDataButtonMethod()
    {
        Debug.Log("ResetDataButtonMethod: Reset Data Button Pressed");

        //Call the Database Manager Method and Reference to reset the data

        //Instantiate Objects Method to destroy all child objects of the storg Object

    }

    //TODO: MAS: 8. Write a method that we can link to a button for publishing to the firebase
    public void PublishDataButtonMethod()
    {
        Debug.Log("PublishDataButtonMethod: Publish Data Button Pressed");

        //Utilize instantiate Objects method to create a rhino compatible dictionary for objects

        //Call the Database Manager Method and Reference to publish the data

    }

}

