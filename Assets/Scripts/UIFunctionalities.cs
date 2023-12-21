using System.Collections;
using System.Collections.Generic;
using Extentions;
using UnityEngine;
using UnityEngine.UI;
using Instantiate;
using JSON;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Xml.Linq;
using UnityEngine.XR.ARFoundation;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARSubsystems;


public class UIFunctionalities : MonoBehaviour
{
    //Other Scripts for inuse objects
    public DatabaseManager databaseManager;
    public InstantiateObjects instantiateObjects;
    
    //Primary UI Objects
    public GameObject NextGeometryButtonObject;

    //////////////////////////////////////////////////////////////////////////
    public GameObject PushAllDataTEST;

    public string TESTKey;
    //////////////////////////////////////////////////////////////////////////

    public GameObject PreviousGeometryButtonObject;
    public GameObject PreviewGeometrySliderObject;
        
    //Toggle GameObjects
    private GameObject VisibilityMenuObject;
    private GameObject MenuButtonObject;
    private GameObject EditorToggleObject;

    //Visualizer Menu Toggle Objects
    private GameObject VisualzierBackground;
    private GameObject PreviewBuilderButtonObject;
    private GameObject IDButtonObject;
    private GameObject RobotButtonObject;
    private GameObject ObjectLengthsButtonObject;

    //Menu Toggle Objects
    private GameObject MenuBackground;
    private GameObject InfoButtonObject;
    private GameObject ReloadButtonObject;
    private GameObject CommunicationButtonObject;

    //Editor Toggle Objects
    private GameObject EditorBackground;
    private GameObject BuilderEditorButtonObject;
    private GameObject BuildStatusButtonObject;
    
    //Object Colors
    private Color Yellow = new Color(1.0f, 1.0f, 0.0f, 1.0f);
    private Color White = new Color(1.0f, 1.0f, 1.0f, 1.0f);

    //Parent Objects for gameObjects
    public GameObject Elements;
    public GameObject QRMarkers;

    //AR Camera and Touch GameObjects
    public Camera arCamera;
    private GameObject activeGameObject;
    private GameObject temporaryObject;
    private int mode = 0;
    
    public bool NextButtonPressed = false;
    public bool PreviousButtonClicked = false;

    //On Screen Text
    public TMPro.TMP_Text CurrentStepText;

    //Touch Input Variables
    private ARRaycastManager rayManager;

    //In script use variables
    public string CurrentStep;
    
    void Start()
    {
        //Find Initial Objects and other sctipts
        OnAwakeInitilization();
    }

    void Update()
    {
        SearchControler();
    }

