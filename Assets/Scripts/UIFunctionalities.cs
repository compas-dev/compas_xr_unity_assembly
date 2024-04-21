using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;
using System.Linq;
using TMPro;
using UnityEngine.XR.ARFoundation;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using CompasXR.Core;
using CompasXR.Systems;
using CompasXR.Core.Data;
using CompasXR.Core.Extentions;
using CompasXR.AppSettings;
using CompasXR.Robots;
using CompasXR.Robots.MqttData;

namespace CompasXR.UI
{
    public class UIFunctionalities : MonoBehaviour
    {
        //Other Scripts for inuse objects
        public DatabaseManager databaseManager;
        public InstantiateObjects instantiateObjects;
        public Eventmanager eventManager;
        public MqttTrajectoryManager mqttTrajectoryManager;
        public TrajectoryVisulizer trajectoryVisulizer;
        public RosConnectionManager rosConnectionManager;
        public ScrollSearchManager scrollSearchManager;

        //Primary UI Objects
        private GameObject VisibilityMenuObject;
        private GameObject MenuButtonObject;
        private GameObject EditorToggleObject;
        public GameObject CanvasObject;
        public GameObject ConstantUIPanelObjects;
        public GameObject NextGeometryButtonObject;
        public GameObject PreviousGeometryButtonObject;
        public GameObject PreviewGeometrySliderObject;
        public Slider PreviewGeometrySlider;
        public GameObject IsBuiltPanelObjects;
        public GameObject IsBuiltButtonObject;
        public GameObject IsbuiltButtonImage;
        public GameObject IsbuiltPriorityLockedImage;

        //On Screen Messages
        public GameObject MessagesParent;
        public GameObject OnScreenErrorMessagePrefab;
        public GameObject OnScreenInfoMessagePrefab;
        private GameObject PriorityIncompleteWarningMessageObject;
        private GameObject PriorityIncorrectWarningMessageObject;
        private GameObject PriorityCompleteMessageObject;
        public GameObject MQTTFailedToConnectMessageObject;
        public GameObject MQTTConnectionLostMessageObject;
        public GameObject ErrorFetchingDownloadUriMessageObject;
        public GameObject ErrorDownloadingObjectMessageObject;
        public GameObject TrajectoryReviewRequestMessageObject;
        public GameObject TrajectoryCancledMessage;
        public GameObject SearchItemNotFoundWarningMessageObject;
        public GameObject ActiveRobotIsNullWarningMessageObject;
        public GameObject TransactionLockActiveWarningMessageObject;
        public GameObject ActiveRobotCouldNotBeFoundWarningMessage;
        public GameObject ActiveRobotUpdatedFromPlannerMessageObject;
        public GameObject TrajectoryResponseIncorrectWarningMessageObject;
        public GameObject ConfigDoesNotMatchURDFStructureWarningMessageObject;
        public GameObject TrajectoryNullWarningMessageObject;

        //Visualizer Menu Objects
        private GameObject VisualzierBackground;
        private GameObject PreviewActorToggleObject;
        public GameObject IDToggleObject;
        public GameObject RobotToggleObject;
        public GameObject ObjectLengthsToggleObject;
        private GameObject ObjectLengthsUIPanelObjects;
        private Vector3 ObjectLengthsUIPanelPosition;
        private TMP_Text ObjectLengthsText;
        private GameObject ObjectLengthsTags;
        public GameObject ScrollSearchToggleObject;
        private GameObject ScrollSearchObjects;
        public GameObject PriorityViewerToggleObject;
        public GameObject NextPriorityButtonObject;
        public GameObject PreviousPriorityButtonObject;
        public GameObject PriorityViewerBackground;
        public GameObject SelectedPriorityTextObject;
        public TMP_Text SelectedPriorityText;

        //Menu Toggle Button Objects
        private GameObject MenuBackground;
        private GameObject ReloadButtonObject;
        private GameObject InfoToggleObject;
        private GameObject InfoPanelObject;
        public GameObject CommunicationToggleObject;
        private GameObject CommunicationPanelObject;

        //Editor Toggle Objects
        private GameObject EditorBackground;
        private GameObject BuilderEditorButtonObject;
        private GameObject BuildStatusButtonObject;
        
        //Communication Specific Objects
        private TMP_InputField MqttBrokerInputField;
        private TMP_InputField MqttPortInputField;
        private GameObject MqttUpdateConnectionMessage;
        public GameObject MqttConnectionStatusObject;
        public GameObject MqttConnectButtonObject;
        public GameObject RosConnectButtonObject;
        private TMP_InputField RosHostInputField;
        private TMP_InputField RosPortInputField;
        private GameObject RosUpdateConnectionMessage;
        public GameObject RosConnectionStatusObject;

        //Trajectory Review UI Controls
        public GameObject ReviewTrajectoryObjects;
        public GameObject RequestTrajectoryButtonObject;
        public GameObject ApproveTrajectoryButtonObject;
        public GameObject RejectTrajectoryButtonObject;
        public GameObject TrajectoryReviewSliderObject;
        public Slider TrajectoryReviewSlider;
        public GameObject ExecuteTrajectoryButtonObject;
        public GameObject RobotSelectionControlObjects;
        public GameObject RobotSelectionDropdownObject;
        public TMP_Dropdown RobotSelectionDropdown;
        public GameObject SetActiveRobotToggleObject;

        //Object Colors
        private Color Yellow = new Color(1.0f, 1.0f, 0.0f, 1.0f);
        private Color White = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        private Color TranspWhite = new Color(1.0f, 1.0f, 1.0f, 0.4f);
        private Color TranspGrey = new Color(0.7843137f, 0.7843137f, 0.7843137f, 0.4f);

        //Parent Objects for gameObjects
        public GameObject Elements;
        public GameObject QRMarkers;
        public GameObject UserObjects;

        //AR Camera and Touch GameObjects & Occlusion Objects
        public Camera arCamera;
        private GameObject activeGameObject;
        private GameObject temporaryObject; 
        private ARRaycastManager rayManager;
        public CompasXR.Systems.OperatingSystem currentOperatingSystem;
        private AROcclusionManager occlusionManager;
        private GameObject OcclusionToggleObject;

        //On Screen Text
        public GameObject CurrentStepTextObject;
        public GameObject EditorSelectedTextObject;
        public TMP_Text CurrentStepText;
        public TMP_Text LastBuiltIndexText;
        public TMP_Text CurrentPriorityText;
        public TMP_Text EditorSelectedText;

        //In script use variables
        public string CurrentStep = null;
        // public string SearchedElement = "None";
        public string SearchedElementStepID;
        public string SelectedPriority = "None";
        public bool IDTagIsOffset = false;
        public bool PriorityTagIsOffset = false;
        
        void Start()
        {
            //Find Initial Objects and other sctipts
            OnAwakeInitilization();
        }

        void Update()
        {
            //Control Touch Search
            TouchSearchControler();
        }

        //TODO: REMOVE RANDOM TESTING METHODS.
        public void PrintRandomIntFromNamespace()
        {
            // Debug.Log("Printing random int from Class Member Variable" + nameSpaceTestingMono.Instance.RandomMonoInt);

            // nameSpaceTestingMono.Instance.methodAccessTesting();

            // nameSpaceTestingMono.Instance.methodAccessStaticMonoTesting();

            // Debug.Log("Printing random int from Static Class Member Variable " + nameSpaceTestingStatic.RandomStaticInt);

            // nameSpaceTestingStatic.methodAccessTestingStatic();
        }

        //TODO: REMOVE RANDOM TESTING METHODS.

