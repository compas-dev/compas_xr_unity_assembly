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
using UnityEngine.XR.ARSubsystems;

public class UIFunctionalities : MonoBehaviour
{
    //Other Scripts for inuse objects
    public DatabaseManager databaseManager;
    public InstantiateObjects instantiateObjects;
    
    //Primary UI Objects
    public GameObject NextGeometryButtonObject;
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

    //In script use variables
    public string CurrentStep;
    
    //TODO: This could have a better name... it is actually not last written step but Previous step to current step.
    public string LastWrittenStep = "0";
    public bool NextButtonPressed = false;
    public bool PreviousButtonClicked = false;

    //On Screen Text
    public TMPro.TMP_Text CurrentStepText;

    //Touch Input Variables
    private ARRaycastManager rayManager;
    
    void Start()
    {
        //Find Initial Objects and other sctipts
        OnAwakeInitilization();
    }

    void Update()
    {
        
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
                
                //Set color of toggle
                SetUIObjectColor(EditorToggleObject, Yellow);

            }
            else
            {
                //Set Visibility of buttons
                EditorBackground.SetActive(false);
                BuilderEditorButtonObject.SetActive(false);
                BuildStatusButtonObject.SetActive(false);

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
    
    //TODO: THIS COULD BE A SIMPLE BUT HELPFUL UPDATE... I is an input, and on initilization the input is zero, but inside of button functions input is much higher.
    public void FindCurrentStep(bool writeCurrentStep)
    {
        //ITERATE THROUGH THE BUILDING PLAN DATA DICT IN ORDER.
        for (int i =0 ; i < databaseManager.BuildingPlanDataDict.Count; i++)
        {
            //Set data items
            Step step = databaseManager.BuildingPlanDataDict[i.ToString()];

            //Find the first unbuilt element
            if(step.data.is_built == false)
            {
                //Set Current Element
                SetCurrentStep(i.ToString(), writeCurrentStep);

                break;
            }
        }
    }
    public void OnCurrentStepChanged(string newCurrentStep)
    {
        //Find correct information for current object on my device
        Step step = databaseManager.BuildingPlanDataDict[CurrentStep];
        GameObject currentElement = Elements.FindObject(CurrentStep);

        //Color current step on this phone as built or unbuilt
        instantiateObjects.ColorBuiltOrUnbuilt(step.data.is_built, currentElement.FindObject("Geometry"));

        //Set Current Step
        SetCurrentStep(newCurrentStep, false);
    }
    public void NextElementButton()
    {
        Debug.Log("Next Element Button Pressed");
        
        //Set Next Button Pressed to true
        NextButtonPressed = true;

        for (int i =0 ; i < databaseManager.BuildingPlanDataDict.Count; i++)
        {
            //Set data items
            Step step = databaseManager.BuildingPlanDataDict[i.ToString()];

            //Find Gameobject
            GameObject element = Elements.FindObject(i.ToString());

            if(element != null)
            {
                //ONLY WAY TO FIX IF SOMEONE CHANGES THE ONE YOU ARE WORKING ON.
                if(step.data.is_built == true)
                {
                    //Color Previous step object as built or unbuilt
                    instantiateObjects.ColorBuiltOrUnbuilt(step.data.is_built, element.FindObject("Geometry"));
                }

                //Find the first unbuilt element
                else
                {
                    // Set First Found Element as Current Step
                    step.data.is_built = true;

                    //Color Previous step object as built or unbuilt
                    instantiateObjects.ColorBuiltOrUnbuilt(step.data.is_built, element.FindObject("Geometry"));

                    //WRITE INFORMATION TO DATABASE...HAS TO STAY HERE
                    databaseManager.PushAllData(databaseManager.dbreference_buildingplan.Child(i.ToString()), JsonConvert.SerializeObject(databaseManager.BuildingPlanDataDict[i.ToString()]));
                    
                    //Set current element as this step + 1
                    SetCurrentStep((i + 1).ToString(), true);

                    //Find current step loop was run to avoid async if someone builds one in front of me...
                    //This could be improved though... I could write a function that starts at a point and goes to the end of the dictionary looking for the first false... Then sets that as my current element.
                    // FindCurrentStep(false);
                    
                    break;
                }
            }

        }

    }
    public void SetCurrentStep(string key, bool write)
    {
        //Set current element name
        CurrentStep = key;

        //Find the step in the dictoinary
        Step step = databaseManager.BuildingPlanDataDict[key];

        //Find Gameobject Associated with that step
        GameObject element = Elements.FindObject(key);

        if(element != null)
        {
            //Color it Human or Robot Built
            instantiateObjects.ColorHumanOrRobot(step.data.actor, step.data.is_built, element.FindObject("Geometry"));
            Debug.Log($"Current Step is {CurrentStep}");
        }

        //Set Last written step
        if (key != "0")
        {
            LastWrittenStep = (int.Parse(key) - 1).ToString();
        }
        
        //Update Onscreen Text
        CurrentStepText.text = CurrentStep;
        
        //Bool to control writing to the database
        if(write)
        {
            //Push Current key to the firebase
            databaseManager.PushAllData(databaseManager.dbrefernece_currentstep, CurrentStep);
        }
        
    }
    public void PreviousElementButton()
    {
        if(LastWrittenStep != null)
        {
            //Set Previous Button Pressed to true
            PreviousButtonClicked = true;
            
            //Previous element button clicked
            Debug.Log("Previous Element Button Pressed");
            
            //Find Gameobject
            GameObject element = Elements.FindObject(LastWrittenStep);
            Debug.Log($"Last Written Step is {LastWrittenStep}");
            GameObject previouselement = Elements.FindObject(CurrentStep);
            Debug.Log($"Current Step is {CurrentStep}");

            if(element != null && previouselement != null)
            {
                Debug.Log("Entered loop");
                //Set the element to unbuilt
                Step step = databaseManager.BuildingPlanDataDict[LastWrittenStep];
                step.data.is_built = false;

                //PreviousStep Data
                Step previousstep = databaseManager.BuildingPlanDataDict[CurrentStep];

                //Color Previous step object as built or unbuilt
                instantiateObjects.ColorBuiltOrUnbuilt(previousstep.data.is_built, previouselement);

                //Push to the database
                databaseManager.PushAllData(databaseManager.dbreference_buildingplan.Child(LastWrittenStep), JsonConvert.SerializeObject(databaseManager.BuildingPlanDataDict[LastWrittenStep]));

                //Set the current element to the last written element
                SetCurrentStep(LastWrittenStep, true);
            }
        }
    }

    //TODO: Is there a better way to update this then calling it in every time a button is pressed?
    public void PreviewGeometrySlider(float value)
    {
        if (CurrentStep != null)
        {
            Debug.Log("You are changing the Preview Geometry");
            int min = Convert.ToInt16(CurrentStep);
            float SliderValue = value;
            int ElementsTotal = databaseManager.BuildingPlanDataDict.Count;
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
    
    private void ColliderControler()
    {
        //Set data items
        Step Currentstep = databaseManager.BuildingPlanDataDict[CurrentStep];
    
        for (int i =0 ; i < databaseManager.BuildingPlanDataDict.Count; i++)
        {
            //Set data items
            Step step = databaseManager.BuildingPlanDataDict[i.ToString()];

            //Find Gameobject
            Collider ElementCollider = Elements.FindObject(i.ToString()).FindObject("Geometry").GetComponent<Collider>();

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
                }
            }
            
        }
    }

    // private GameObject SelectedObject(GameObject activeGameObject = null)
    // {
    //     if (Application.isEditor)
    //     {
    //         Ray ray = arCamera.ScreenPointToRay(Input.mousePosition);
    //         RaycastHit hitObject;

    //         Debug.Log("Looking for your Joint To Select");

    //         if (Physics.Raycast(ray, out hitObject))
    //         {
    //             if (hitObject.collider.tag != "plane")
    //             {
    //                 activeGameObject = hitObject.collider.gameObject;
    //                 Debug.Log(activeGameObject);
    //             }
    //         }
    //     }
    //     else
    //     {
    //         Touch touch = Input.GetTouch(0);
    //         Debug.Log("Your touch sir :)" + Input.touchCount);
    //         Debug.Log("Your Phase sir :)" + (touch.phase == TouchPhase.Ended));

    //         if (Input.touchCount == 1 && touch.phase == TouchPhase.Ended)
    //         {
    //             List<ARRaycastHit> hits = new List<ARRaycastHit>();
    //             Debug.Log ("YOU HITS SIR" + hits);
    //             rayManager.Raycast(touch.position, hits);

    //             if (hits.Count > 0)
    //             {
    //                 Ray ray = arCamera.ScreenPointToRay(touch.position);
    //                 RaycastHit hitObject;

    //                 if (Physics.Raycast(ray, out hitObject))
    //                 {
    //                     if (hitObject.collider.tag != "plane")
    //                     {
    //                         activeGameObject = hitObject.collider.gameObject;
    //                         Debug.Log(activeGameObject);
    //                     }
    //                 }
    //             }
    //         }
    //     }

    //     return activeGameObject;
    // }

}