    /////////////////////////////////////////// UI Control ////////////////////////////////////////////////////
    private void OnAwakeInitilization()
    {
        /////////////////////////////////////////// Initial Elements ////////////////////////////////////////////
        //Find Other Scripts
        databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
        instantiateObjects = GameObject.Find("Instantiate").GetComponent<InstantiateObjects>();

        //Find Specific GameObjects
        Elements = GameObject.Find("Elements");
        QRMarkers = GameObject.Find("QRMarkers");
        
        //Find toggles for visibility
        VisibilityMenuObject = GameObject.Find("Visibility_Editor");
        Toggle VisibilityMenuToggle = VisibilityMenuObject.GetComponent<Toggle>();
        
        //Find toggles for menu
        MenuButtonObject = GameObject.Find("Menu_Toggle");
        Toggle MenuToggle = MenuButtonObject.GetComponent<Toggle>();

        //Find toggle for editor... Slightly different then the other two because it is off on start.
        EditorToggleObject = MenuButtonObject.FindObject("Editor_Toggle");
        Toggle EditorToggle = EditorToggleObject.GetComponent<Toggle>();

        //Find Background Images for Toggles
        VisualzierBackground = VisibilityMenuObject.FindObject("Background_Visualizer");
        MenuBackground = MenuButtonObject.FindObject("Background_Menu");
        EditorBackground = EditorToggleObject.FindObject("Background_Editor");

        //Find AR Camera gameObject
        arCamera = GameObject.Find("XR Origin").FindObject("Camera Offset").FindObject("Main Camera").GetComponent<Camera>();

        /////////////////////////////////////////// Visualizer Menu Buttons ////////////////////////////////////////////
        //Find Object, Button, and Add Listener for OnClick method
        PreviewBuilderButtonObject = VisibilityMenuObject.FindObject("Preview_Builder");
        Button PreviewBuilderButton = PreviewBuilderButtonObject.GetComponent<Button>();
        PreviewBuilderButton.onClick.AddListener(() => print_string_on_click("Preview Builder Clicked"));;

        //Find Object, Button, and Add Listener for OnClick method
        IDButtonObject = VisibilityMenuObject.FindObject("ID_Button");
        Button IDButton = IDButtonObject.GetComponent<Button>();
        IDButton.onClick.AddListener(() => print_string_on_click("ID Button Clicked"));;

        //Find Object, Button, and Add Listener for OnClick method
        RobotButtonObject = VisibilityMenuObject.FindObject("Robot_Button");
        Button RobotButton = RobotButtonObject.GetComponent<Button>();
        RobotButton.onClick.AddListener(() => print_string_on_click("Robot Button Clicked"));;

        //Find Object, Button, and Add Listener for OnClick method
        ObjectLengthsButtonObject = VisibilityMenuObject.FindObject("ObjectLength_Button");
        Button ObjectLengthsButton = ObjectLengthsButtonObject.GetComponent<Button>();
        ObjectLengthsButton.onClick.AddListener(() => print_string_on_click("Object Lengths Button Clicked"));;
    
       
        /////////////////////////////////////////// Menu Buttons ////////////////////////////////////////////
        //Find Object, Button, and Add Listener for OnClick method
        InfoButtonObject = MenuButtonObject.FindObject("Info_Button");
        Button InfoButton = InfoButtonObject.GetComponent<Button>();
        InfoButton.onClick.AddListener(() => print_string_on_click("Info Button Clicked"));;

        //Find Object, Button, and Add Listener for OnClick method
        ReloadButtonObject = MenuButtonObject.FindObject("Reload_Button");
        Button ReloadButton = ReloadButtonObject.GetComponent<Button>();
        ReloadButton.onClick.AddListener(() => print_string_on_click("Reload Button Clicked"));;

        //Find Object, Button, and Add Listener for OnClick method
        CommunicationButtonObject = MenuButtonObject.FindObject("Communication_Button");
        Button CommunicationButton = CommunicationButtonObject.GetComponent<Button>();
        CommunicationButton.onClick.AddListener(() => print_string_on_click("Communication Button Clicked"));;

        //Find Object, Button, and Add Listener for OnClick method
        BuilderEditorButtonObject = EditorToggleObject.FindObject("Builder_Editor_Button");
        Button BuilderEditorButton = BuilderEditorButtonObject.GetComponent<Button>();
        BuilderEditorButton.onClick.AddListener(() => print_string_on_click("Builder Editor Button Clicked"));;

        //Find Object, Button, and Add Listener for OnClick method
        BuildStatusButtonObject = EditorToggleObject.FindObject("Build_Status_Editor");
        Button BuildStatusButton = BuildStatusButtonObject.GetComponent<Button>();
        BuildStatusButton.onClick.AddListener(() => print_string_on_click("Build Status Editor Button Clicked"));;


        /////////////////////////////////////////// Primary UI Buttons ////////////////////////////////////////////
        //Find Object, Button, and Add Listener for OnClick method
        NextGeometryButtonObject = GameObject.Find("Next_Geometry");
        Button NextGeometryButton = NextGeometryButtonObject.GetComponent<Button>();
        NextGeometryButton.onClick.AddListener(NextElementButton);;

        //////////////////////////////////////////////////////////////////////////
        //Find Object, Button, and Add Listener for OnClick method
        PushAllDataTEST = GameObject.Find("PushAllData");
        Button PushAllDataTESTButton = PushAllDataTEST.GetComponent<Button>();
        PushAllDataTESTButton.onClick.AddListener(PushAllDataTESTFunction);;
        //////////////////////////////////////////////////////////////////////////

        //Find Object, Button, and Add Listener for OnClick method
        PreviousGeometryButtonObject = GameObject.Find("Previous_Geometry");
        Button PreviousGeometryButton = PreviousGeometryButtonObject.GetComponent<Button>();
        PreviousGeometryButton.onClick.AddListener(PreviousElementButton);;

        //Find Object, Button, and Add Listener for OnClick method
        PreviewGeometrySliderObject = GameObject.Find("GeometrySlider");
        Slider PreviousGeometrySlider = PreviewGeometrySliderObject.GetComponent<Slider>();
        PreviousGeometrySlider.onValueChanged.AddListener(PreviewGeometrySlider);;

        //Find Text Objects
        GameObject CurrentStepTextObject = GameObject.Find("Current_Element_Text");
        CurrentStepText = CurrentStepTextObject.GetComponent<TMPro.TMP_Text>();


        /////////////////////////////////////////// Set Toggles ////////////////////////////////////////////
        //Add Listners for Visibility Toggle on and off.
        VisibilityMenuToggle.onValueChanged.AddListener(delegate {
        ToggleVisibilityMenu(VisibilityMenuToggle);
        });

        //Add Listners for Menu Toggle on and off.
        MenuToggle.onValueChanged.AddListener(delegate {
        ToggleMenu(MenuToggle);
        });

        //Add Listners for Editor Toggle on and off.
        EditorToggle.onValueChanged.AddListener(delegate {
        ToggleEditor(EditorToggle);
        });

    }
    public void ToggleVisibilityMenu(Toggle toggle)
    {
        if (VisualzierBackground != null && PreviewBuilderButtonObject != null && RobotButtonObject != null && ObjectLengthsButtonObject != null && IDButtonObject != null)
        {    
            if (toggle.isOn)
            {             
                //Set Visibility of buttons
                VisualzierBackground.SetActive(true);
                PreviewBuilderButtonObject.SetActive(true);
                RobotButtonObject.SetActive(true);
                ObjectLengthsButtonObject.SetActive(true);
                IDButtonObject.SetActive(true);

                //Set color of toggle
                SetUIObjectColor(VisibilityMenuObject, Yellow);

            }
            else
            {
                //Set Visibility of buttons
                VisualzierBackground.SetActive(false);
                PreviewBuilderButtonObject.SetActive(false);
                RobotButtonObject.SetActive(false);
                ObjectLengthsButtonObject.SetActive(false);
                IDButtonObject.SetActive(false);

                //Set color of toggle
                SetUIObjectColor(VisibilityMenuObject, White);
            }
        }
        else
        {
            Debug.LogWarning("Could not find one of the buttons in the Visualizer Menu.");
        }   
    }
    public void ToggleMenu(Toggle toggle)
    {
        if (MenuBackground != null && InfoButtonObject != null && ReloadButtonObject != null && CommunicationButtonObject != null && EditorToggleObject != null)
        {    
            if (toggle.isOn)
            {             
                //Set Visibility of buttons
                MenuBackground.SetActive(true);
                InfoButtonObject.SetActive(true);
                ReloadButtonObject.SetActive(true);
                CommunicationButtonObject.SetActive(true);
                EditorToggleObject.SetActive(true);

                //Set color of toggle
                SetUIObjectColor(MenuButtonObject, Yellow);

            }
            else
            {
                //Set Visibility of buttons
                MenuBackground.SetActive(false);
                InfoButtonObject.SetActive(false);
                ReloadButtonObject.SetActive(false);
                CommunicationButtonObject.SetActive(false);
                EditorToggleObject.SetActive(false);

                //TODO: Add mode switch for if editor touch mode is on or off. Big improvement from last time where you had to close both in sequence.

                //Set color of toggle
                SetUIObjectColor(MenuButtonObject, White);
            }
        }
        else
        {
            Debug.LogWarning("Could not find one of the buttons in the Menu.");
        }   
    }
    public void ToggleEditor(Toggle toggle)
    {
        if (EditorBackground != null && BuilderEditorButtonObject != null && BuildStatusButtonObject != null)
        {    
            if (toggle.isOn)
            {             
                //Set Visibility of buttons
                EditorBackground.SetActive(true);
                BuilderEditorButtonObject.SetActive(true);
                BuildStatusButtonObject.SetActive(true);

                //Control Selectable objects in scene
                ColliderControler();

                //Update mode so we know to search for touch input
                TouchSearchModeControler(1);
                
                //Set color of toggle
                SetUIObjectColor(EditorToggleObject, Yellow);

            }
            else
            {
                //Set Visibility of buttons
                EditorBackground.SetActive(false);
                BuilderEditorButtonObject.SetActive(false);
                BuildStatusButtonObject.SetActive(false);

                //Update mode so we are no longer searching for touch
                TouchSearchModeControler(0);

                //Color Elements by build status
                instantiateObjects.ApplyColorBasedOnBuildState();

                //Set color of toggle
                SetUIObjectColor(EditorToggleObject, White);
            }
        }
        else
        {
            Debug.LogWarning("Could not find one of the buttons in the Editor Menu.");
        }  
    }
    private void SetUIObjectColor(GameObject Button, Color color)
    {
        Button.GetComponent<Image>().color = color;
    }
    public void print_string_on_click(string Text)
    {
        Debug.Log(Text);
    }

