using System.Collections;
using System.Collections.Generic;
using Extentions;
using UnityEngine;
using UnityEngine.UI;
using Instantiate;
using JSON;
using Newtonsoft.Json;

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
    public string LastWrittenStep = "0";
    public bool NextButtonPressed = false;

    
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
        // PreviousGeometrySlider.onValueChanged.AddListener(() => print_string_on_click("Previous Object Button Clicked"));;


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

        //TODO: FIND ON SCREEN TEXT OBJECTS
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
                
                //Set Last Written Element only if it is not == 0
                if(i.ToString() != "0")
                {
                    //Set Last Written Element
                    LastWrittenStep = (i- 1).ToString();
                    Debug.Log($"Last Written Step is {LastWrittenStep}");
                }

                break;
            }
        }
    }
    
    //TODO: ADD BOOL...
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

                    //SET LAST WRITTEN ELEMENT
                    LastWrittenStep = i.ToString();

                    //WRITE INFORMATION TO DATABASE...HAS TO STAY HERE
                    databaseManager.PushAllData(databaseManager.dbreference_buildingplan.Child(i.ToString()), JsonConvert.SerializeObject(databaseManager.BuildingPlanDataDict[i.ToString()]));
                    
                    //TODO: THIS SHOULD WORK BY ADDING 1
                    FindCurrentStep(false);
                    
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

        //Update Onscreen Text
        //.......
        
        //Bool to control writing to the database
        if(write)
        {
            //Push Current key to the firebase
            databaseManager.PushAllData(databaseManager.dbrefernece_currentstep, CurrentStep);
        }
        
    }
    
    //TODO: GETS STUCK AND I AM NOT SURE WHY. JUST CHECK IT.
    public void PreviousElementButton()
    {
        if(LastWrittenStep != null)
        {
            //Find Gameobject
            GameObject element = Elements.FindObject(LastWrittenStep);
            GameObject previouselement = Elements.FindObject(CurrentStep);

            if(element != null && previouselement != null)
            {
                //Set the element to unbuilt
                Step step = databaseManager.BuildingPlanDataDict[LastWrittenStep];
                step.data.is_built = false;

                //PreviousStep Data
                Step previousstep = databaseManager.BuildingPlanDataDict[CurrentStep];

                //Push to the database
                databaseManager.PushAllData(databaseManager.dbreference_buildingplan.Child(LastWrittenStep), JsonConvert.SerializeObject(databaseManager.BuildingPlanDataDict[LastWrittenStep]));

                //Color Previous step object as built or unbuilt
                instantiateObjects.ColorBuiltOrUnbuilt(previousstep.data.is_built, previouselement);

                //Set the current element to the last written element
                SetCurrentStep(LastWrittenStep, true);
            }
        }
    }

    
    ////////////////////////////////////// Visualizer Menu Buttons ////////////////////////////////////////////
    

    ////////////////////////////////////////// Menu Buttons ///////////////////////////////////////////////////
    

    ////////////////////////////////////////// Editor Buttons /////////////////////////////////////////////////


}