        /////////////////////////////////// UI Control & OnStart methods ////////////////////////////////////////////////////
        private void OnAwakeInitilization()
        {
            //Find Other Scripts
            databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
            instantiateObjects = GameObject.Find("Instantiate").GetComponent<InstantiateObjects>();
            eventManager = GameObject.Find("EventManager").GetComponent<Eventmanager>();
            mqttTrajectoryManager = GameObject.Find("MQTTTrajectoryManager").GetComponent<MqttTrajectoryManager>();
            trajectoryVisulizer = GameObject.Find("TrajectoryVisulizer").GetComponent<TrajectoryVisulizer>();
            rosConnectionManager = GameObject.Find("RosManager").GetComponent<RosConnectionManager>();
            scrollSearchManager = GameObject.Find("ScrollSearchManager").GetComponent<ScrollSearchManager>();

            //Find Specific GameObjects
            Elements = GameObject.Find("Elements");
            QRMarkers = GameObject.Find("QRMarkers");
            CanvasObject = GameObject.Find("Canvas");
            UserObjects = GameObject.Find("ActiveUserObjects");     

            //Find AR Camera gameObject
            arCamera = GameObject.Find("XR Origin").FindObject("Camera Offset").FindObject("Main Camera").GetComponent<Camera>();

            //Find the Raycast manager in the script in order to use it to acquire data
            rayManager = FindObjectOfType<ARRaycastManager>();

            //Find and set current operating system
            currentOperatingSystem = OperatingSystemManager.GetCurrentOS();

            //Find Constant UI Pannel
            ConstantUIPanelObjects = GameObject.Find("ConstantUIPanel");
        
            //Set up UI Objects and buttons on start
            SetPrimaryUIItemsOnStart();
            SetVisulizerMenuItemsOnStart();
            SetMenuItemsOnStart();
            SetCommunicationItemsOnStart();
        }
        private void SetPrimaryUIItemsOnStart()
        {
            //Find Next Object, Button, and Add Listener for OnClick method
            FindButtonandSetOnClickAction(ConstantUIPanelObjects, ref NextGeometryButtonObject, "Next_Geometry", NextStepButton);

            //Find Previous Object, Button, and Add Listener for OnClick method
            FindButtonandSetOnClickAction(ConstantUIPanelObjects, ref PreviousGeometryButtonObject, "Previous_Geometry", PreviousStepButton);

            //Find PreviewGeometry Object, Slider, and Add Listener for OnClick method
            FindSliderandSetOnValueChangeAction(CanvasObject, ref PreviewGeometrySliderObject, ref PreviewGeometrySlider, "GeometrySlider", PreviewGeometrySliderSetVisibilty);

            //Find IsBuilt Object, Button, and Add Listener for OnClick method
            IsBuiltPanelObjects = ConstantUIPanelObjects.FindObject("IsBuiltPanel"); 
            FindButtonandSetOnClickAction(IsBuiltPanelObjects, ref IsBuiltButtonObject, "IsBuiltButton", () => ModifyStepBuildStatus(CurrentStep));
            IsbuiltButtonImage = IsBuiltButtonObject.FindObject("Image");
            IsbuiltPriorityLockedImage = IsBuiltButtonObject.FindObject("PriorityLockedImage");

            //Find toggles for menu & Add on value changed event
            FindToggleandSetOnValueChangedAction(CanvasObject, ref MenuButtonObject, "Menu_Toggle", ToggleMenu);

            //Find toggles for visibility menu and add on value changed event
            FindToggleandSetOnValueChangedAction(CanvasObject, ref VisibilityMenuObject, "Visibility_Editor", ToggleVisibilityMenu);

            //Find Text Objects
            CurrentStepTextObject = GameObject.Find("Current_Index_Text");
            CurrentStepText = CurrentStepTextObject.GetComponent<TMPro.TMP_Text>();

            GameObject LastBuiltIndexTextObject = GameObject.Find("LastBuiltElement_Text");
            LastBuiltIndexText = LastBuiltIndexTextObject.GetComponent<TMPro.TMP_Text>();

            GameObject CurrentPriorityTextObject = GameObject.Find("CurrentPriority_Text");
            CurrentPriorityText = CurrentPriorityTextObject.GetComponent<TMPro.TMP_Text>();

            EditorSelectedTextObject = CanvasObject.FindObject("Editor_Selected_Text");
            EditorSelectedText = EditorSelectedTextObject.GetComponent<TMPro.TMP_Text>();
            
            //Find Background Images for Toggles
            VisualzierBackground = VisibilityMenuObject.FindObject("Background_Visualizer");
            MenuBackground = MenuButtonObject.FindObject("Background_Menu");

            //Find OnScreeen Message Prefabs
            MessagesParent = CanvasObject.FindObject("OnScreenMessages");
            OnScreenErrorMessagePrefab = MessagesParent.FindObject("Prefabs").FindObject("OnScreenErrorMessagePrefab");
            OnScreenInfoMessagePrefab = MessagesParent.FindObject("Prefabs").FindObject("OnScreenInfoMessagePrefab");

            //OnScreen Messages with custom acknowledgement events.
            ActiveRobotUpdatedFromPlannerMessageObject = MessagesParent.FindObject("Prefabs").FindObject("ActiveRobotUpdatedFromPlannerMessage");
            TrajectoryReviewRequestMessageObject = MessagesParent.FindObject("Prefabs").FindObject("TrajectoryReviewRequestReceivedMessage");
        }
        private void SetVisulizerMenuItemsOnStart()
        {
            //Find PreviewBuilder Object, Button, and Add Listener for OnClick method
            FindToggleandSetOnValueChangedAction(VisibilityMenuObject, ref PreviewActorToggleObject, "PreviewActorToggle", TogglePreviewActor);

            //Find IDToggle Object, Button, and Add Listener for OnClick method
            FindToggleandSetOnValueChangedAction(VisibilityMenuObject, ref IDToggleObject, "ID_Toggle", ToggleID);

            //Find Robot toggle and Objects
            FindToggleandSetOnValueChangedAction(VisibilityMenuObject, ref RobotToggleObject, "RobotToggle", ToggleRobot);

            //Find toggle for element search
            FindToggleandSetOnValueChangedAction(VisibilityMenuObject, ref ScrollSearchToggleObject, "ScrollSearchToggle", ToggleScrollSearch);
            ScrollSearchObjects = ScrollSearchToggleObject.FindObject("ScrollSearchObjects");

            //Find Robot toggle and Objects
            FindToggleandSetOnValueChangedAction(VisibilityMenuObject, ref PriorityViewerToggleObject, "PriorityViewer", TogglePriority);
            PriorityViewerBackground = PriorityViewerToggleObject.FindObject("BackgroundPriorityViewer");
            SelectedPriorityTextObject = PriorityViewerToggleObject.FindObject("SelectedPriorityText");
            SelectedPriorityText = SelectedPriorityTextObject.GetComponent<TMP_Text>();
            FindButtonandSetOnClickAction(PriorityViewerToggleObject, ref NextPriorityButtonObject, "NextPriorityButton", SetNextPriorityGroup);
            FindButtonandSetOnClickAction(PriorityViewerToggleObject, ref PreviousPriorityButtonObject, "PreviousPriorityButton", SetPreviousPriorityGroup);

            //Find Object Lengths Toggle and Objects
            FindToggleandSetOnValueChangedAction(VisibilityMenuObject, ref ObjectLengthsToggleObject, "ObjectLength_Button", ToggleObjectLengths);
            ObjectLengthsUIPanelObjects = CanvasObject.FindObject("ObjectLengthsPanel");
            ObjectLengthsUIPanelPosition = ObjectLengthsUIPanelObjects.transform.localPosition;
            ObjectLengthsText = ObjectLengthsUIPanelObjects.FindObject("LengthsText").GetComponent<TMP_Text>();
            ObjectLengthsTags = GameObject.Find("ObjectLengthsTags");
        }
        private void SetMenuItemsOnStart()
        {
            //Find Info Toggle, and Add Listener for OnValueChanged method
            FindToggleandSetOnValueChangedAction(MenuButtonObject, ref InfoToggleObject, "Info_Button", ToggleInfo);

            //Find Object, Button, and Add Listener for OnClick method
            FindButtonandSetOnClickAction(MenuButtonObject, ref ReloadButtonObject, "Reload_Button", ReloadApplication);

            //Find communication toggle objects
            FindToggleandSetOnValueChangedAction(MenuButtonObject, ref CommunicationToggleObject, "Communication_Button", ToggleCommunication);

            //Find toggle for editor.
            FindToggleandSetOnValueChangedAction(MenuButtonObject, ref EditorToggleObject, "Editor_Toggle", ToggleEditor);
            
            //Find Object, Button, and Add Listener for OnClick method
            FindButtonandSetOnClickAction(EditorToggleObject, ref BuilderEditorButtonObject, "Builder_Editor_Button", TouchModifyActor);

            //Find Object, Button, and Add Listener for OnClick method
            FindButtonandSetOnClickAction(EditorToggleObject, ref BuildStatusButtonObject, "Build_Status_Editor", TouchModifyBuildStatus);

            //Find communication toggle objects
            FindToggleandSetOnValueChangedAction(MenuButtonObject, ref CommunicationToggleObject, "Communication_Button", ToggleCommunication);

            //Find Panel Objects used for Info and communication
            InfoPanelObject = CanvasObject.FindObject("InfoPanel");
            CommunicationPanelObject = CanvasObject.FindObject("CommunicationPanel");

            //Find Background Images for Toggles
            EditorBackground = EditorToggleObject.FindObject("Background_Editor");
        }
        private void SetCommunicationItemsOnStart()
        {
            //Find Pannel Objects used for connecting to a different MQTT broker
            MqttBrokerInputField = CommunicationPanelObject.FindObject("MqttBrokerInputField").GetComponent<TMP_InputField>();
            MqttPortInputField = CommunicationPanelObject.FindObject("MqttPortInputField").GetComponent<TMP_InputField>();
            MqttUpdateConnectionMessage = CommunicationPanelObject.FindObject("UpdateInputsMQTTReconnectMessage");
            MqttConnectionStatusObject = CommunicationPanelObject.FindObject("MqttConnectionStatusObject");
            FindButtonandSetOnClickAction(CommunicationPanelObject, ref MqttConnectButtonObject, "MqttConnectButton", UpdateMqttConnectionFromUserInputs);

            //Find Pannel Objects used for connecting to a different ROS host
            RosHostInputField = CommunicationPanelObject.FindObject("ROSHostInputField").GetComponent<TMP_InputField>();
            RosPortInputField = CommunicationPanelObject.FindObject("ROSPortInputField").GetComponent<TMP_InputField>();
            RosUpdateConnectionMessage = CommunicationPanelObject.FindObject("UpdateInputsROSReconnectMessage");
            RosConnectionStatusObject = CommunicationPanelObject.FindObject("ROSConnectionStatusObject");
            FindButtonandSetOnClickAction(CommunicationPanelObject, ref RosConnectButtonObject, "ROSConnectButton", UpdateRosConnectionFromUserInputs);

            //Find Control Objects and set up events
            GameObject TrajectoryControlObjects = GameObject.Find("TrajectoryReviewUIControls");
            ReviewTrajectoryObjects = TrajectoryControlObjects.FindObject("ReviewTrajectoryControls");

            //Find Object, request button and add event listner for on click method
            FindButtonandSetOnClickAction(TrajectoryControlObjects, ref RequestTrajectoryButtonObject, "RequestTrajectoryButton", RequestTrajectoryButtonMethod);
        
            //Find object, approve button and add event listner for on click method
            FindButtonandSetOnClickAction(ReviewTrajectoryObjects, ref ApproveTrajectoryButtonObject, "ApproveTrajectoryButton", ApproveTrajectoryButtonMethod);

            //Find Reject button object, reject button and add event listner for on click method
            FindButtonandSetOnClickAction(ReviewTrajectoryObjects, ref RejectTrajectoryButtonObject, "RejectTrajectoryButton", RejectTrajectoryButtonMethod);

            //Find slider for trajectory review and add event listner for on value changed method
            FindSliderandSetOnValueChangeAction(ReviewTrajectoryObjects, ref TrajectoryReviewSliderObject, ref TrajectoryReviewSlider, "TrajectoryReviewSlider", TrajectorySliderReviewMethod);

            //Find Object, Execute button and add event listner for on click method
            FindButtonandSetOnClickAction(TrajectoryControlObjects, ref ExecuteTrajectoryButtonObject, "ExecuteTrajectoryButton", ExecuteTrajectoryButtonMethod);

            //Find Objects for active robot selection
            RobotSelectionControlObjects = GameObject.Find("RobotSelectionControls");
            RobotSelectionDropdownObject = RobotSelectionControlObjects.FindObject("RobotSelectionDropdown");
            RobotSelectionDropdown = RobotSelectionDropdownObject.GetComponent<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> robotOptions = SetDropDownOptionsFromStringList(RobotSelectionDropdown ,trajectoryVisulizer.RobotURDFList);
            RobotSelectionDropdown.onValueChanged.AddListener(RobotSelectionDropdownValueChanged);
            if(RobotSelectionControlObjects == null)
            {
                Debug.Log("Robot Selection Control Objects is null.");

            }
            else if (RobotSelectionDropdownObject == null)
            {
                Debug.Log("Robot Selection Dropdown Object is null.");
            }
            else
            {
                RobotSelectionDropdown.options = robotOptions;
            }

            //Find Object, Execute button and add event listner for on click method
            FindToggleandSetOnValueChangedAction(RobotSelectionControlObjects, ref SetActiveRobotToggleObject, "SetActiveRobotToggle", SetActiveRobotToggleMethod);
        }
        public void SetUIObjectColor(GameObject Button, Color color)
        {
            Button.GetComponent<Image>().color = color;
        }
        public void FindButtonandSetOnClickAction(GameObject searchObject, ref GameObject buttonParentObjectReference, string unityObjectName, UnityAction customAction)
        {
            //Check if the search object is null
            if (searchObject != null)
            {    
                //Find Object, Button and add event listner for on click method
                buttonParentObjectReference = searchObject.FindObject(unityObjectName);
                Button buttonComponent = buttonParentObjectReference.GetComponent<Button>();
                buttonComponent.onClick.AddListener(customAction);
            }
            else
            {
                Debug.LogError($"Button Constructer: Could not Set OnClick Action because search object is null for {unityObjectName}");
            }
        }
        public void FindToggleandSetOnValueChangedAction(GameObject searchObject, ref GameObject toggleParentObjectReference, string unityObjectName, UnityAction<Toggle> customAction)
        {
            //Check if the search object is null
            if (searchObject != null)
            {    
                //Find Object, Toggle and add event listner for on value changed method
                toggleParentObjectReference = searchObject.FindObject(unityObjectName);
                Toggle toggleComponent = toggleParentObjectReference.GetComponent<Toggle>();
                toggleComponent.onValueChanged.AddListener(value => customAction(toggleComponent));
            }
            else
            {
                Debug.LogError($"Toggle Constructer: Could not Set OnValueChanged Action because search object is null for {unityObjectName}");
            }
        }
        public void FindSliderandSetOnValueChangeAction(GameObject searchObject, ref GameObject sliderParentObjectReference, ref Slider sliderObjectReference, string unityObjectName, UnityAction<float> customAction)
        {
            if(searchObject != null)
            {
                //Find Object, Slider and add event listner for on value changed method
                sliderParentObjectReference = searchObject.FindObject(unityObjectName);
                sliderObjectReference = sliderParentObjectReference.GetComponent<Slider>();
                sliderObjectReference.onValueChanged.AddListener(customAction);
            }
            else
            {
                Debug.LogError($"Slider Constructer: Could not Set OnValueChanged Action because search object is null for {unityObjectName}");
            }
        }
        public void print_string_on_click(string Text)
        {
            Debug.Log(Text);
        }
        public void SetOcclusionFromOS(ref AROcclusionManager occlusionManager, CompasXR.Systems.OperatingSystem currentOperatingSystem)
        {
            Debug.Log($"SetOcclusionFromOS: Current Operating System is {currentOperatingSystem}");
            if(currentOperatingSystem == CompasXR.Systems.OperatingSystem.iOS)
            {
                occlusionManager = FindObjectOfType<AROcclusionManager>(true);
                occlusionManager.enabled = true;

                Debug.Log("AROcclusion: will be activated because current platform is ios");
            }
            else
            {
                Debug.Log("AROcclusion: will not be activated because current system is not ios");
            }
        }

