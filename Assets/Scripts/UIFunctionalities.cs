using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Instantiate;
using JSON;
using Newtonsoft.Json;

//Notes: The entirity of the file is structured as a class that inherits from MonoBehaviour (Unity's base class for scripts)
//Notes: Script is used for connecting onScreen events User Interaction with results or actions.

public class UIFunctionalities : MonoBehaviour
{

    //Notes: Class Member Variables, used to store data and references ex. public DatabaseReference dbreferenceMyFramesData;
    //Notes: The public keyword is named an access modifier, it determines how Member variables, methods, and classes can be accessed from other classes.

    //TODO: MAS: Create a reference to the DatabaseManager script
    public DatabaseManager databaseManager; //Notes: Example 1 of finding an object in the scene (searching)

    //TODO: MAS: Create a reference to the InstantiateObjects script
    public InstantiateObjects instantiateObjects;

    //TODO: MAS: Create a reference to the GameObject that will store the runtime objects
    public GameObject RuntimeObjectStorage; //Notes: Example 2 of linking an object in the scene to the script (via editor)

    //TODO: MAS: Create a reference to the Toggle Component on the Canvas
    public Toggle ObjectMovementToggle;

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
        //TODO: MAS: Call the OnStartInitilization Method
        OnStartInitilization();
    }
    void Update()
    {
        //TODO: MAS: Call the movement toggle in the update method
        ToggleObjectMovement(ObjectMovementToggle);
    }

    /////////////////////////////// Initilization and Set up Actions //////////////////////////////

    private void OnStartInitilization()
    {
        //TODO: MAS: Find Other Script References 
        databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>(); //Notes: Example 1 of finding and setting a refrence to another object in the scene (searching)
        instantiateObjects = GameObject.Find("Instantiate").GetComponent<InstantiateObjects>();

        //TODO: MAS: On Start Find the toggle component on the canvas
        ObjectMovementToggle = GameObject.Find("ObjectMovementToggle").GetComponent<Toggle>(); //Notes: Example of finding a specific component on an object
    }

    /////////////////////////////////// UI Button and Toggle Actions /////////////////////////////

    //MAS METHODS
    //TODO: MAS: Write a method that we can link to a button for publishing to the firebase
    public void FetchDataButtonMethod()
    {
        Debug.Log("FetchDataButtonMethod: Fetch Data Button Pressed");

        //Call the Database Manager Method and Reference to fetch the data
        databaseManager.FetchRealTimeDatabaseData(databaseManager.dbreferenceMyFramesData);

    }
    
    //TODO: MAS: Write a method that we can link to a button for publishing to the firebase
    public void PublishDataButtonMethod()
    {
        //Utilize instantiate Objects method to create a rhino compatible dictionary for objects
        Dictionary<string, Frame> temporaryFrameDict = instantiateObjects.ConvertParentChildrenToRhinoFrameData(instantiateObjects.RuntimeObjectStorageObject, databaseManager.MyFramesDataDict);

        //Call the Database Manager Method and Reference to publish the data
        databaseManager.PushStringData(databaseManager.dbreferenceMyFramesData, JsonConvert.SerializeObject(temporaryFrameDict));
    }

    //TODO: MAS: Write a method that will reset everything (destroy game objects, and clear the dictionary)
    public void ResetDataButtonMethod()
    {
        Debug.Log("ResetDataButtonMethod: Reset Data Button Pressed");

        //Call the Database Manager Method and Reference to reset the data
        databaseManager.MyFramesDataDict.Clear();

        //Destroy all child objects of the runtime object storage object
        instantiateObjects.DestroyAllChildren(instantiateObjects.RuntimeObjectStorageObject);
    }

    //TODO: MAS: Write a method that will toggle on and off movement for the objects (tip: design it for the update method)
    public void ToggleObjectMovement(Toggle toggle)
    {
        if(toggle.isOn)
        {
            Debug.Log("ToggleObjectMovement: Objects should be moving now.");

            //Call Method from instantiateObjects to start moving objects
            instantiateObjects.MoveAllChildrenByVector(instantiateObjects.RuntimeObjectStorageObject);
        }
        else
        {
            Debug.Log("ToggleObjectMovement: Object Movement is now off.");
        }
    }

}

