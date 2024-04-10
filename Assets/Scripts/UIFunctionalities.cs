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

    //MAS: Create references to store other scripts
    public DatabaseManager databaseManager;
    public InstantiateObjects instantiateObjects;

    //TODO: MAS: 2. Create a reference to store the Fetch Data button object
    public GameObject fetchDataButtonObject;

    //TODO: MAS: 6. Create a reference to store the Move Objects Toggle object
    public Toggle MyObjectMovementToggle;

    //TODO: MAS: 7. Create a reference to store the Reset Data Button object
    public GameObject resetButtonObject; 

    //TODO: MAS: 8. Create a reference to store the Publish Data button object
    public GameObject myPublishButtonObject;

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
        ToggleObjectMovement(MyObjectMovementToggle);
    }

    /////////////////////////////// Initilization and Set up Actions //////////////////////////////

    private void OnStartInitilization()
    {
        //MAS: Find Other Script References....
        databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
        instantiateObjects = GameObject.Find("InstantiateObjects").GetComponent<InstantiateObjects>();


        //MAS: 2.Fetch Data Button Set up....

        //Find the game object that the button is associated with in the scene
        fetchDataButtonObject = GameObject.Find("FetchDataButton");

        //Get the button component from the game object
        Button fetchDataButton = fetchDataButtonObject.GetComponent<Button>();
        
        //Add an onClick event listner to the button
        fetchDataButton.onClick.AddListener(FetchDataButtonMethod);



        //TODO: MAS: 6. Object Movement Toggle Set up.....

        //Find the game object that the toggle is associated with in the scene
        GameObject myMovementToggleObject = GameObject.Find("MyObjectMoveToggle");

        //Find and store the toggle component from the game object
        MyObjectMovementToggle = myMovementToggleObject.GetComponent<Toggle>();



        //TODO: MAS: 7. Set the Reset Data Button....

        //Find the game object that the button is associated with in the scene
        resetButtonObject = GameObject.Find("MyResetButton");

        //Get the button component from the game object
        Button resetButton = resetButtonObject.GetComponent<Button>();

        //Add an onClick event listner to the button
        resetButton.onClick.AddListener(ResetDataButtonMethod);


        // TODO: MAS: 8. Publish Data Button Method...

        //Find the game object that the button is associated with in the scene
        myPublishButtonObject = GameObject.Find("MyPublishingButton");

        //Get the button component from the game object
        Button myPublishButton = myPublishButtonObject.GetComponent<Button>(); 

        //Add an onClick event listner to the button
        myPublishButton.onClick.AddListener(PublishDataButtonMethod);

    }

    /////////////////////////////////// UI Button and Toggle Actions /////////////////////////////

    //MAS: 2. Write a method that we can link to a button for publishing to the firebase
    public async void FetchDataButtonMethod()
    {
        Debug.Log("FetchDataButtonMethod: Fetch Data Button Pressed");

        //Call the Database Manager Method and Reference to fetch the data
        await databaseManager.FetchRealTimeDatabaseData(databaseManager.MyFramesReference);
    }
    
    //TODO: MAS: 6. Write a method that will toggle on and off movement for the objects (tip: design it for the update method)
    public void ToggleObjectMovement(Toggle toggle)
    {

        //Call Method from instantiateObjects to start moving objects
        if(toggle.isOn)
        {
            Debug.Log("ToggleObjectMovement: Objects should be moving now.");

            //Call the method for moving all of my child objects by y vec
            instantiateObjects.MoveAllChildrenByVector(instantiateObjects.MyRuntimeParentStorageObject);
        }
        else
        {
            Debug.Log("TogglObjectMovement: Objects should be stationary now.");
        }


    }

    //TODO: MAS: 7. Write a method that will reset everything (destroy game objects, and clear the dictionary)
    public void ResetDataButtonMethod()
    {
        Debug.Log("ResetDataButtonMethod: Reset Data Button Pressed");

        //Call the Database Manager Method and Reference to reset the data
        databaseManager.MyFramesDictionary.Clear();

        //Instantiate Objects Method to destroy all child objects of the storg Object
        instantiateObjects.DestroyAllChildren(instantiateObjects.MyRuntimeParentStorageObject);

    }

    //TODO: MAS: 8. Write a method that we can link to a button for publishing to the firebase
    public void PublishDataButtonMethod()
    {
        Debug.Log("PublishDataButtonMethod: Publish Data Button Pressed");

        //Utilize instantiate Objects method to create a rhino compatible dictionary for objects
        Dictionary<string,Frame> tempPublishData = instantiateObjects.ConvertParentChildrenToRhinoFrameData(instantiateObjects.MyRuntimeParentStorageObject, databaseManager.MyFramesDictionary);

        //Call the Database Manager Method and Reference to publish the data
        databaseManager.PushStringData(databaseManager.MyFramesReference, JsonConvert.SerializeObject(tempPublishData));
    }

}