        /////////////////////////////////////// Primary UI Functions //////////////////////////////////////////////
        public void ToggleVisibilityMenu(Toggle toggle)
        {
            if (VisualzierBackground != null && PreviewActorToggleObject != null && RobotToggleObject != null && ObjectLengthsToggleObject != null && IDToggleObject != null && PriorityViewerToggleObject != null)
            {    
                if (toggle.isOn)
                {             
                    //Set Visibility of buttons
                    VisualzierBackground.SetActive(true);
                    PreviewActorToggleObject.SetActive(true);
                    RobotToggleObject.SetActive(true);
                    ObjectLengthsToggleObject.SetActive(true);
                    IDToggleObject.SetActive(true);
                    PriorityViewerToggleObject.SetActive(true);
                    ScrollSearchToggleObject.SetActive(true);

                    //Set color of toggle
                    SetUIObjectColor(VisibilityMenuObject, Yellow);

                }
                else
                {
                    //Set Visibility of buttons
                    VisualzierBackground.SetActive(false);
                    PreviewActorToggleObject.SetActive(false);
                    RobotToggleObject.SetActive(false);
                    ObjectLengthsToggleObject.SetActive(false);
                    IDToggleObject.SetActive(false);
                    PriorityViewerToggleObject.SetActive(false);
                    ScrollSearchToggleObject.SetActive(false);

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
            if (MenuBackground != null && InfoToggleObject != null && ReloadButtonObject != null && CommunicationToggleObject != null && EditorToggleObject != null)
            {    
                if (toggle.isOn)
                {             
                    //Set Visibility of buttons
                    MenuBackground.SetActive(true);
                    InfoToggleObject.SetActive(true);
                    ReloadButtonObject.SetActive(true);
                    CommunicationToggleObject.SetActive(true);
                    EditorToggleObject.SetActive(true);

                    //Set color of toggle
                    SetUIObjectColor(MenuButtonObject, Yellow);

                }
                else
                {
                    //Control Menu Internal Toggles
                    if(EditorToggleObject.GetComponent<Toggle>().isOn){
                        EditorToggleObject.GetComponent<Toggle>().isOn = false;
                    }
                    if(InfoToggleObject.GetComponent<Toggle>().isOn){
                        InfoToggleObject.GetComponent<Toggle>().isOn = false;
                    }
                    if(CommunicationToggleObject.GetComponent<Toggle>().isOn){
                        CommunicationToggleObject.GetComponent<Toggle>().isOn = false;
                    }

                    //Set Visibility of buttons
                    MenuBackground.SetActive(false);
                    InfoToggleObject.SetActive(false);
                    ReloadButtonObject.SetActive(false);
                    CommunicationToggleObject.SetActive(false);
                    EditorToggleObject.SetActive(false);

                    //Set color of toggle
                    SetUIObjectColor(MenuButtonObject, White);
                }
            }
            else
            {
                Debug.LogWarning("Could not find one of the buttons in the Menu.");
            }   
        }
        public void NextStepButton()
        {
            //Press Next Element Button
            Debug.Log("Next Element Button Pressed");
            
            //If current step is not null and smaller then the length of the list
            if(CurrentStep != null)
            {
                int CurrentStepInt = Convert.ToInt16(CurrentStep);

                if(CurrentStepInt < databaseManager.BuildingPlanDataItem.steps.Count - 1)
                {
                    //Set current element as this step + 1
                    SetCurrentStep((CurrentStepInt + 1).ToString());
                }  
            }
        }
        public void SetCurrentStep(string key)
        {
            //If the current step is not null find the previous current step and color it bulit or unbuilt.
            if(CurrentStep != null)
            {
                //Find Arrow and Destroy it
                instantiateObjects.RemoveObjects($"{CurrentStep} Arrow");
                
                //Find Gameobject Associated with that step
                GameObject previousStepElement = Elements.FindObject(CurrentStep);

                if(previousStepElement != null)
                {
                    //Color previous object based on Visulization Mode
                    Step PreviousStep = databaseManager.BuildingPlanDataItem.steps[CurrentStep];
                    string elementID = PreviousStep.data.element_ids[0];

                    instantiateObjects.ObjectColorandTouchEvaluater(instantiateObjects.visulizationController.VisulizationMode, instantiateObjects.visulizationController.TouchMode, PreviousStep, key, previousStepElement.FindObject(elementID + " Geometry"));

                    //If Priority Viewer toggle is on then color the add additional color based on priority
                    if (PriorityViewerToggleObject.GetComponent<Toggle>().isOn)
                    {
                        instantiateObjects.ColorObjectByPriority(SelectedPriority, PreviousStep.data.priority.ToString(), CurrentStep, previousStepElement.FindObject(elementID + " Geometry"));
                    }
                }
            }

            if(CurrentStep == null)
            {
                Debug.Log("CURRENT STEP IS NULL");
            }
            else
            {
                Debug.Log($"CURRENT STEP IS {CurrentStep.ToString()}");
            }
                    
            //Set current element name
            CurrentStep = key;

            //Find the step in the dictoinary
            Step step = databaseManager.BuildingPlanDataItem.steps[key];

            //Find Gameobject Associated with that step
            GameObject element = Elements.FindObject(key);

            if(element != null)
            {
                //Color it Human or Robot Built
                instantiateObjects.ColorHumanOrRobot(step.data.actor, step.data.is_built, element.FindObject(step.data.element_ids[0] + " Geometry"));
                Debug.Log($"Current Step is {CurrentStep}");
            }
            
            //Update Onscreen Text
            CurrentStepText.text = CurrentStep;
            
            //Instantiate an arrow at the current step
            // instantiateObjects.ArrowInstantiator(element, CurrentStep);
            instantiateObjects.UserIndicatorInstantiator(ref instantiateObjects.MyUserIndacator, element, CurrentStep, CurrentStep, "ME", 0.25f);

            //Write Current Step to the database under my device name
            UserCurrentInfo userCurrentInfo = new UserCurrentInfo();
            userCurrentInfo.currentStep = CurrentStep;
            userCurrentInfo.timeStamp = (System.DateTime.UtcNow.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss"));

            //Add to the UserCurrentStepDict
            databaseManager.UserCurrentStepDict[SystemInfo.deviceUniqueIdentifier] = userCurrentInfo;

            //Push Current key to the firebase
            databaseManager.PushStringData(databaseManager.dbrefernece_usersCurrentSteps.Child(SystemInfo.deviceUniqueIdentifier), JsonConvert.SerializeObject(userCurrentInfo));

            //Update Lengths if Object Lengths Toggle is on
            if(ObjectLengthsToggleObject.GetComponent<Toggle>().isOn)
            {
                instantiateObjects.CalculateandSetLengthPositions(CurrentStep);
            }

            //Update Trajectory Request interactibility based on new current step
            if(RobotToggleObject.GetComponent<Toggle>().isOn)
            {
                //Set interaction based on current step.
                SetRoboticUIElementsFromKey(CurrentStep);

                //If the current robot is not null then set visibility
                if(trajectoryVisulizer.ActiveRobot != null)
                {
                    if(step.data.actor == "ROBOT")
                    {
                        trajectoryVisulizer.ActiveRobot.SetActive(true);
                        trajectoryVisulizer.ActiveRobot.transform.GetChild(0).gameObject.SetActive(true);
                    }
                    else
                    {
                        trajectoryVisulizer.ActiveRobot.SetActive(false);
                    }
                    //If the Active Trajectory child count is greater the 0 then destroy children
                    if(trajectoryVisulizer.ActiveTrajectoryParentObject.transform.childCount > 0)
                    {
                        trajectoryVisulizer.DestroyActiveTrajectoryChildren();
                    }
                }
                else
                {
                    Debug.LogWarning("SetCurrentStep: Active Robot is null.");
                }
            }

            //Update Preview Geometry the visulization is remapped correctly
            PreviewGeometrySliderSetVisibilty(PreviewGeometrySlider.value);
            
            //Update Is Built Button
            IsBuiltButtonGraphicsControler(step.data.is_built, step.data.priority);
        }
        public void PreviousStepButton()
        {
            //Previous element button clicked
            Debug.Log("Previous Element Button Pressed");

            //If current step is not null and greater then Zero add subtract 1
            if(CurrentStep != null)
            {
            int CurrentStepInt = Convert.ToInt16(CurrentStep);

                if(CurrentStepInt > 0)
                {
                    //Set current element as this step - 1
                    SetCurrentStep((CurrentStepInt - 1).ToString());
                }  
            }       

        }
        public void PreviewGeometrySliderSetVisibilty(float value)
        {
            if (CurrentStep != null)
            {
                Debug.Log("You are changing the Preview Geometry");
                int min = Convert.ToInt16(CurrentStep);
                float SliderValue = value;
                int ElementsTotal = databaseManager.BuildingPlanDataItem.steps.Count;
                float SliderMax = 1; //Input Slider Max Value == 1
                float SliderMin = 0; // Input Slider Min Value == 0
                    
                float SliderRemaped = HelpersExtensions.Remap(SliderValue, SliderMin, SliderMax, min, ElementsTotal); 

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
        public void IsBuiltButtonGraphicsControler(bool builtStatus, int stepPriority)
        {
            if (IsBuiltPanelObjects.activeSelf)
            {
                //Set is built button graphis based on build status
                if (builtStatus)
                {
                    IsbuiltButtonImage.SetActive(true);
                    IsBuiltButtonObject.GetComponent<Image>().color = TranspGrey;
                }
                else
                {
                    IsbuiltButtonImage.SetActive(false);
                    IsBuiltButtonObject.GetComponent<Image>().color = TranspWhite;
                }
            }
        }
        public bool LocalPriorityChecker(Step step)
        {
            //Check if the current priority is null
            if(databaseManager.CurrentPriority == null)
            {
                //Print out the priority tree as a check
                Debug.LogError("Current Priority is null.");
                
                //Return false to not push data.
                return false;
            }
            
            //Check if they are the same. If they are return true
            else if (databaseManager.CurrentPriority == step.data.priority.ToString())
            {
                Debug.Log($"Priority Check: Current Priority is equal to step priority. Pushing data");

                //Return true to push all the data to the database
                return true;
            }

            //Else if the current priority is higher then the step priority loop through all the elements in priority above and unbuild them. This allows you to go back in priority.
            //TODO: THIS ONLY WORKS BECAUSE WE PUSH EVERYTHING.
            else if (Convert.ToInt16(databaseManager.CurrentPriority) > step.data.priority)
            {
                Debug.Log($"Priority Check: Current Priority is higher then the step priority. Unbuilding elements.");
            
                //Loop from steps priority to the highest priority group and unbuild all elements above this one.
                for(int i = Convert.ToInt16(step.data.priority) + 1; i < databaseManager.PriorityTreeDict.Count; i++)
                {

                    //Find the current priority in the dictionary for iteration
                    List<string> PriorityDataItem = databaseManager.PriorityTreeDict[i.ToString()];

                    //Iterate through the Priority tree dictionary to unbuild elements of a higher priority.
                    foreach(string key in PriorityDataItem)
                    {
                        //Find the step in the dictoinary
                        Step stepToUnbuild = databaseManager.BuildingPlanDataItem.steps[key];

                        //If step is built unbuild it
                        if(stepToUnbuild.data.is_built)
                        {                        
                            //Unbuild the element
                            stepToUnbuild.data.is_built = false;
                        }

                        //Update color and touch depending on what is on.
                        instantiateObjects.ObjectColorandTouchEvaluater(instantiateObjects.visulizationController.VisulizationMode, instantiateObjects.visulizationController.TouchMode, stepToUnbuild, key, Elements.FindObject(key).FindObject(stepToUnbuild.data.element_ids[0] + " Geometry"));
                    
                    }
                }

                //Return true to push data to the database
                return true;
        
            }
            //The priority is higher. Check if all elements in Current Priority are built.
            else
            {
                //if the elements priority is more then 1 greater then current priority return false and signal on screen warning.
                if(step.data.priority != Convert.ToInt16(databaseManager.CurrentPriority) + 1)
                {
                    Debug.Log($"Priority Check: Current Priority is more then 1 greater then the step priority. Incorrect Priority");

                    //Signal on screen message for priority incorrect
                    string message = $"WARNING: This elements priority is incorrect. It is priority {step.data.priority.ToString()} and next priority to build is {Convert.ToInt16(databaseManager.CurrentPriority) + 1}";
                    // SignalOnScreenMessageFromReference(ref PriorityIncorrectWarningMessageObject, message, "Priority Incorrect Warning");
                    
                    SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref PriorityIncorrectWarningMessageObject, "PriorityIncorrectWarningMessage", MessagesParent, message, "LocalPriorityChecker: Priority Incorrect Warning");
                    //Return false to not push data.
                    return false;
                }

                //This is the next Priority.
                else
                {   
                    //New empty list to store unbuilt elements
                    List<string> UnbuiltElements = new List<string>();
                    
                    //Find the current priority in the dictionary for iteration
                    List<string> PriorityDataItem = databaseManager.PriorityTreeDict[databaseManager.CurrentPriority];

                    //Iterate through the Priority tree dictionary to check the elements and if the priority is complete
                    foreach(string element in PriorityDataItem)
                    {
                        //Find the step in the dictoinary
                        Step stepToCheck = databaseManager.BuildingPlanDataItem.steps[element];

                        //Check if the element is built
                        if(!stepToCheck.data.is_built)
                        {
                            UnbuiltElements.Add(element);
                        }
                    }

                    //If the list is empty return false because all elements of that priority are built, and we want to move on to the next priority but not write info.
                    if(UnbuiltElements.Count == 0)
                    {
                        Debug.Log($"Priority Check: Current Priority is complete. Unlocking Next Priority.");

                        //Signal on screen message for priority complete
                        string message = $"The previous priority {databaseManager.CurrentPriority} is complete you are now moving on to priority {step.data.priority.ToString()}.";
                        // SignalOnScreenMessageFromReference(ref PriorityCompleteMessageObject, message ,"Priority Complete Message");
                        SignalOnScreenMessageFromPrefab(ref OnScreenInfoMessagePrefab, ref PriorityCompleteMessageObject, "PriorityCompleteMessage", MessagesParent, message, "LocalPriorityChecker: Priority Complete Message");
                        
                        //Set Current Priority
                        SetCurrentPriority(step.data.priority.ToString());

                        //If my CurrentStep Priority is the same as New Current Priority then update UI graphics
                        if(databaseManager.BuildingPlanDataItem.steps[CurrentStep].data.priority.ToString() == databaseManager.CurrentPriority)
                        {    
                            IsBuiltButtonGraphicsControler(step.data.is_built, step.data.priority);
                        }
                        
                        //Return false, this is for the first time that an element is changed and we only want to update a priority, but not write information.
                        return false;
                    }
                    
                    //If the list is not empty return false because not all elements of that priority are built and signal on screen warning.
                    else
                    {
                        //Signal on screen message for priority incomplete
                        string message = $"WARNING: This element cannot build because the following elements from Current Priority {databaseManager.CurrentPriority} are not built: {string.Join(", ", UnbuiltElements)}";
                        SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref PriorityIncompleteWarningMessageObject, "PriorityIncompleteWarningMessage", MessagesParent, message, "LocalPriorityChecker: Priority Incomplete Warning");

                        //Return true to not push data.
                        return false;
                    }
                }
            }
        }
        public void ModifyStepBuildStatus(string key)
        {
            Debug.Log($"Modifying Build Status of: {key}");

            //Find the step in the dictoinary
            Step step = databaseManager.BuildingPlanDataItem.steps[key];

            //Check if priority is correct.
            if (LocalPriorityChecker(step))
            {
                //Change Build Status
                if(step.data.is_built)
                {
                    //Change Build Status
                    step.data.is_built = false;

                    //Convert my key to an int
                    int StepInt = Convert.ToInt16(key);

                    //Iterate through steps backwards to find the last built step that is closest to my current step
                    for(int i = StepInt; i >= 0; i--)
                    {
                        //Find Step in the dictionary
                        Step stepToCheck = databaseManager.BuildingPlanDataItem.steps[i.ToString()];
                        
                        //Check if step int is 0 and then set current priority to 0 and last built index do nothing
                        if(StepInt == 0)
                        {
                            //Set Current Priority but leave last built index alone.
                            SetCurrentPriority(stepToCheck.data.priority.ToString());

                            //exit if condition above this one
                            break;   
                        }

                        if(stepToCheck.data.is_built)
                        {
                            //Change LastBuiltIndex
                            databaseManager.BuildingPlanDataItem.LastBuiltIndex = i.ToString();
                            SetLastBuiltText(i.ToString());

                            //Set Current Priority
                            SetCurrentPriority(stepToCheck.data.priority.ToString());

                            break;
                        }
                    }

                }
                else
                {
                    //Change Build Status
                    step.data.is_built = true;

                    //Change LastBuiltIndex
                    databaseManager.BuildingPlanDataItem.LastBuiltIndex = key;
                    SetLastBuiltText(key);

                    //Set Current Priority
                    SetCurrentPriority(step.data.priority.ToString());
                }

                //Update color
                instantiateObjects.ColorHumanOrRobot(step.data.actor, step.data.is_built, Elements.FindObject(key).FindObject(step.data.element_ids[0] + " Geometry"));
                
                //If it is current element update UI graphics
                if(key == CurrentStep)
                {    
                    //Update Is Built Button
                    IsBuiltButtonGraphicsControler(step.data.is_built, step.data.priority);
                }

                //Push Data to the database
                databaseManager.PushAllDataBuildingPlan(key);
            }
            else
            {
                Debug.Log("Priority Check: Checked Priority and not pushing Data.");
            }

        }
        public void SetLastBuiltText(string key)
        {
            //Set Last Built Text
            LastBuiltIndexText.text = $"Last Built Element : {key}";
        }
        public void SetCurrentPriority(string Priority)
        {        
            //If Priority Viewer is on and new priority is not equal to current priority update the priority viewer (only place I can do this)
            if(PriorityViewerToggleObject.GetComponent<Toggle>().isOn && databaseManager.CurrentPriority != Priority)
            {
                //Update Priority Viewer
                instantiateObjects.ApplyColorBasedOnPriority(Priority);
                
                //Create the priority line if it is on
                instantiateObjects.CreatePriorityViewerItems(Priority, ref instantiateObjects.PriorityViewrLineObject, Color.red, 0.03f, 0.125f, Color.red, instantiateObjects.PriorityViewerPointsObject);

                //Set the selected priority
                SelectedPriority = Priority;

                //Set selected priority text
                PriorityViewerObjectsGraphicsController(true, Priority);
            }
            
            //Current Priority Text current Priority Items
            databaseManager.CurrentPriority = Priority;

            //Update Trajectory Request interactibility based on my current step after priority check if Robot Toggle is on //TODO: THIS ONLY WORKS BECAUSE I UPDATE CURRENT PRIORITY EVERY TIME I WRITE.
            if(RobotToggleObject.GetComponent<Toggle>().isOn)
            {
                //Set interaction based on current step.
                SetRoboticUIElementsFromKey(CurrentStep);
            }
            
            //Set On Screen Text
            CurrentPriorityText.text = $"Current Priority : {Priority}";
            
            //Print setting current priority
            Debug.Log($"Setting Current Priority to {Priority} ");
        }
        public List<TMP_Dropdown.OptionData> SetDropDownOptionsFromStringList(TMP_Dropdown dropDown, List<string> stringList)
        {
            //Create a new list of option data
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

            //Iterate through the robot list and add them to the option data list
            foreach(string stringItem in stringList)
            {
                options.Add(new TMP_Dropdown.OptionData(stringItem));
            }

            Debug.Log($"SetDropDownOptionsFromStringList: Added Options {options} to Dropdown {dropDown.name}");
            
            //Return the options list
            return options;
        }
        public TMP_Dropdown.OptionData AddOptionDataToDropdown(string option, TMP_Dropdown dropDown)
        {
            Debug.Log($"AddOptionDataToDropdown: Adding Option {option} to Dropdown {dropDown.name}");
            
            //Create a new option data
            TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData(option);

            //Add the option to the dropdown
            dropDown.options.Add(newOption);

            //Return the new option
            return newOption;
        }
        public void SetActiveRobotToggleMethod(Toggle toggle)
        {
            if(toggle!=null && toggle.isOn)
            {
                Debug.Log($"SettingActiveRobotButtonMethod: Setting Active Robot based on input {RobotSelectionDropdown.options[RobotSelectionDropdown.value].text}");
                
                //Robot name from dropdown and visibility based on current visibility of active robot if it is on.
                string robotName = RobotSelectionDropdown.options[RobotSelectionDropdown.value].text;
                
                //Set active robot visibility based on the CurrentStep
                bool visibility = false;
                if(CurrentStep != null && RobotToggleObject.GetComponent<Toggle>().isOn)
                {
                    if(databaseManager.BuildingPlanDataItem.steps[CurrentStep].data.actor == "ROBOT")
                    {
                        visibility = true;
                    }
                }

                //Set Active Robot
                trajectoryVisulizer.SetActiveRobotFromDropdown(robotName, true, visibility);

                //Turn on the check mark image on
                SetActiveRobotToggleObject.FindObject("Image").SetActive(true);
            }
            else
            {
                Debug.Log("SettingActiveRobotButtonMethod: Destroying Current Active Robot");

                //If the active robot is not null destroy it.
                if(trajectoryVisulizer.ActiveRobotObjects.transform.childCount > 0)
                {
                    trajectoryVisulizer.DestroyActiveRobotObjects();
                }

                //Change My Active Robot to null for MQTT Service Manager
                mqttTrajectoryManager.serviceManager.ActiveRobotName = null;

                //Turn on the check mark image off
                SetActiveRobotToggleObject.FindObject("Image").SetActive(false);           
            }
        }
        public void RobotSelectionDropdownValueChanged(int dropDownValue)
        {
            Debug.Log($"RobotSelectionDropdownValueChanged: Robot Selection Dropdown Value Changed to {dropDownValue}. Setting Current Active Robot to False.");

            //Set Active Robot Toggle to False
            SetActiveRobotToggleObject.GetComponent<Toggle>().isOn = false;
        }

        /////////////////////////////////////// On Screen Message Functions //////////////////////////////////////////////
        public void SignalTrajectoryReviewRequest(string key, string robotName, string activeRobotName, Action visualizeRobotMethod)
        {
            Debug.Log($"Trajectory Review Request: Other User is Requesting review of Trajectory for Step {key} .");

            //Find text component for on screen message
            TMP_Text messageComponent = TrajectoryReviewRequestMessageObject.FindObject("MessageText").GetComponent<TMP_Text>();

            //Define message for the onscreen text
            string message = null;
            if(activeRobotName != robotName)
            {
                //Set message to notify that the active robot has been updated
                message = $"REQUEST : Trajectory Review requested for step: {key} with Robot: {robotName}. YOUR ACTIVE ROBOT UPDATED.";

                //Update Active robot usign the dropdown.
                int robotSelection = RobotSelectionDropdown.options.FindIndex(option => option.text == robotName);

                //If the robot is in the dropdown options then update the active robot
                if(robotSelection != -1)
                {            
                    if(SetActiveRobotToggleObject.GetComponent<Toggle>().isOn)
                    {
                        SetActiveRobotToggleObject.GetComponent<Toggle>().isOn = false;
                    }

                    //Set the dropdown value to the robot selection
                    RobotSelectionDropdown.value = robotSelection;

                    //Set Active Robot
                    SetActiveRobotToggleObject.GetComponent<Toggle>().isOn = true;
                }
                else
                {
                    Debug.LogError("Trajectory Review Request Message: Could not find robot in dropdown options.");
                }
            }
            else
            {
                message = $"REQUEST : Trajectory Review requested for step: {key} with Robot: {robotName}.";
            }

            //If the transaction lock message is active turn it off
            if(TransactionLockActiveWarningMessageObject != null && TransactionLockActiveWarningMessageObject.activeSelf)
            {
                //Set visibility of transaction lock active warning message
                TransactionLockActiveWarningMessageObject.SetActive(false);
            }
            
            if(messageComponent != null && message != null && TrajectoryReviewRequestMessageObject != null)
            {
                //Signal On Screen Message with Acknowledge Button
                SignalOnScreenMessageWithButton(TrajectoryReviewRequestMessageObject, messageComponent, message);
            }
            else
            {
                Debug.LogWarning("Trajectory Review Request Message: Could not find message object or message component.");
            }

            //Add additional for acknolwedge button to acknowledge button if they are not already there.
            GameObject AcknowledgeButton = TrajectoryReviewRequestMessageObject.FindObject("AcknowledgeButton");

            //Check if this item already has a listner or not.
            if (AcknowledgeButton!= null && AcknowledgeButton.GetComponent<Button>().onClick.GetPersistentEventCount() <= 1)
            {
                //Add Listner for Acknowledge Button of this message to Set Current Step
                AcknowledgeButton.GetComponent<Button>().onClick.AddListener(() => SetCurrentStep(key));

                //Add Listner for Acknowledge Button of this message to turn on robot toggle
                AcknowledgeButton.GetComponent<Button>().onClick.AddListener(() => 
                {
                    //if robot toggle is off turn it on
                    if(!RobotToggleObject.GetComponent<Toggle>().isOn)
                    {
                        RobotToggleObject.GetComponent<Toggle>().isOn = true;
                    }
                });

                //Add Listner for Acknowledge Button of this message to visualize robot
                AcknowledgeButton.GetComponent<Button>().onClick.AddListener(() => visualizeRobotMethod());

                //Add Listner for Acknowledge Button of this to set interactibility of review trajectory buttons
                AcknowledgeButton.GetComponent<Button>().onClick.AddListener(() => TrajectoryServicesUIControler(false, false, true, true, false, false));
            }
            else
            {
                Debug.LogWarning("Trajectory Review Request Message: Something Is messed up with on click event listner.");
            }

        }
        public void SignalActiveRobotUpdateFromPlanner(string key, string robotName, string activeRobotName, Action visualizeRobotMethod)
        {
            Debug.Log($"SignalActiveRobotUpdateFromPlanner: Other User is Requesting review of Trajectory for Step {key} .");

            //Find text component for on screen message
            TMP_Text messageComponent = ActiveRobotUpdatedFromPlannerMessageObject.FindObject("MessageText").GetComponent<TMP_Text>();


            //Set message to notify that the active robot has been updated
            string message = $"WARNING: You requested for {activeRobotName} but reply Trajectory is for {robotName}. ACTIVE ROBOT UPDATED.";

            //Update Active robot usign the dropdown.
            int robotSelection = RobotSelectionDropdown.options.FindIndex(option => option.text == robotName);

            //If the robot is in the dropdown options then update the active robot
            if(robotSelection != -1)
            {            
                if(SetActiveRobotToggleObject.GetComponent<Toggle>().isOn)
                {
                    SetActiveRobotToggleObject.GetComponent<Toggle>().isOn = false;
                }

                //Set the dropdown value to the robot selection
                RobotSelectionDropdown.value = robotSelection;

                //Set Active Robot
                SetActiveRobotToggleObject.GetComponent<Toggle>().isOn = true;
            }
            else
            {
                Debug.LogError("SignalActiveRobotUpdateFromPlanner: Could not find robot in dropdown options.");
            }
            
            //Check Messaging componenets
            if(messageComponent != null && message != null && ActiveRobotUpdatedFromPlannerMessageObject != null)
            {
                //Signal On Screen Message with Acknowledge Button
                SignalOnScreenMessageWithButton(ActiveRobotUpdatedFromPlannerMessageObject, messageComponent, message);
            }
            else
            {
                Debug.LogWarning("SignalActiveRobotUpdateFromPlanner: Could not find message object or message component.");
            }

            //Add additional for acknolwedge button to acknowledge button if they are not already there.
            GameObject AcknowledgeButton = ActiveRobotUpdatedFromPlannerMessageObject.FindObject("AcknowledgeButton");

            //Check if this item already has a listner or not.
            if (AcknowledgeButton!= null && AcknowledgeButton.GetComponent<Button>().onClick.GetPersistentEventCount() <= 1)
            {
                //Add Listner for Acknowledge Button of this message to visualize robot
                AcknowledgeButton.GetComponent<Button>().onClick.AddListener(() => visualizeRobotMethod());

                //Add Listner for Acknowledge Button of this to set interactibility of review trajectory buttons //TODO: CAN GET RID OF THIS.
                AcknowledgeButton.GetComponent<Button>().onClick.AddListener(() => TrajectoryServicesUIControler(false, false, true, true, false, false));
            }
            else
            {
                Debug.LogWarning("SignalActiveRobotUpdateFromPlanner: Something Is messed up with on click event listner.");
            }

        }
        public void SignalMQTTConnectionFailed()
        {
            Debug.LogWarning("MQTT: MQTT Connection Failed.");
            
            //Check if the Connectoin Toggle is on and if it is turn it off.
            if(CommunicationToggleObject.GetComponent<Toggle>().isOn)
            {
                CommunicationToggleObject.GetComponent<Toggle>().isOn = false;
            }
            
            //Signal On Screen Message with Acknowledge Button
            string message = $"WARNING: MQTT Failed to connect to broker: {mqttTrajectoryManager.brokerAddress} on port: {mqttTrajectoryManager.brokerPort}. Please check your internet and try again.";
            SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref MQTTFailedToConnectMessageObject, "MQTTConnectionFailedMessage", MessagesParent, message, "SignalMQTTConnectionFailed: MQTT Connection Failed.");
        
        }
        public void SignalOnScreenMessageFromPrefab(ref GameObject prefabReference, ref GameObject messageObjectReference, string activeMessageGameObjectName, GameObject activeMessageParent, string message, string logMessageName)
        {
            Debug.Log($"SignalOnScreenMessageFromPrefab: {logMessageName}: Signal On Screen Message.");

            //If the message object reference is null then create it from the prefab
            if(messageObjectReference == null)
            {
                //Instantiate the prefab
                messageObjectReference = Instantiate(prefabReference);
                messageObjectReference.transform.SetParent(activeMessageParent.transform, false);

                //Set the name of the message object
                messageObjectReference.name = activeMessageGameObjectName;
            }

            //Find text component for on screen message
            TMP_Text messageTextComponent = messageObjectReference.FindObject("MessageText").GetComponent<TMP_Text>();

            if(messageTextComponent != null && message != null && messageObjectReference != null)
            {
                //Signal On Screen Message with Acknowledge Button
                SignalOnScreenMessageWithButton(messageObjectReference, messageTextComponent, message);
            }
            else
            {
                Debug.LogWarning($"SignalOnScreenMessageFromPrefab: {logMessageName}: Could not find message object or message component.");
            }
        }
        public void SignalOnScreenMessageFromReference(ref GameObject messageObjectReference, string message, string logMessageName)
        {
            Debug.Log($"SignalOnScreenMessageFromReference: {logMessageName}: Signal On Screen Message.");

            //Find text component for on screen message
            TMP_Text messageTextComponent = messageObjectReference.FindObject("MessageText").GetComponent<TMP_Text>();

            if(messageTextComponent != null && message != null && messageObjectReference != null)
            {
                //Signal On Screen Message with Acknowledge Button
                SignalOnScreenMessageWithButton(messageObjectReference, messageTextComponent, message);
            }
            else
            {
                Debug.LogWarning($"SignalOnScreenMessageFromReference: {logMessageName}: Could not find message object or message component.");
            }
        }
        public void SignalOnScreenMessageWithButton(GameObject messageGameObject, TMP_Text messageComponent = null, string message = "None")
        {
            if (messageGameObject != null)
            {
                if(message != "None" && messageComponent != null)
                {
                    //Set Text
                    messageComponent.text = message;
                }

                //Set Object Active
                messageGameObject.SetActive(true);

                //Get Acknowledge button from the child of this panel
                GameObject AcknowledgeButton = messageGameObject.FindObject("AcknowledgeButton");

                //Check if this item already has a listner or not.
                if (AcknowledgeButton.GetComponent<Button>().onClick.GetPersistentEventCount() == 0)
                {
                    //Add Listner for Acknowledge Button
                    AcknowledgeButton.GetComponent<Button>().onClick.AddListener(() => messageGameObject.SetActive(false));
                }
                else
                {
                    Debug.LogWarning("ACKNOWLEDGE BUTTON SHOULD ALREADY HAVE A LISTNER THAT SETS IT TO FALSE.");
                }

            }
            else
            {
                Debug.LogWarning($"Message: Could not find message object or message component inside of GameObject {messageGameObject.name}.");
            }  
        }

        /////////////////////////////////////// Communication Buttons //////////////////////////////////////////////
        public void UpdateConnectionStatusText(GameObject connectionStatusObject, bool connectionStatus)
        {
            //Find the text component in the children
            TMP_Text connectionStatusText = connectionStatusObject.FindObject("StatusText").GetComponent<TMP_Text>();
            
            if(RosConnectionStatusObject == null)
            {
                Debug.LogWarning("ConnectionStatusText is null for " + connectionStatusObject.name);
            }

            //If connected set the text and color
            if(connectionStatus)
            {
                connectionStatusText.text = "CONNECTED";
                connectionStatusText.color = Color.green;
            }
            else
            {
                connectionStatusText.text = "DISCONNECTED";
                connectionStatusText.color = Color.red;
            }
        }
        public void UpdateMqttConnectionFromUserInputs()
        {
            //Set UI Color
            SetUIObjectColor(MqttConnectButtonObject, White);
            
            //Check inputs and if they are not null update the connection if they are null leave the default.
            string newMqttBroker = MqttBrokerInputField.text;
            if (string.IsNullOrWhiteSpace(newMqttBroker))
            {
                newMqttBroker = "broker.hivemq.com";
            }

            string newMqttPort = MqttPortInputField.text;
            if (string.IsNullOrWhiteSpace(newMqttPort))
            {
                newMqttPort = "1883";
            }

            //Check if the manual the port or broker is different then the current one.
            if (newMqttBroker != mqttTrajectoryManager.brokerAddress || Convert.ToInt32(newMqttPort) != mqttTrajectoryManager.brokerPort)
            {
                //Unsubscibe from events
                mqttTrajectoryManager.RemoveConnectionEventListners();

                //Unsubscribe from topics
                mqttTrajectoryManager.UnsubscribeFromCompasXRTopics();

                //Update Broker and Port to the user inputs
                mqttTrajectoryManager.brokerAddress = newMqttBroker;
                mqttTrajectoryManager.brokerPort = Convert.ToInt32(newMqttPort);

                //Disconnect from current broker
                mqttTrajectoryManager.DisconnectandReconnectAsyncRoutine();
            }
            else
            {
                Debug.Log("MQTT: Broker and Port are the same as the current one. Not updating connection.");
                
                //Signal Manual Input text
                MqttUpdateConnectionMessage.SetActive(true);

            }
        }
        public void UpdateRosConnectionFromUserInputs()
        {
            Debug.Log($"UpdateRosConnectionFromUserInputs: Attempting ROS Connection to ws://{RosHostInputField.text}:{RosPortInputField.text} from User Inputs.");
            
            //Set UI Color
            SetUIObjectColor(RosConnectButtonObject, White);
            
            //Check inputs and if they are not null update the connection if they are null leave the default.
            string rosHostInput = RosHostInputField.text;
            string rosPortInput = RosPortInputField.text;

            if (string.IsNullOrWhiteSpace(rosHostInput) || string.IsNullOrWhiteSpace(rosPortInput))
            {
                rosHostInput = "localhost";
                rosPortInput = "9090";
            }

            //Ross bridge connection address
            string newRosBridgeAddress = $"ws://{rosHostInput}:{rosPortInput}";

            Debug.Log("UpdateRosConnectionFromUserInputs: New ROS Bridge Address: " + newRosBridgeAddress);
            
            //Check if the manual the port or broker is different then the current one.
            if (newRosBridgeAddress != rosConnectionManager.RosBridgeServerUrl || !rosConnectionManager.IsConnectedToRos)
            {
                //If we are connected to ros then disconnect
                if(rosConnectionManager.IsConnectedToRos)
                {
                    //Disconnect from current ros bridge socket
                    rosConnectionManager.RosSocket.Close();
                }
                
                //Update rosBridgeServerUrl from the inputs
                rosConnectionManager.RosBridgeServerUrl = newRosBridgeAddress;

                //Disconnect from current broker
                rosConnectionManager.ConnectAndWait();
            }
            else
            {
                Debug.Log("UpdateRosConnectionFromUserInputs: ROS Host and Port are the same as our current and we are connected. Not updating connection.");
                
                //Signal Manual Input text
                RosUpdateConnectionMessage.SetActive(true);

            }
        }
        public void TrajectoryServicesUIControler(bool requestTrajectoryVisability, bool requestTrajectoryInteractable, bool trajectoryReviewVisibility, bool trajectoryReviewInteractable, bool executeTrajectoryVisability, bool executeTrajectoryInteractable)
        {
            //Set Visability and Interactable of Trajectory Request Button.
            RequestTrajectoryButtonObject.SetActive(requestTrajectoryVisability);
            RequestTrajectoryButtonObject.GetComponent<Button>().interactable = requestTrajectoryInteractable;

            //Set Visability of Trajectory Review objects and Interactable of Approval and Reject Buttons
            ReviewTrajectoryObjects.SetActive(trajectoryReviewVisibility);
            ApproveTrajectoryButtonObject.GetComponent<Button>().interactable = trajectoryReviewInteractable;
            RejectTrajectoryButtonObject.GetComponent<Button>().interactable = trajectoryReviewInteractable;

            //Set Visability and Interactable of Execute Trajectory Button.
            ExecuteTrajectoryButtonObject.SetActive(executeTrajectoryVisability);
            ExecuteTrajectoryButtonObject.GetComponent<Button>().interactable = executeTrajectoryInteractable;

            //Set interactablity of reject button if the exacute trajectory is interactable.
            if (executeTrajectoryInteractable)
            {
                RejectTrajectoryButtonObject.GetComponent<Button>().interactable = executeTrajectoryInteractable;
            }

            //Adjust interactibility of Robot toggle based on visibility of other services controls
            if ( trajectoryReviewVisibility || executeTrajectoryVisability)
            {
                //if trajectory approval or exacute trajectory is visible then robot toggle is not interactable
                RobotToggleObject.GetComponent<Toggle>().interactable = false;

                //Next and previous button not interactable based on service
                NextGeometryButtonObject.GetComponent<Button>().interactable = false;
                PreviousGeometryButtonObject.GetComponent<Button>().interactable = false;
                
            }
            else if (requestTrajectoryVisability)
            {
                //If request trajectory is visaible then robot toggle is interactable
                RobotToggleObject.GetComponent<Toggle>().interactable = true;

                //Next and previous button not interactable based on service
                NextGeometryButtonObject.GetComponent<Button>().interactable = true;
                PreviousGeometryButtonObject.GetComponent<Button>().interactable = true;
            }

        }
        public void RequestTrajectoryButtonMethod()
        {
            Debug.Log($"Request Trajectory Button Pressed: Requesting Trajectory for Step {CurrentStep}");

            if (mqttTrajectoryManager.serviceManager.TrajectoryRequestTransactionLock)
            {
                Debug.Log("RequestTrajectoryButtonMethod : You cannot request because transaction lock is active");

                //If the active robot is null signal On Screen Message
                // SignalOnScreenMessageWithButton(TransactionLockActiveWarningMessageObject);
                string message = "WARNING: You are currently prevented from requesting because another active user is awaiting a Trajectory Result.";
                SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref TransactionLockActiveWarningMessageObject, "TransactionLockActiveWarningMessage", MessagesParent, message, "RequestTrajectoryButtonMethod: Transaction Lock Active Warning.");
                
                return;
            }
            else if (trajectoryVisulizer.ActiveRobot == null)
            {
                Debug.Log("RequestTrajectoryButtonMethod : Active Robot is null");
            
                //If the active robot is null signal On Screen Message
                // SignalOnScreenMessageWithButton(ActiveRobotIsNullWarningMessageObject);
                string message = "WARNING: Active Robot is currently null. An active robot must be set before visulizing robotic information.";
                SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref ActiveRobotIsNullWarningMessageObject, "ActiveRobotNullWarningMessage", MessagesParent, message, "RequestTrajectoryButtonMethod: Active Robot is null.");

                return;
            }
            else
            {    
                //Publish new GetTrajectoryRequest message to the GetTrajectoryRequestTopic for CurrentStep
                mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.compasXRTopics.publishers.getTrajectoryRequestTopic, new GetTrajectoryRequest(CurrentStep, mqttTrajectoryManager.serviceManager.ActiveRobotName).GetData());

                //Set mqttTrajectoryManager.serviceManager.PrimaryUser to true && Set Current Service to GetTrajectory
                mqttTrajectoryManager.serviceManager.PrimaryUser = true;
                mqttTrajectoryManager.serviceManager.currentService = ServiceManager.CurrentService.GetTrajectory;

                //TODO: INCLUDE TIMEOUT FOR WAITING ON REPLY FROM CONTROLER.... THIS IS A BIT DIFFICULT BECAUSE I CANNOT PROVIDE CANCELATION LIKE OTHER MESSAGE.

                //Make the request button not interactable to prevent sending multiple requests.. Message Handler will set it back to true if trajectory is null.
                TrajectoryServicesUIControler(true, false, false, false, false, false);
            }
        }
        public void ApproveTrajectoryButtonMethod()
        {
            Debug.Log($"Approve Trajectory Button Pressed: Approving Trajectory for Step {CurrentStep}");
            
            //TODO: Put this here to prevent accidentally setting it if the message is too fast.
            //Make the approval and disapproval button not interactable to prevent sending multiple approvals and disapprovals.
            TrajectoryServicesUIControler(false, false, true, false, false, false);
            
            //Publish new ApproveTrajectoryMessage to the trajectory approval topic for current step
            mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.compasXRTopics.publishers.approveTrajectoryTopic, new ApproveTrajectory(CurrentStep, mqttTrajectoryManager.serviceManager.ActiveRobotName, mqttTrajectoryManager.serviceManager.CurrentTrajectory, 1).GetData());
        }
        public void RejectTrajectoryButtonMethod()
        {
            Debug.Log($"RejectTrajectoryButtonMethod: Rejecting Trajectory for Step {CurrentStep}");

            //Publish new ApproveTrajectoryMessage to the trajectory approval topic for current step
            mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.compasXRTopics.publishers.approveTrajectoryTopic, new ApproveTrajectory(CurrentStep, mqttTrajectoryManager.serviceManager.ActiveRobotName, mqttTrajectoryManager.serviceManager.CurrentTrajectory, 0).GetData());

            //Make the approval and disapproval button not interactable to prevent sending multiple approvals and disapprovals....
            TrajectoryServicesUIControler(false, false, true, false, false, false);
        }
        public void TrajectorySliderReviewMethod(float value)
        {
            //Check if MQTT Service Manager is not null or count is greater then 0
            if (mqttTrajectoryManager.serviceManager.CurrentTrajectory != null)
            {
                //double check that count is not 0
                if (mqttTrajectoryManager.serviceManager.CurrentTrajectory.Count > 0)
                {
                    //Remap input value to the count of the trajectory
                    float SliderValue = value;
                    int TrajectoryConfigurationsCount = mqttTrajectoryManager.serviceManager.CurrentTrajectory.Count; 
                    float SliderMax = 1;
                    float SliderMin = 0;
                    
                    float SliderValueRemaped = HelpersExtensions.Remap(SliderValue, SliderMin, SliderMax, 0, TrajectoryConfigurationsCount-1); 

                    //Print list item at the index of the remapped value //TODO: SERILIZE CONFIGURATION TO STRING SO YOU CAN READ IT.
                    Debug.Log($"Trajectory Review: Slider Value Changed is value {value} and the item is {JsonConvert.SerializeObject(mqttTrajectoryManager.serviceManager.CurrentTrajectory[(int)SliderValueRemaped])}"); //TODO:CHECK SLIDER REMAP

                    //Color Static Robot Image based on SliderRemapedValue
                    trajectoryVisulizer.ColorRobotConfigfromSliderInput((int)SliderValueRemaped, instantiateObjects.InactiveRobotMaterial, instantiateObjects.ActiveRobotMaterial,ref trajectoryVisulizer.previousSliderValue);
                }
                else
                {
                    Debug.Log("Trajectory Review: Current Trajectory Count is 0.");
                }
            }
            else
            {
                Debug.Log("Trajectory Review: Current Trajectory is null.");
            }
        }
        public void ExecuteTrajectoryButtonMethod()
        {
            Debug.Log($"Execute Trajectory Button Pressed: Executing Trajectory for Step {CurrentStep}");

            //Send Trajectory message as dictionary
            Dictionary<string, object> sendTrajectoryMessage = new SendTrajectory(CurrentStep, mqttTrajectoryManager.serviceManager.ActiveRobotName, mqttTrajectoryManager.serviceManager.CurrentTrajectory).GetData();
            Debug.Log("Send Trajectory Message: " + JsonConvert.SerializeObject(sendTrajectoryMessage));

            //Publish new SendTrajectoryMessage to the trajectory execution topic for current step
            mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.compasXRTopics.publishers.sendTrajectoryTopic, sendTrajectoryMessage);

            //Make the execute button not interactable to prevent sending multiple just a precaustion, should be handled by message handler anyway.
            TrajectoryServicesUIControler(false, false, false, false, true, false);

            //Publish new ApproveTrajectoryMessage for CONSENSUS APPROVAL
            mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.compasXRTopics.publishers.approveTrajectoryTopic, new ApproveTrajectory(CurrentStep, mqttTrajectoryManager.serviceManager.ActiveRobotName, mqttTrajectoryManager.serviceManager.CurrentTrajectory, 2).GetData());
        }

        ////////////////////////////////////// Visualizer Menu Buttons ////////////////////////////////////////////
        public void TogglePreviewActor(Toggle toggle)
        {
            Debug.Log("TogglePreviewActor: Preview Builder Toggle Pressed");

            if(toggle.isOn)
            {
                //Turn on the Preview Builder
                instantiateObjects.visulizationController.VisulizationMode = VisulizationMode.ActorView;
                
                //Apply color to the objects based on actor
                instantiateObjects.ApplyColorBasedOnActor();
                
                //Color the button if it is on
                SetUIObjectColor(PreviewActorToggleObject, Yellow);
            }
            else
            {
                //Turn off the Preview Builder
                instantiateObjects.visulizationController.VisulizationMode = VisulizationMode.BuiltUnbuilt;

                //Apply color to the objects based on actor
                instantiateObjects.ApplyColorBasedOnBuildState();

                //Color the button if it is off
                SetUIObjectColor(PreviewActorToggleObject, White);
            }
        }
        public void ToggleID(Toggle toggle)
        {
            Debug.Log("ID Toggle Pressed");

            if (toggle != null && IDToggleObject != null)
            {
                if(toggle.isOn)
                {
                    //Turn on the ID Tags
                    ARSpaceTextControler(true, "IdxText", ref IDTagIsOffset, "IdxImage", PriorityViewerToggleObject.GetComponent<Toggle>().isOn, 0.155f); //bool verticlReposition, float distance
                    
                    //Set color of toggle
                    SetUIObjectColor(IDToggleObject, Yellow);
                }
                else
                {
                    //Turn of the ID Tags
                    ARSpaceTextControler(false, "IdxText", ref IDTagIsOffset, "IdxImage");

                    //If Priority viewer is on and the tag is offset then position the tag back to the priority tags back to original position
                    if(PriorityViewerToggleObject.GetComponent<Toggle>().isOn && PriorityTagIsOffset)
                    {
                        ARSpaceTextControler(true, "PriorityText", ref PriorityTagIsOffset, "PriorityImage");
                    }

                    //Set color of toggle
                    SetUIObjectColor(IDToggleObject, White);
                }
            }
            else
            {
                Debug.LogWarning("Could not find ID Toggle or ID Toggle Object.");
            }
        }
        public void ARSpaceTextControler(bool Visibility, string textObjectBaseName, ref bool tagIsOffset, string imageObjectBaseName = null, bool verticalReposition = false, float? verticalOffset = null)
        {
            Debug.Log($"ARSpaceTextControler: Toggling Text Objects {textObjectBaseName}.");

            if (instantiateObjects != null && instantiateObjects.Elements != null)
            {

                foreach (Transform child in instantiateObjects.Elements.transform)
                {
                    // Toggle Text Object
                    Transform textChild = child.Find(child.name + textObjectBaseName);
                    if (textChild != null)
                    {
                        textChild.gameObject.SetActive(Visibility);
                    }

                    // If vertical reposition is true then reposition the text object
                    if (verticalReposition)
                    {
                        Vector3 objectposition = textChild.transform.position;
                        Vector3 newPosition = instantiateObjects.OffsetPositionVectorByDistance(objectposition, verticalOffset.GetValueOrDefault(0.0f), "y");
                        textChild.position = newPosition;
                    }
                    else
                    {
                        //Set the position of the text object to the original position
                        HelpersExtensions.ObjectPositionInfo instantiationPosition = textChild.GetComponent<HelpersExtensions.ObjectPositionInfo>();
                        textChild.localPosition = instantiationPosition.position;
                    }

                    if(imageObjectBaseName != null)
                    {
                        // Toggle background Image Object
                        Transform imageChild = child.Find(child.name + imageObjectBaseName);
                        if (imageChild != null)
                        {
                            imageChild.gameObject.SetActive(Visibility);
                        }

                        // If vertical reposition is true then reposition the text object
                        if (verticalReposition)
                        {
                            Vector3 objectposition = imageChild.transform.position;
                            Vector3 newPosition = instantiateObjects.OffsetPositionVectorByDistance(objectposition, verticalOffset.GetValueOrDefault(0.0f), "y");
                            imageChild.position = newPosition;
                            tagIsOffset = true;
                        }
                        else
                        {
                            //Set the position of the text object to the original position
                            HelpersExtensions.ObjectPositionInfo instantiationPosition = imageChild.GetComponent<HelpersExtensions.ObjectPositionInfo>();
                            imageChild.localPosition = instantiationPosition.position;
                            tagIsOffset = false;
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("InstantiateObjects script or Elements object not set.");
            }
        }
        public void ToggleObjectLengths(Toggle toggle)
        {
            Debug.Log("Object Lengths Toggle Pressed");

            if (ObjectLengthsUIPanelObjects != null && ObjectLengthsText != null && ObjectLengthsTags != null)
            {    
                if (toggle.isOn)
                {             
                    //If the robot toggle is on move the position lower
                    if (RobotToggleObject.GetComponent<Toggle>().isOn)
                    {
                        Vector3 offsetPosition = new Vector3(ObjectLengthsUIPanelPosition.x, ObjectLengthsUIPanelPosition.y - 300, ObjectLengthsUIPanelPosition.z);
                        ObjectLengthsUIPanelObjects.transform.localPosition = offsetPosition; 
                    }
                    else
                    {
                        ObjectLengthsUIPanelObjects.transform.localPosition = ObjectLengthsUIPanelPosition;
                    }
                    
                    //Set Visibility of buttons
                    ObjectLengthsUIPanelObjects.SetActive(true);
                    ObjectLengthsTags.FindObject("P1Tag").SetActive(true);
                    ObjectLengthsTags.FindObject("P2Tag").SetActive(true);

                    //Function to calculate distances to the ground and show them
                    if (CurrentStep != null)
                    {    
                        instantiateObjects.CalculateandSetLengthPositions(CurrentStep);
                    }
                    else
                    {
                        Debug.LogWarning("Current Step is null.");
                    }

                    //Set color of toggle
                    SetUIObjectColor(ObjectLengthsToggleObject, Yellow);

                }
                else
                {
                    //Set Visibility of buttons
                    ObjectLengthsUIPanelObjects.SetActive(false);
                    ObjectLengthsTags.FindObject("P1Tag").SetActive(false);
                    ObjectLengthsTags.FindObject("P2Tag").SetActive(false);

                    //Set color of toggle
                    SetUIObjectColor(ObjectLengthsToggleObject, White);
                }
            }
            else
            {
                Debug.LogWarning("Could not find Object Lengths Objects.");
            }
            
        }
        public void SetObjectLengthsText(float P1distance, float P2distance)
        {
            //Update Distance Text
            ObjectLengthsText.text = $"P1 | {(float)Math.Round(P1distance, 2)}     P2 | {(float)Math.Round(P2distance, 2)}";
        }
        public void ToggleRobot(Toggle toggle)
        {
            Debug.Log("Robot Toggle Pressed");

            if(toggle.isOn && RequestTrajectoryButtonObject != null)
            {
                //Set Visibility of Request Trajectory Button
                if(trajectoryVisulizer.ActiveRobot && CurrentStep != null)
                {
                    trajectoryVisulizer.ActiveRobot.SetActive(true);
                }
                else
                {
                    Debug.Log("ToggleRobot: Active Robot or CurrentStep is null not setting any visibility.");
                }

                //Set Robot Selection Objects to visible
                RobotSelectionDropdownObject.SetActive(true);
                SetActiveRobotToggleObject.SetActive(true);

                //Check current step data to set visibility and interactibility of request trajectory button.
                if(CurrentStep != null)
                {
                    //Set interaction based on current step.
                    SetRoboticUIElementsFromKey(CurrentStep);
                }
                else
                {
                    Debug.LogWarning("Current Step is null.");
                }
                
                //Set the color of the robot toggle to yellow.
                SetUIObjectColor(RobotToggleObject, Yellow);
            }
            else
            {            
                //If the request trajectory button is visable then set everything to not visable.
                if (RequestTrajectoryButtonObject.activeSelf)
                {
                    //Set Visibility of Request Trajectory Button
                    TrajectoryServicesUIControler(false, false, false, false, false, false);
                }
                            
                //Set Visibility of Robot Selection Objects
                RobotSelectionDropdownObject.SetActive(false);
                SetActiveRobotToggleObject.SetActive(false);
                            
                //Set Visibility of Robot.
                if(trajectoryVisulizer.ActiveRobotObjects.transform.childCount > 0)
                {
                    if(trajectoryVisulizer.ActiveRobot.activeSelf)
                    {
                        trajectoryVisulizer.ActiveRobot.SetActive(false);
                    }
                    else if(trajectoryVisulizer.ActiveTrajectoryParentObject.activeSelf) //TODO: SHOULD THIS DESTROY?
                    {
                        trajectoryVisulizer.ActiveTrajectoryParentObject.SetActive(false);
                    }
                }

                //Set the color of the Robot toggle button to white.
                SetUIObjectColor(RobotToggleObject, White);
            }
        }
        public void SetRoboticUIElementsFromKey(string key)
        {
            Step step = databaseManager.BuildingPlanDataItem.steps[key];

            //If step is a robot step then make the request button visible.
            if(step.data.actor == "ROBOT")
            {
                //If the step is not built and priority is current priority then make request button visible and interactable
                if (!step.data.is_built && step.data.priority.ToString() == databaseManager.CurrentPriority)
                {    
                    //Set Visibility of Request Trajectory Button, and interactability to true.
                    TrajectoryServicesUIControler(true, true, false, false, false, false);
                }
                else
                {
                    //Set visivility to true, but interactability to false.
                    TrajectoryServicesUIControler(true, false, false, false, false, false);
                }
            
            }
            else
            {
                //Set Robot Selection Objects to visible
                RobotSelectionDropdownObject.SetActive(true);
                SetActiveRobotToggleObject.SetActive(true);

                //Set Visibility of Request Trajectory Button
                TrajectoryServicesUIControler(false, false, false, false, false, false);
            }
        }
        public void TogglePriority(Toggle toggle)
        {
            Debug.Log("Priority Toggle Pressed");

            if(toggle.isOn && PriorityViewerToggleObject != null)
            {
                //Turn on Priority Tags in 3D Space
                ARSpaceTextControler(true, "PriorityText", ref PriorityTagIsOffset, "PriorityImage", IDToggleObject.GetComponent<Toggle>().isOn, 0.155f);

                //Set visibility of 3D reference objects
                instantiateObjects.PriorityViewrLineObject.SetActive(true);
                instantiateObjects.PriorityViewerPointsObject.SetActive(true);

                //Set visibility of onScreen Button Objects
                PriorityViewerObjectsGraphicsController(true, databaseManager.CurrentPriority);

                //Set Selected Priority
                SelectedPriority = databaseManager.CurrentPriority;

                //Create the priority line
                instantiateObjects.CreatePriorityViewerItems(databaseManager.CurrentPriority, ref instantiateObjects.PriorityViewrLineObject, Color.red, 0.02f, 0.10f, Color.red, instantiateObjects.PriorityViewerPointsObject);

                // Color Elements Based on Priority
                instantiateObjects.ApplyColorBasedOnPriority(databaseManager.CurrentPriority);

                //Set UI Color
                SetUIObjectColor(PriorityViewerToggleObject, Yellow);
            }
            else
            {
                
                //Color Elements by visulization mode
                if(instantiateObjects.visulizationController.VisulizationMode == VisulizationMode.ActorView)
                {
                    instantiateObjects.ApplyColorBasedOnActor();
                }
                else if(instantiateObjects.visulizationController.VisulizationMode == VisulizationMode.BuiltUnbuilt)
                {
                    instantiateObjects.ApplyColorBasedOnBuildState();
                }
                else
                {
                    Debug.LogWarning("Could not find Visulization Mode.");
                }

                //Set visibility of reference objects
                instantiateObjects.PriorityViewrLineObject.SetActive(false);
                instantiateObjects.PriorityViewerPointsObject.SetActive(false);

                //Set Selected Priority
                SelectedPriority = "None";

                //Turn off Priority Tags
                ARSpaceTextControler(false, "PriorityText", ref PriorityTagIsOffset, "PriorityImage");

                //If the ID tag is on and offset then return it to its origial position
                if(IDToggleObject.GetComponent<Toggle>().isOn && IDTagIsOffset)
                {
                    ARSpaceTextControler(true, "IdxText", ref IDTagIsOffset, "IdxImage");
                }

                //Set visibility of onScreen Button Objects
                PriorityViewerObjectsGraphicsController(false);

                //Set UI Color
                SetUIObjectColor(PriorityViewerToggleObject, White);
            }
        }
        public void PriorityViewerObjectsGraphicsController(bool? isVisible, string selectedPrioritytext=null)
        {
            //Set Visibility of Priority on screen button objects.
            if(isVisible.HasValue)
            {
                NextPriorityButtonObject.SetActive(isVisible.Value);
                PreviousPriorityButtonObject.SetActive(isVisible.Value);
                SelectedPriorityTextObject.SetActive(isVisible.Value);
                PriorityViewerBackground.SetActive(isVisible.Value);
            }

            //Set Text of Selected Priority if it is not null.
            if(selectedPrioritytext != null)
            {
                SelectedPriorityText.text = selectedPrioritytext;
            }
        }
        public void SetNextPriorityGroup()
        {
            Debug.Log("SetNextPriorityGroup: Next Priority Button Pressed");

            //Set the text of the priority viewer to the next priority
            if(SelectedPriority != "None")
            {
                //Convert the selected priority to an int
                int SelectedPriorityInt = Convert.ToInt16(SelectedPriority);
                int newPriorityGroupInt = SelectedPriorityInt + 1;

                //Check if the next priority is not null
                if(newPriorityGroupInt <= databaseManager.PriorityTreeDict.Count - 1)
                {                
                    // Color previous elements of priority back to the original color
                    instantiateObjects.ApplyColortoPriorityGroup(SelectedPriorityInt.ToString(), newPriorityGroupInt.ToString());
                    
                    //Create the priority line
                    instantiateObjects.CreatePriorityViewerItems(newPriorityGroupInt.ToString(), ref instantiateObjects.PriorityViewrLineObject, Color.red, 0.02f, 0.10f, Color.red, instantiateObjects.PriorityViewerPointsObject);

                    //Color elements of new priority
                    instantiateObjects.ApplyColortoPriorityGroup(newPriorityGroupInt.ToString(), newPriorityGroupInt.ToString(), true);

                    //Set the text with the graphics controler
                    PriorityViewerObjectsGraphicsController(true, newPriorityGroupInt.ToString());

                    //Set the next priority
                    SelectedPriority = (SelectedPriorityInt + 1).ToString();
                }
                else
                {
                    Debug.Log("SetNextPriorityGroup: We have reached the priority groups limit.");
                }
            }
            else
            {
                Debug.LogWarning("SetNextPriorityGroup: Selected Priority is null.");
            }
        }
        public void SetPreviousPriorityGroup()
        {
            Debug.Log("SetPreviousPriorityGroup: Next Priority Button Pressed");

            //Set the text of the priority viewer to the next priority
            if(SelectedPriority != "None")
            {
                //Convert the selected priority to an int
                int SelectedPriorityInt = Convert.ToInt16(SelectedPriority);
                int newPriorityGroupInt = SelectedPriorityInt - 1;

                //Check if the next priority is not null
                if(newPriorityGroupInt >= 0)
                {
                    // Color previous elements of priority back to the original color
                    instantiateObjects.ApplyColortoPriorityGroup(SelectedPriorityInt.ToString(), newPriorityGroupInt.ToString());
                    
                    //Create the priority line
                    instantiateObjects.CreatePriorityViewerItems(newPriorityGroupInt.ToString(), ref instantiateObjects.PriorityViewrLineObject, Color.red, 0.02f, 0.10f, Color.red, instantiateObjects.PriorityViewerPointsObject);

                    //Color elements of new priority
                    instantiateObjects.ApplyColortoPriorityGroup(newPriorityGroupInt.ToString(), newPriorityGroupInt.ToString(), true);

                    //Set the text with the graphics controler
                    PriorityViewerObjectsGraphicsController(true, newPriorityGroupInt.ToString());

                    //Set the next priority
                    SelectedPriority = (SelectedPriorityInt - 1).ToString();
                }
                else
                {
                    Debug.Log("SetPreviousPriorityGroup: We have reached the zero priority group.");
                }
            }
            else
            {
                Debug.LogWarning("SetPreviousPriorityGroup: Selected Priority is null.");
            }
        }
        public void ToggleScrollSearch(Toggle toggle)
        {
            if (toggle.isOn)
            {             
                //Set Visibility of buttons
                ScrollSearchObjects.SetActive(true);

                //Create Cells for the Scroll Search
                scrollSearchManager.CreateCellsFromPrefab(ref scrollSearchManager.cellPrefab, scrollSearchManager.cellSpacing, scrollSearchManager.cellsParent, databaseManager.BuildingPlanDataItem.steps.Count, ref scrollSearchManager.cellsExist);
                
                //Set color of toggle
                SetUIObjectColor(ScrollSearchToggleObject, Yellow);
            }
            else
            {
                //Set Visibility of buttons
                ScrollSearchObjects.SetActive(false);

                //Reset all of the information in the cell search to off
                scrollSearchManager.ResetScrollSearch(ref scrollSearchManager.cellsExist);
                
                //Set color of toggle
                SetUIObjectColor(ScrollSearchToggleObject, White);
            }
        }

        ////////////////////////////////////////// Menu Buttons ///////////////////////////////////////////////////
        private void ToggleInfo(Toggle toggle)
        {
            if(InfoPanelObject != null)
            {
                Debug.Log("Info Toggle Pressed");

                if (toggle.isOn)
                {             
                    //Check if communication toggle is on and if it is turn it off
                    if(CommunicationToggleObject.GetComponent<Toggle>().isOn)
                    {
                        CommunicationToggleObject.GetComponent<Toggle>().isOn = false;
                    }
                    
                    //Set Visibility of Information panel
                    InfoPanelObject.SetActive(true);

                    //Set color of toggle
                    SetUIObjectColor(InfoToggleObject, Yellow);

                }
                else
                {
                    //Set Visibility of Information panel
                    InfoPanelObject.SetActive(false);

                    //Set color of toggle
                    SetUIObjectColor(InfoToggleObject, White);
                }
            }
            else
            {
                Debug.LogWarning("Could not find Info Panel.");
            }
        }
        private void ToggleCommunication(Toggle toggle)
        {
            if(CommunicationPanelObject != null)
            {
                Debug.Log("Communication Toggle Pressed");

                if (toggle.isOn)
                {             
                    //Check if info toggle is on and if it is turn it off
                    if(InfoToggleObject.GetComponent<Toggle>().isOn)
                    {
                        InfoToggleObject.GetComponent<Toggle>().isOn = false;
                    }

                    //Set Visibility of Information panel
                    CommunicationPanelObject.SetActive(true);

                    //Update Connection Status Objects
                    UpdateConnectionStatusText(MqttConnectionStatusObject, mqttTrajectoryManager.mqttClientConnected);
                    UpdateConnectionStatusText(RosConnectionStatusObject, rosConnectionManager.IsConnectedToRos);


                    //Set color of toggle
                    SetUIObjectColor(CommunicationToggleObject, Yellow);

                }
                else
                {
                    //if the update connection message is on turn it off
                    if(MqttUpdateConnectionMessage.activeSelf)
                    {
                        MqttUpdateConnectionMessage.SetActive(false);
                    }
                    
                    //Set Visibility of Information panel
                    CommunicationPanelObject.SetActive(false);

                    //Set color of toggle
                    SetUIObjectColor(CommunicationToggleObject, White);
                }
            }
            else
            {
                Debug.LogWarning("Could not find Communication Panel.");
            }
        }
        private void ReloadApplication()
        {
            Debug.Log("ReloadApplication: Reload Button Pressed");
            
            //Remove listners - This is important to not add multiple listners in the application
            databaseManager.RemoveListners();

            //Clear all elements in the scene
            if (Elements.transform.childCount > 0)
            {
                foreach (Transform child in Elements.transform)
                {
                    Destroy(child.gameObject);
                }
            }

            //Put all QR Markers back to Origin Location
            if (QRMarkers.transform.childCount > 0)
            {        
                foreach (Transform child in QRMarkers.transform)
                {
                    child.transform.position = Vector3.zero;
                    child.transform.rotation = Quaternion.identity;
                }
            }

            //Clear all User Current Step Objects if there are some
            if (UserObjects.transform.childCount > 0)
            {
                foreach (Transform child in UserObjects.transform)
                {
                    Destroy(child.gameObject);
                }
            }        

            //Clear all dictionaries
            databaseManager.BuildingPlanDataItem.steps.Clear();
            databaseManager.AssemblyDataDict.Clear();
            databaseManager.QRCodeDataDict.Clear();
            databaseManager.UserCurrentStepDict.Clear();
            databaseManager.PriorityTreeDict.Clear();

            //Unsubscribe from topics
            mqttTrajectoryManager.UnsubscribeFromCompasXRTopics();

            //Unsubscibe from connection events
            mqttTrajectoryManager.RemoveConnectionEventListners();

            //Fetch settings data again
            databaseManager.FetchSettingsData(eventManager.settings_reference); //TODO: Should I await this?

            //Disconnect from MQTT and reconnect after new application settings are received.
            mqttTrajectoryManager.DisconnectandReconnectAsyncRoutine();

        }
        public void ToggleEditor(Toggle toggle)
        {
            if (EditorBackground != null && BuilderEditorButtonObject != null && BuildStatusButtonObject != null)
            {    
                Debug.Log("Editor Toggle Pressed");
                
                if (toggle.isOn)
                {             
                    //Set Visibility of buttons
                    EditorBackground.SetActive(true);
                    BuilderEditorButtonObject.SetActive(true);
                    BuildStatusButtonObject.SetActive(true);

                    //Set visibility of on screen text
                    CurrentStepTextObject.SetActive(false);
                    EditorSelectedTextObject.SetActive(true);

                    //Update mode so we know to search for touch input
                    TouchSearchModeController(TouchMode.ElementEditSelection);
                    
                    //Set color of toggle
                    SetUIObjectColor(EditorToggleObject, Yellow);

                }
                else
                {
                    //Set Visibility of buttons
                    EditorBackground.SetActive(false);
                    BuilderEditorButtonObject.SetActive(false);
                    BuildStatusButtonObject.SetActive(false);

                    //Set visibility of on screen text
                    EditorSelectedTextObject.SetActive(false);
                    CurrentStepTextObject.SetActive(true);

                    //Update mode so we are no longer searching for touch
                    TouchSearchModeController(TouchMode.None);

                    //Color Elements by visulization mode
                    if(instantiateObjects.visulizationController.VisulizationMode == VisulizationMode.ActorView)
                    {
                        instantiateObjects.ApplyColorBasedOnActor();
                    }
                    else if(instantiateObjects.visulizationController.VisulizationMode == VisulizationMode.BuiltUnbuilt)
                    {
                        instantiateObjects.ApplyColorBasedOnBuildState();
                    }
                    else if(PriorityViewerToggleObject.GetComponent<Toggle>().isOn)
                    {
                        instantiateObjects.ApplyColorBasedOnPriority(SelectedPriority);
                    }
                    else
                    {
                        Debug.LogWarning("Could not find Visulization Mode.");
                    }

                    //Set color of toggle
                    SetUIObjectColor(EditorToggleObject, White);
                }
            }
            else
            {
                Debug.LogWarning("Could not find one of the buttons in the Editor Menu.");
            }  
        }
        public void ToggleAROcclusion(Toggle toggle)
        {
            if (OcclusionToggleObject != null && occlusionManager != null)
            {
                Debug.Log("Occlusion Toggle Pressed");

                if (toggle.isOn)
                { 
                    //Enable Occlusion
                    occlusionManager.enabled = true;            

                    //Set color of the toggle
                    SetUIObjectColor(OcclusionToggleObject, Yellow);
                }
                else
                {
                    //Disable Occlusion Manager
                    occlusionManager.enabled = false;

                    //Set color of the toggle
                    SetUIObjectColor(OcclusionToggleObject, White);            
                }
            }
            else
            {
                Debug.LogWarning("Could not find Occlusion Toggle Object.");
            }
        }

        ////////////////////////////////////////// Editor Buttons /////////////////////////////////////////////////
        public void TouchSearchModeController(TouchMode modetype)
        {
            //Set Touch Mode
            instantiateObjects.visulizationController.TouchMode = modetype;

            // If input mode type is ElementEditSelection then we know to search for touch input on objects
            if (modetype == TouchMode.ElementEditSelection)
            {
                Debug.Log ("***TouchMode: ELEMENT EDIT MODE***");
            }

            // If input mode type is None then fix all elements for touch selection.
            else if(modetype == TouchMode.None)
            {
                Debug.Log ("***TouchMode: NONE***");

                //Destroy active bounding box
                DestroyBoundingBoxFixElementColor();
                activeGameObject = null;
                Debug.Log ("***TouchMode: NONE***");
            }

            else
            {
                Debug.LogWarning("Could not find Touch Mode.");
            }

        }
        private void TouchSearchControler()
        {
            if (instantiateObjects.visulizationController.TouchMode == TouchMode.ElementEditSelection)
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
                GameObject element = Elements.FindObject(i.ToString()).FindObject(step.data.element_ids[0] + " Geometry");
                Collider ElementCollider = element.FindObject(step.data.element_ids[0] + " Geometry").GetComponent<Collider>();
                Renderer ElementRenderer = element.FindObject(step.data.element_ids[0] + " Geometry").GetComponent<Renderer>();

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
                        Debug.Log ("TOUCH: Your hits count is greater then 0" + hits.Count);

                        if (Physics.Raycast(ray, out hitObject))
                        {
                            if (hitObject.collider.tag != "plane")
                            {
                                Debug.Log("TOUCH: I HIT SOMETHING ");
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

                    if (instantiateObjects.visulizationController.TouchMode == TouchMode.ElementEditSelection) // EDIT MODE
                    {
                        Debug.Log("*** ELEMENT SELECTION MODE : Editor ***");
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
                    if (instantiateObjects.visulizationController.TouchMode == TouchMode.ElementEditSelection) //EDIT MODE
                    {
                        Debug.Log("*** ELEMENT SELECTION MODE: Touch ***");
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
            if (HelpersExtensions.IsPointerOverUIObject(touchPosition))
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
                //Set On screen text object to the correct name
                EditorSelectedText.text = activeGameObject.transform.parent.name;
                
                //Set temporary object as the active game object
                temporaryObject = activeGameObject;

                //String name of the parent object to find step element in the dictionary
                string activeGameObjectParentname = activeGameObject.transform.parent.name;

                Debug.Log($"Active Game Object Priority: {databaseManager.BuildingPlanDataItem.steps[activeGameObjectParentname].data.priority}");
                
                //Color the object based on human or robot
                instantiateObjects.ColorHumanOrRobot(databaseManager.BuildingPlanDataItem.steps[activeGameObjectParentname].data.actor, databaseManager.BuildingPlanDataItem.steps[activeGameObjectParentname].data.is_built, activeGameObject);
                
                //Add Bounding Box for Object
                addBoundingBox(temporaryObject);
            }

            else
            {
                Debug.Log("ACTIVE GAME OBJECT IS NULL");
                if (GameObject.Find("BoundingArea") != null)
                {
                    DestroyBoundingBoxFixElementColor();
                }
            }
        }
        private void addBoundingBox(GameObject gameObj)
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
                GameObject elementGameobject = Box.transform.parent.gameObject;

                if (element != null && elementGameobject != null)
                {
                    if (CurrentStep != null)
                    {
                        if (element.name != CurrentStep)
                        {
                            //Find Step in the dictionary
                            Step step = databaseManager.BuildingPlanDataItem.steps[element.name];
                            
                            if(step != null)
                            {
                                //color object based on visulization mode.
                                instantiateObjects.ObjectColorandTouchEvaluater(instantiateObjects.visulizationController.VisulizationMode, instantiateObjects.visulizationController.TouchMode, step, element.name, elementGameobject.FindObject(step.data.element_ids[0] + " Geometry"));                        
                            }
                            else
                            {
                                Debug.LogWarning("Fix Element Color: Step is null.");
                            }                        
                        }
                    }

                }

                Destroy(GameObject.Find("BoundingArea"));
            }

        }
        public void ModifyStepActor(string key)
        {
            Debug.Log($"Modifying Actor of: {key}");

            //Find the step in the dictoinary
            Step step = databaseManager.BuildingPlanDataItem.steps[key];

            //Change Build Status
            if(step.data.actor == "HUMAN")
            {
                //Change Builder
                step.data.actor = "ROBOT";
            }
            else
            {
                //Change Builder
                step.data.actor = "HUMAN";
            }

            //Update color
            instantiateObjects.ColorHumanOrRobot(step.data.actor, step.data.is_built, Elements.FindObject(key).FindObject(step.data.element_ids[0] + " Geometry"));
            

            //Push Data to the database
            databaseManager.PushAllDataBuildingPlan(key);
        }
        private void TouchModifyBuildStatus()
        {
            Debug.Log("Build Status Button Pressed");
            
            if (activeGameObject != null)
            {
                ModifyStepBuildStatus(activeGameObject.transform.parent.name);
            }
        }
        private void TouchModifyActor()
        {
            Debug.Log("Actor Modifier Button Pressed");
            
            if (activeGameObject != null)
            {
                ModifyStepActor(activeGameObject.transform.parent.name);
            }
        }

    }
}