    /////////////////////////////////////// Primary UI Functions //////////////////////////////////////////////
    public void NextElementButton()
    {
        Debug.Log("Next Element Button Pressed");
        
        // for (int i =0 ; i < databaseManager.BuildingPlanDataDict.Count; i++)
        // {
        //     //Set data items
        //     Step step = databaseManager.BuildingPlanDataDict[i.ToString()];

        //     //Find Gameobject
        //     GameObject element = Elements.FindObject(i.ToString());

        //     if(element != null)
        //     {
        //         //ONLY WAY TO FIX IF SOMEONE CHANGES THE ONE YOU ARE WORKING ON.
        //         if(step.data.is_built == true)
        //         {
        //             //Color Previous step object as built or unbuilt
        //             instantiateObjects.ColorBuiltOrUnbuilt(step.data.is_built, element.FindObject("Geometry"));
        //         }

        //         //Find the first unbuilt element
        //         else
        //         {
        //             // Set First Found Element as Current Step
        //             step.data.is_built = true;

        //             //Color Previous step object as built or unbuilt
        //             instantiateObjects.ColorBuiltOrUnbuilt(step.data.is_built, element.FindObject("Geometry"));

        //             //WRITE INFORMATION TO DATABASE...HAS TO STAY HERE
        //             // databaseManager.PushAllData(databaseManager.dbreference_buildingplan.Child(i.ToString()), JsonConvert.SerializeObject(databaseManager.BuildingPlanDataDict[i.ToString()]));
                    
        //             //Set current element as this step + 1
        //             SetCurrentStep((i + 1).ToString(), true);

        //             //Find current step loop was run to avoid async if someone builds one in front of me...
        //             //This could be improved though... I could write a function that starts at a point and goes to the end of the dictionary looking for the first false... Then sets that as my current element.
        //             // FindCurrentStep(false);
                    
        //             break;
        //         }
        //     }

        // }

    }

    public void PushAllDataTESTFunction()
    {
        Debug.Log($"I am pushing all data to the database for test key {TESTKey}");
        Debug.Log($"LastBuiltIndex is {databaseManager.BuildingPlanDataItem.LastBuiltIndex}");

        databaseManager.PushAllDataBuildingPlan(TESTKey);

    }

    public void SetCurrentStep(string key, bool write)
    {
        //Set current element name
        CurrentStep = key;

        //Find the step in the dictoinary
        Step step = databaseManager.BuildingPlanDataItem.steps[key];

        //Find Gameobject Associated with that step
        GameObject element = Elements.FindObject(key);

        if(element != null)
        {
            //Color it Human or Robot Built
            instantiateObjects.ColorHumanOrRobot(step.data.actor, step.data.is_built, element.FindObject("Geometry"));
            Debug.Log($"Current Step is {CurrentStep}");
        }
        
        //Update Onscreen Text
        CurrentStepText.text = CurrentStep;
        
        //Bool to control writing to the database
        if(write)
        {
            //Push Current key to the firebase
            databaseManager.PushStringData(databaseManager.dbrefernece_currentstep, CurrentStep);
        }
        
    }
    public void PreviousElementButton()
    {
        // if(LastWrittenStep != null)
        // {
        //     //Previous element button clicked
            Debug.Log("Previous Element Button Pressed");
            
        //     //Find Gameobject
        //     GameObject element = Elements.FindObject(LastWrittenStep);
        //     Debug.Log($"Last Written Step is {LastWrittenStep}");
        //     GameObject previouselement = Elements.FindObject(CurrentStep);
        //     Debug.Log($"Current Step is {CurrentStep}");

        //     if(element != null && previouselement != null)
        //     {
        //         Debug.Log("Entered loop");
        //         //Set the element to unbuilt
        //         Step step = databaseManager.BuildingPlanDataDict[LastWrittenStep];
        //         step.data.is_built = false;

        //         //PreviousStep Data
        //         Step previousstep = databaseManager.BuildingPlanDataDict[CurrentStep];

        //         //Color Previous step object as built or unbuilt
        //         instantiateObjects.ColorBuiltOrUnbuilt(previousstep.data.is_built, previouselement);

        //         //Push to the database
        //         // databaseManager.PushAllData(databaseManager.dbreference_buildingplan.Child(LastWrittenStep), JsonConvert.SerializeObject(databaseManager.BuildingPlanDataDict[LastWrittenStep]));

        //         //Set the current element to the last written element
        //         SetCurrentStep(LastWrittenStep, true);
        //     }
        // }
    }
    public void PreviewGeometrySlider(float value)
    {
        if (CurrentStep != null)
        {
            Debug.Log("You are changing the Preview Geometry");
            int min = Convert.ToInt16(CurrentStep);
            float SliderValue = value;
            int ElementsTotal = databaseManager.BuildingPlanDataItem.steps.Count;
            float SliderMax = 1; //Input Slider Max Value == 1
            float SliderMin = 0; // Input Slider Min Value == 0
                
            float SliderRemaped = GameObjectExtensions.Remap(SliderValue, SliderMin, SliderMax, min, ElementsTotal); 

            foreach(int index in Enumerable.Range(min, ElementsTotal))
            {
                string elementName = index.ToString();
                int InstanceNumber = Convert.ToInt16(elementName);

                GameObject element = Elements.FindObject(elementName);
                
                if (element != null)
                {
                    if (InstanceNumber > SliderRemaped)
                    {
                        element.SetActive(false); 
                    }
                    else
                    {
                        element.SetActive(true);
                    }
                }
            }
        }
    }
    
    ////////////////////////////////////// Visualizer Menu Buttons ////////////////////////////////////////////
    

    ////////////////////////////////////////// Menu Buttons ///////////////////////////////////////////////////
    

    ////////////////////////////////////////// Editor Buttons /////////////////////////////////////////////////
    public void TouchSearchModeControler(int modetype)
    {
        if (modetype == 1)
            {        
                //Enable Editor Selected Stick text and disable current stick text 
                //.... Previously controlled as sepert text objects

                mode = 1; // for editing existing objects
                Debug.Log ("You have set Mode 1: Element Search");
            }

        else
            {
               //Enable Editor Selected Stick text and disable current stick text 
                //.... Previously controlled as sepert text objects

                mode = 0; // setting back to original mode

                //Destroy active bounding box
                DestroyBoundingBoxFixElementColor();
                activeGameObject = null;
                Debug.Log ("You have set Mode 0");
            }
    }
    private void SearchControler()
    {
        if (mode == 1)
        {
            SearchInput();
        }
    }
    private void ColliderControler()
    {
        //Set data items
        Step Currentstep = databaseManager.BuildingPlanDataItem.steps[CurrentStep];
    
        for (int i =0 ; i < databaseManager.BuildingPlanDataItem.steps.Count; i++)
        {
            //Set data items
            Step step = databaseManager.BuildingPlanDataItem.steps[i.ToString()];

            //Find Gameobject Collider and Renderer
            GameObject element = Elements.FindObject(i.ToString()).FindObject("Geometry");
            Collider ElementCollider = element.FindObject("Geometry").GetComponent<Collider>();
            Renderer ElementRenderer = element.FindObject("Geometry").GetComponent<Renderer>();

            if(ElementCollider != null)
            {
                //Find the first unbuilt element
                if(step.data.priority == Currentstep.data.priority)
                {
                    //Set Collider to true
                    ElementCollider.enabled = true;
                }
                else
                {
                    //Set Collider to false
                    ElementCollider.enabled = false;

                    //Update Renderer for Objects that cannot be selected
                    ElementRenderer.material = instantiateObjects.LockedObjectMaterial;
                }
            }
            
        }
    }
    private GameObject SelectedObject(GameObject activeGameObject = null)
    {
        if (Application.isEditor)
        {
            Ray ray = arCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitObject;

            if (Physics.Raycast(ray, out hitObject))
            {
                if (hitObject.collider.tag != "plane")
                {
                    activeGameObject = hitObject.collider.gameObject;
                    Debug.Log(activeGameObject);
                }
            }
        }
        else
        {
            Touch touch = Input.GetTouch(0);

            if (Input.touchCount == 1 && touch.phase == TouchPhase.Ended)
            {
                List<ARRaycastHit> hits = new List<ARRaycastHit>();
                rayManager.Raycast(touch.position, hits);

                if (hits.Count > 0)
                {
                    Ray ray = arCamera.ScreenPointToRay(touch.position);
                    RaycastHit hitObject;

                    if (Physics.Raycast(ray, out hitObject))
                    {
                        if (hitObject.collider.tag != "plane")
                        {
                            activeGameObject = hitObject.collider.gameObject;
                            Debug.Log(activeGameObject);
                        }
                    }
                }
            }
        }

        return activeGameObject;
    }
    private void SearchInput()
    {
        if (Application.isEditor)
        {   
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                if (mode == 1) // EDIT MODE
                {
                    Debug.Log("***MODE 2***");
                    EditMode();
                }

                else
                {
                    Debug.Log("Press a button to initialize a mode");
                }
            }
        }
        else
        {
            SearchTouch();
        }
    }
    private void SearchTouch()
    {
        if (Input.touchCount > 0) //if there is an input..           
        {
            if (PhysicRayCastBlockedByUi(Input.GetTouch(0).position))
            {
                if (mode == 1) //EDIT MODE
                {
                    Debug.Log("***MODE 2***");
                    EditMode();                     
                }

                else
                {
                    Debug.Log("Press a button to initialize a mode");
                }
            }
        }
    }
    private bool PhysicRayCastBlockedByUi(Vector2 touchPosition)
    {
        //creating a Boolean value if we are touching a button
        if (GameObjectExtensions.IsPointerOverUIObject(touchPosition))
        {
            Debug.Log("YOU CANT FIND YOUR OBJECT INSIDE PHYSICRAYCAST...");
            return false;
        }
        return true;
    }
    private void EditMode()
    {
        activeGameObject = SelectedObject();
        
        if (Input.touchCount == 1) //try and locate the selected object only when we click, not on update
        {
            activeGameObject = SelectedObject();
        }

        if (activeGameObject != null)
        {
            // _EditorCurrentStickText(activeGameObject);
            temporaryObject = activeGameObject;
            //TODO: Color the object the correct color

            addBoundingBox(temporaryObject);
        }

        else
        {
            if (GameObject.Find("BoundingArea") != null)
            {
                DestroyBoundingBoxFixElementColor();
            }
        }
    }
    private void addBoundingBox(GameObject gameObj) // Still need to fix it is so weird.
    {
        DestroyBoundingBoxFixElementColor(); //destroy the bounding box

        //create a primitive cube
        GameObject boundingArea = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        //add name
        boundingArea.name = "BoundingArea";

        //add material
        boundingArea.GetComponent<Renderer>().material = instantiateObjects.HumanUnbuiltMaterial;


        //use collider to find object bounds
        Collider collider = gameObj.GetComponent<Collider>();
        Vector3 center = collider.bounds.center;
        float radius = collider.bounds.extents.magnitude;
        Debug.Log("RADIUS BOUNDING BOX = " + radius);

        // destroy any Collider component
        if (boundingArea.GetComponent<Rigidbody>() != null)
        {
            Destroy(boundingArea.GetComponent<BoxCollider>());
        }
        if (boundingArea.GetComponent<Collider>() != null)
        {
            Destroy(boundingArea.GetComponent<BoxCollider>());
        }

        // scale the bounding box according to the bounds values
        boundingArea.transform.localScale = new Vector3(radius * 0.5f, radius * 0.5f, radius * 0.5f);
        boundingArea.transform.localPosition = center;
        boundingArea.transform.rotation = gameObj.transform.rotation;

        //Set parent as the step Item from the dictionary
        var stepParent = gameObj.transform.parent;

        boundingArea.transform.SetParent(stepParent);
    }
    private void DestroyBoundingBoxFixElementColor()
    {

        //destroy the previous bounding box

        if (GameObject.Find("BoundingArea") != null)
        {
            GameObject Box = GameObject.Find("BoundingArea");
            var element = Box.transform.parent;

            if (element != null)
            {
                if (CurrentStep != null)
                {
                    if (element.name != CurrentStep)
                    {

                        Step step = databaseManager.BuildingPlanDataItem.steps[element.name];
                        
                        instantiateObjects.ColorBuiltOrUnbuilt(step.data.is_built, element.gameObject.FindObject("Geometry"));                          
                        
                    }
                }

            }

            Destroy(GameObject.Find("BoundingArea"));
        }

    }
}

