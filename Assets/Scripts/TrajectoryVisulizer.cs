using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSON;
using MQTTDataCompasXR;
using Helpers;
using RosSharp.RosBridgeClient;
using Newtonsoft.Json;
using Unity.VisualScripting;
using Vuforia;
using Instantiate;
using RosSharp.Urdf;


public class TrajectoryVisulizer : MonoBehaviour
{
    //Other script objects
    private InstantiateObjects instantiateObjects;
    private MqttTrajectoryManager mqttTrajectoryManager;
    private UIFunctionalities uiFunctionalities;

    //GameObjects for storing the active robot objects in the scene
    public GameObject ActiveRobotObjects;
    public GameObject ActiveRobot;
    public GameObject ActiveTrajectory;
    private GameObject BuiltInRobotsParent;
    
    //List for storing the joint names of the active robot...
    //TODO: Ideally I could send this in the message as a dictionary so it is easier to find and more flexible, but for some reason joint names on CAD and in Unity do not match.
    public List<string> JointNames; //TODO: UPDATE THIS TO A DICT OF JOINT NAMES AND VALUES.
    public Dictionary<string, string> URDFLinkNames = new Dictionary<string, string>();
    public int? previousSliderValue;
    public Dictionary<string, string> URDFRenderComponents = new Dictionary<string, string>();

    //List of available robots
    public List<string> RobotURDFList = new List<string> {"UR3", "UR5", "UR10e", "ETHZurichRFL"};
        
    // Start is called before the first frame update
    void Start()
    {
        OnStartInitilization();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    ////////////////////////////////////////// Initilization & Selection //////////////////////////////////////////
    private void OnStartInitilization()
    {
        //Find Objects for retreiving and storing the active robots in the scene
        instantiateObjects = GameObject.Find("Instantiate").GetComponent<InstantiateObjects>();
        uiFunctionalities = GameObject.Find("UIFunctionalities").GetComponent<UIFunctionalities>();
        mqttTrajectoryManager = GameObject.Find("MQTTTrajectoryManager").GetComponent<MqttTrajectoryManager>();
        BuiltInRobotsParent = GameObject.Find("RobotPrefabs");
        ActiveRobotObjects = GameObject.Find("ActiveRobotObjects");
    }
    public void SetActiveRobotFromDropdown(string robotName, bool yRotation, bool visibility = true)
    {
        //Clear data objects from the previous robot //TODO: NEED TO CLEAR URDF LINK NAMES AND RENDER COMPONENTS.
        if(URDFLinkNames.Count > 0)
        {
            URDFLinkNames.Clear();
        }
        if(URDFRenderComponents.Count > 0)
        {
            URDFRenderComponents.Clear();
        }
        
        //Get the joint names for the active robot
        // JointNames = AddJointNamesList(robotName , JointNames);

        // Debug.Log("jointNames: " + JointNames.Count);
        // Debug.Log("joint name serilized:" + JsonConvert.SerializeObject(JointNames));

        //Instantiate the active robot in the ActiveRobotObjectsParent
        SetActiveRobot(BuiltInRobotsParent, robotName, yRotation, ActiveRobotObjects, ref ActiveRobot, ref ActiveTrajectory, instantiateObjects.InactiveRobotMaterial, visibility);
    }
    private void SetActiveRobot(GameObject BuiltInRobotsParent, string robotName, bool yRotation, GameObject ActiveRobotObjectsParent, ref GameObject ActiveRobot, ref GameObject ActiveTrajectory, Material material, bool visibility)
    {
        //Set the active robot in the scene
        GameObject selectedRobot = BuiltInRobotsParent.FindObject(robotName);

        if(selectedRobot != null)
        {
            //If the current elements exist then destroy them.
            if(ActiveRobot != null)
            {
                Destroy(ActiveRobot);
            }
            if(ActiveTrajectory != null)
            {
                Destroy(ActiveTrajectory);
            }
            
            //Instantiate a new robot object in the ActiveRobotObjectsParent //TODO: CHECK THIS.
            GameObject temporaryRobot = Instantiate(selectedRobot, ActiveRobotObjectsParent.transform.position, ActiveRobotObjectsParent.transform.rotation);

            //If extra rotation is needed then rotate the URDF.
            if(yRotation)
            {
                temporaryRobot.transform.Rotate(0, 90, 0); //TODO: CHECK THIS.
            }

            //Create the active robot parent object and Active trajectory 
            ActiveRobot = Instantiate(new GameObject(), ActiveRobotObjectsParent.transform.position, ActiveRobotObjectsParent.transform.rotation);
            ActiveRobot.name = "ActiveRobot";
            ActiveRobot.transform.SetParent(ActiveRobotObjectsParent.transform);
            ActiveTrajectory = Instantiate(new GameObject(), ActiveRobot.transform.position, ActiveRobot.transform.rotation);
            ActiveTrajectory.name = "ActiveTrajectory";
            ActiveTrajectory.transform.SetParent(ActiveRobotObjectsParent.transform);

            //Updating Service Manager with My active Robot Name
            mqttTrajectoryManager.serviceManager.ActiveRobotName = robotName;

            //Set temporary Robots parent to the ActiveRobot.
            temporaryRobot.transform.SetParent(ActiveRobot.transform);

            //Color the active robot
            ColorRobot(temporaryRobot, material, ref URDFRenderComponents);
        
            //Set the active robot visibility.
            temporaryRobot.SetActive(visibility);
        }
        else
        {
            Debug.Log($"SetActiveRobot: Robot {robotName} not found in the BuiltInRobotsParent.");
            uiFunctionalities.SignalOnScreenMessageWithButton(uiFunctionalities.ActiveRobotCouldNotBeFoundWarningMessage);
        }
    }

    ////////////////////////////////////////// Object Management /////// /////////////////////////////////////////////////////
    public void InstantiateRobotTrajectory(List<Dictionary<string, float>> TrajectoryConfigs, Frame robotBaseFrame, string trajectoryID, GameObject robotToConfigure, Dictionary<string, string> URDFLinks, GameObject parentObject, bool visibility) //TODO: THIS COULD POSSIBLY BE A DICT OF CONFIGS w/ JOINT NAMES.
    {
        Debug.Log($"InstantiateRobotTrajectory: For {trajectoryID} with {TrajectoryConfigs.Count} configurations.");
        
        if (TrajectoryConfigs.Count > 0 && robotToConfigure != null && URDFLinks.Count > 0 || parentObject != null)
        {
            //Get the number of configurations in the trajectory
            int trajectoryCount = TrajectoryConfigs.Count;

            //Find the parent object for holding trajectory Objects
            for (int i = 0; i < trajectoryCount; i++)
            {
                //Instantiate a new robot object in the ActiveRobotObjectsParent
                GameObject temporaryRobot = Instantiate(robotToConfigure, robotToConfigure.transform.position, robotToConfigure.transform.rotation);
                
                //Set the position of the robot by the included robot baseframe
                SetRobotPositionandRotation(robotBaseFrame, temporaryRobot);
                temporaryRobot.name = $"Config {i}";

                //Visulize the robot configuration
                SetRobotConfigfromDictWrapper(TrajectoryConfigs[i], $"Config {i}", temporaryRobot, ref URDFLinkNames); //TODO: CONVERT THIS TO A CUSTOM ACTION.

                //Set temporary Robots parent to the ActiveRobot.
                temporaryRobot.transform.SetParent(parentObject.transform);
                temporaryRobot.SetActive(visibility);
            }
        }
        else
        {
            //TODO: THIS SHOULD BE AN ERROR MESSAGE INSTEAD.
            Debug.Log("VisulizeRobotTrajectory: Trajectory is empty, robotToConfigure is null, or joint_names is empty.");
        }
    }
    public void VisulizeRobotTrajectory(List<Dictionary<string, float>> TrajectoryConfigs, Frame robotBaseFrame, string trajectoryID, GameObject robotToConfigure, List<string> joint_names, GameObject parentObject, bool visibility)
    {
        Debug.Log($"VisulizeRobotTrajectory: For {trajectoryID} with {TrajectoryConfigs.Count} configurations.");
        //If the child is not active for some reason, activate it.
        if(!ActiveRobot.transform.GetChild(0).gameObject.activeSelf)
        {
            ActiveRobot.transform.GetChild(0).gameObject.SetActive(true);
        }
        
        //Set active robot visibility to false and visualize the trajectory from the message
        ActiveRobot.SetActive(false);

        //Visulize the robot trajectory
        InstantiateRobotTrajectory(TrajectoryConfigs, robotBaseFrame, trajectoryID, robotToConfigure, URDFLinkNames, parentObject, visibility);  
    }
    public void DestroyActiveRobotObjects()
    {
        //Destroy the active robot in the scene
        if(ActiveRobot != null)
        {
            Destroy(ActiveRobot);
        }
        if(ActiveTrajectory != null)
        {
            Destroy(ActiveTrajectory);
        }
    }
    public void DestroyActiveTrajectoryChildren()
    {
        //Destroy the active robot in the scene
        if(ActiveTrajectory != null)
        {
            foreach (Transform child in ActiveTrajectory.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    ////////////////////////////////////////// Position, Rotation, & Configuration ////////////////////////////////////////////
    public void SetRobotConfigfromDictWrapper(Dictionary<string, float> config, string configName, GameObject robotToConfigure,ref Dictionary<string, string> urdfLinkNames) //TODO: THIS COULD POSSIBLY BE A DICT OF CONFIGS w/ JOINT NAMES.
    {
        Debug.Log($"SetRobotConfigfromDictWrapper: Visulizing robot configuration for gameObject {robotToConfigure.name}.");
        
        //If the URDFLinkNames are not found, find them.
        if (urdfLinkNames.Count == 0)
        {
            FindLinkNames(robotToConfigure.transform, config, ref urdfLinkNames);
        }            
        
        //Check if the config structure matches the URDF structure
        if(ConfigJointsEqualURDFLinks(config, ref urdfLinkNames))
        {
            //Find the parent object for holding trajectory Objects
            SetRobotConfigfromDict(config, robotToConfigure, urdfLinkNames);
        }
        else
        {
            //If the warning message is not active, signal the warning message that the config structure does not match the URDF Structure.
            if(uiFunctionalities.ConfigDoesNotMatchURDFStructureWarningMessageObject.activeSelf == false)
            {
                string message = $"WARNING: {configName} structure does not match the URDF structure and will not be visulized.";
                uiFunctionalities.SignalOnScreenMessageFromReference(ref uiFunctionalities.ConfigDoesNotMatchURDFStructureWarningMessageObject, message, "SetRobotConfigfromDictWrapper");
            }
            Debug.LogWarning($"SetRobotConfigfromDictWrapper: Config dict {config.Count} (Count) and LinkNames dict {urdfLinkNames.Count} (Count) for search do not match.");
        }
    }
    public void SetRobotConfigfromDict(Dictionary<string, float> config, GameObject robotToConfigure, Dictionary<string, string> linkNames)
    {
        Debug.Log($"SetRobotConfigFromDict: Visulizing robot configuration for gameObject {robotToConfigure.name}.");    

        //Find the parent object for holding trajectory Objects
        foreach (KeyValuePair<string, float> jointDescription in config)
        {
            //Get the name of the joint, value, and URDFLinkName
            string jointName = jointDescription.Key;
            float jointValue = jointDescription.Value;
            string urdfLinkName = linkNames[jointName];

            //Find the joint object in the robotToConfigure
            GameObject urdfLinkObject = robotToConfigure.FindObject(urdfLinkName);

            if (urdfLinkObject)
            {
                //Get the jointStateWriter component from the joint.
                JointStateWriter jointStateWriter = urdfLinkObject.GetComponent<JointStateWriter>();
                // UrdfJoint urdfJoint = joint.GetComponent<UrdfJoint>();
            
                //If the jointStateWriter is not found, add it to the joint.
                if (!jointStateWriter)
                {
                    jointStateWriter = urdfLinkObject.AddComponent<JointStateWriter>();    
                }
                
                //Write the joint value to the joint.
                jointStateWriter.Write(jointValue);
            }  
            else
            {
                Debug.LogWarning($"SetRobotConfigfromDict: URDF Link {name} not found in the robotToConfigure.");
            }
        }

    }
    public void SetRobotConfigfromList(List<float> config, GameObject robotToConfigure, List<string> jointNames)
    {
        Debug.Log($"SetRobotConfigFromList: Visulizing robot configuration for gameObject {robotToConfigure.name}.");
        
        //Get the number of joints in the list
        int configCount = config.Count;

        //Find the parent object for holding trajectory Objects
        for (int i = 0; i < configCount; i++) //TODO: LOOP THROUGH THE JOINTS OF THE URDF BY NAME.
        {
            GameObject joint = robotToConfigure.FindObject(jointNames[i]); //TODO: FIND OBJECT WITH A SPECIFIC JOINT NAME FROM THIS URDF.

            if (joint)
            {
                //Get the jointStateWriter component from the joint.
                JointStateWriter jointStateWriter = joint.GetComponent<JointStateWriter>();
                UrdfJoint urdfJoint = joint.GetComponent<UrdfJoint>();
                Debug.Log($"VisulizeRobotConfig: URDF Joint of TYPE {urdfJoint.JointType} COMPONENT FOUND FOR NAME {urdfJoint.JointName} found in the robotToConfigure.");
                
                //If the jointStateWriter is not found, add it to the joint.
                if (!jointStateWriter)
                {
                    jointStateWriter = joint.AddComponent<JointStateWriter>();    
                }
                
                //Write the joint value to the joint.
                jointStateWriter.Write(config[i]);
            }  
            else
            {
                Debug.Log($"VisulizeRobotConfig: Joint {name} not found in the robotToConfigure.");
            }
        }

    }
    public void SetRobotPositionandRotation(Frame robotBaseFrame, GameObject robotToPosition)
    {
        Debug.Log($"SetRobotPosition: Setting the robot {robotToPosition.name} to position and rotation from robot baseframe.");

        //Fetch position data from the dictionary
        Vector3 positionData = instantiateObjects.getPosition(robotBaseFrame.point);

        //Fetch rotation data from the dictionary
        InstantiateObjects.Rotation rotationData = instantiateObjects.getRotation(robotBaseFrame.xaxis, robotBaseFrame.yaxis);
        
        //Convert Firebase rotation data to Quaternion rotation. Additionally
        Quaternion rotationQuaternion = instantiateObjects.FromUnityRotation(rotationData);

        //Set the local position and rotation of the active robot, so it it is in relation to the robot base frame and its parent object.
        robotToPosition.transform.localPosition = positionData;
        robotToPosition.transform.localRotation = rotationQuaternion;
    }

    ////////////////////////////////////////// Color and Visiblity ////////////////////////////////////////////////////////////
    public void ColorRobotConfigfromSliderInput(int sliderValue, Material inactiveMaterial, Material activeMaterial, ref int? previousSliderValue)
    {
        Debug.Log($"ColorRobotConfigfromSlider: Coloring robot config {sliderValue} for active trajectory.");
        //If the previous version is not null find it and color it inactive
        if(previousSliderValue != null)
        {
            Debug.Log ("ColorRobotConfigfromSlider: Previous slider value is not null.");
            //Find the parent associated with the slider value
            GameObject previousRobotGameObject = ActiveTrajectory.FindObject($"Config {previousSliderValue}");

            //Color the robot active robot
            ColorRobot(previousRobotGameObject, inactiveMaterial, ref URDFRenderComponents);
        }
        
        //Find the parent associated with the slider value
        GameObject robotGameObject = ActiveTrajectory.FindObject($"Config {sliderValue}");

        if (robotGameObject == null)
        {
            Debug.Log($"ColorRobotConfigfromSlider: Robot GameObject not found for Config {sliderValue}.");
        }

        //Color the robot active robot
        ColorRobot(robotGameObject, activeMaterial, ref URDFRenderComponents);

        //Set the previous slider value to the current slider value
        previousSliderValue = sliderValue;
    }
    public void ColorRobot(GameObject RobotParent, Material material, ref Dictionary<string, string> URDFRenderComponents)
    {
        Debug.Log($"ColorRobotChildCount: {RobotParent.transform.childCount}");

        if (URDFRenderComponents.Count == 0)
        {
            Debug.Log("ColorRobot: URDFRenderComponents list is empty. Searching through URDF for MeshRenderers.");
            //Loop through all the children of the game object
            foreach (Transform child in RobotParent.transform)
            {
                FindMeshRenderers(child, ref URDFRenderComponents);
            }
        }
        else
        {
            Debug.Log("ColorRobot: URDFRenderComponents list is not empty. Coloring URDF from list.");
        }

        //Loop through the list objects and color them
        foreach (KeyValuePair<string, string> component in URDFRenderComponents)
        {
            //Get the name of the object associated with the mesh renderer
            string gameObjectName = component.Value;

            //Find the object with the name
            GameObject gameObject = RobotParent.FindObject(gameObjectName);

            //If the object is found, color it
            if (gameObject)
            {
                //Get the mesh renderer component from the object
                MeshRenderer meshRenderer = gameObject.GetComponentInChildren<MeshRenderer>();

                //If the mesh renderer is found, color it
                if (meshRenderer)
                {
                    //Set the material of the mesh renderer
                    meshRenderer.material = material;
                }
                else
                {
                    Debug.Log($"ColorRobot: MeshRenderer not found for {gameObject} when searching through URDF list.");
                }
            }
        }
    }

    ////////////////////////////////////////// Organization and Structuring ///////////////////////////////////////////////////
    private void FindLinkNames(Transform currentTransform, Dictionary<string, float> config, ref Dictionary<string,string> URDFLinkNames)
    {
        Debug.Log("FindLinkNames: Searching through URDF to find LinkNames Associated with Joint Names.");

        // Check if the current GameObject has a MeshRenderer component
        UrdfJoint urdfJoint = currentTransform.GetComponent<UrdfJoint>();

        if (urdfJoint != null)
        {
            if(config.ContainsKey(urdfJoint.JointName) && !URDFLinkNames.ContainsKey(urdfJoint.JointName))
            {
                Debug.Log($"FindLinkNames: Found UrdfJointName {urdfJoint.JointName} in URDF on GameObject {currentTransform.gameObject.name}.");
                URDFLinkNames.Add(urdfJoint.JointName, currentTransform.gameObject.name);
            }
        }

        // Traverse through all child game objects recursively
        if (currentTransform.childCount > 0)
        {
            foreach (Transform child in currentTransform)
            {
                FindLinkNames(child, config, ref URDFLinkNames);
            }
        }
        else
        {
            Debug.Log($"FindLinkNames: No UrdfJoint found in URDF on GameObject {currentTransform.gameObject.name}");
        }

    }
    private bool ConfigJointsEqualURDFLinks(Dictionary<string, float> config, ref Dictionary<string,string> URDFLinkNames)
    {
        Debug.Log("ConfigJointsEqualURDFLinks: Confirming URDF Link names and sent Joint names are Consistent.");
        
        bool isEqual = true;

        //Loop through the list objects and color them
        foreach (KeyValuePair<string, float> joint in config)
        {
            //Get the name of the object associated with the mesh renderer
            string jointName = joint.Key;

            //Try to fetch the joint name from the URDFLinkNames
            if(URDFLinkNames.ContainsKey(jointName))
            {
                Debug.Log($"ConfigJointsEqualURDFLinks: Found joint {jointName} in URDFLinkNames.");
            }
            else
            {
                Debug.Log($"ConfigJointsEqualURDFLinks: Joint {jointName} not found in URDFLinkNames.");
                isEqual = false;
            }
        }

        return isEqual;
    }
    private void FindMeshRenderers(Transform currentTransform, ref Dictionary<string,string> URDFRenderComponents)
    {
        Debug.Log($"FindMeshRenderers: Searching for Mesh Renderer in {currentTransform.gameObject.name}.");
        // Check if the current GameObject has a MeshRenderer component
        MeshRenderer meshRenderer = currentTransform.GetComponentInChildren<MeshRenderer>();

        if (meshRenderer != null)
        {
            //InstanceID of the MeshRenderer
            int instanceID = meshRenderer.GetInstanceID();

            // If found, do something with the MeshRenderer, like add it to a list
            if (!URDFRenderComponents.ContainsKey(instanceID.ToString()))
            {
                Debug.Log($"Found MeshRenderer in URDF on GameObject {meshRenderer.gameObject.name} and renaming to {meshRenderer.gameObject.name + $"_{instanceID.ToString()}"}.");
                meshRenderer.gameObject.name = meshRenderer.gameObject.name + $"_{instanceID.ToString()}";
                URDFRenderComponents.Add(instanceID.ToString(), meshRenderer.gameObject.name);
            }
        }

        // Traverse through all child game objects recursively
        if (currentTransform.childCount > 0)
        {
            foreach (Transform child in currentTransform)
            {
                FindMeshRenderers(child, ref URDFRenderComponents);
            }
        }
        else
        {
            Debug.Log($"FindMeshRenderers: No MeshRenderer found in URDF on GameObject {currentTransform.gameObject.name}");
        }

    }
    public List<string> AddJointNamesList(string robotName, List<string> jointNames)
    {
        switch(robotName)
        {
            case "UR3": //TODO: CHECK JOINT NAMES
            {
                //Add specific joint names for the UR5 robot
                Debug.Log("AddJointNamesList: UR3");
                jointNames.Add("shoulder_link");
                jointNames.Add("upper_arm_link");
                jointNames.Add("forearm_link");
                jointNames.Add("wrist_1_link");
                jointNames.Add("wrist_2_link");
                jointNames.Add("wrist_3_link");

                break;
            }
            case "UR5":
            {
                //Add specific joint names for the UR5 robot
                Debug.Log("AddJointNamesList: UR5");
                jointNames.Add("shoulder_link");
                jointNames.Add("upper_arm_link");
                jointNames.Add("forearm_link");
                jointNames.Add("wrist_1_link");
                jointNames.Add("wrist_2_link");
                jointNames.Add("wrist_3_link");

                break;
            }
            case "UR10e": //TODO: CHECK JOINT NAMES
            {
                //Add specific joint names for the UR5 robot
                Debug.Log("AddJointNamesList: UR10e");
                jointNames.Add("shoulder_link");
                jointNames.Add("upper_arm_link");
                jointNames.Add("forearm_link");
                jointNames.Add("wrist_1_link");
                jointNames.Add("wrist_2_link");
                jointNames.Add("wrist_3_link");

                break;
            }
            case "UR20": //TODO: CHECK JOINT NAMES
            {
                //Add specific joint names for the UR5 robot
                Debug.Log("AddJointNamesList: UR20");
                jointNames.Add("shoulder_link");
                jointNames.Add("upper_arm_link");
                jointNames.Add("forearm_link");
                jointNames.Add("wrist_1_link");
                jointNames.Add("wrist_2_link");
                jointNames.Add("wrist_3_link");

                break;
            }
            case "ETHZurichRFL": //TODO: CHECK JOINT NAMES
            {
                //Add specific joint names for the UR5 robot
                Debug.Log("AddJointNamesList: ETHZurichRFL");
                // jointNames.Add("bridge1");
                // jointNames.Add("bridge2");
                // jointNames.Add("robot11_xy_cart");
                // jointNames.Add("robot12_xy_cart");
                // jointNames.Add("robot11_base");
                // jointNames.Add("robot11_link_1");
                // jointNames.Add("robot11_link_2");
                // jointNames.Add("robot11_link_3");
                // jointNames.Add("robot11_link_4");
                // jointNames.Add("robot11_link_5");
                // jointNames.Add("robot11_link_6");
                // jointNames.Add("robot12_base");
                // jointNames.Add("robot12_link_1");
                // jointNames.Add("robot12_link_2");
                // jointNames.Add("robot12_link_3");
                // jointNames.Add("robot12_link_4");
                // jointNames.Add("robot12_link_5");
                // jointNames.Add("robot12_link_6");
                // jointNames.Add("robot21_xy_cart");
                // jointNames.Add("robot22_xy_cart");
                // jointNames.Add("robot21_base");
                // jointNames.Add("robot21_link_1");
                // jointNames.Add("robot21_link_2");
                // jointNames.Add("robot21_link_3");
                // jointNames.Add("robot21_link_4");
                // jointNames.Add("robot21_link_5");
                // jointNames.Add("robot21_link_6");
                // jointNames.Add("robot22_base");
                // jointNames.Add("robot22_link_1");
                // jointNames.Add("robot22_link_2");
                // jointNames.Add("robot22_link_3");
                // jointNames.Add("robot22_link_4");
                // jointNames.Add("robot22_link_5");
                // jointNames.Add("robot22_link_6");
                jointNames.Add("robot22_link_1");
                jointNames.Add("robot22_link_2");
                jointNames.Add("robot22_link_3");
                jointNames.Add("bridge1");
                jointNames.Add("robot11_xy_cart");
                jointNames.Add("robot12_xy_cart");
                jointNames.Add("robot12_link_4");
                jointNames.Add("robot12_link_5");
                jointNames.Add("robot12_link_6");
                jointNames.Add("robot12_link_1");
                jointNames.Add("robot12_link_2");
                jointNames.Add("robot12_link_3");
                jointNames.Add("robot21_base");
                jointNames.Add("robot22_base");
                jointNames.Add("bridge2");
                jointNames.Add("robot11_base");
                jointNames.Add("robot12_base");            
                jointNames.Add("robot21_link_6");
                jointNames.Add("robot21_link_5");
                jointNames.Add("robot21_link_4");
                jointNames.Add("robot21_link_3");
                jointNames.Add("robot21_link_2");
                jointNames.Add("robot21_link_1");
                jointNames.Add("robot21_xy_cart");
                jointNames.Add("robot22_xy_cart");
                jointNames.Add("robot22_link_4");
                jointNames.Add("robot22_link_5");
                jointNames.Add("robot22_link_6");
                jointNames.Add("robot11_link_6");
                jointNames.Add("robot11_link_5");
                jointNames.Add("robot11_link_4");
                jointNames.Add("robot11_link_3");
                jointNames.Add("robot11_link_2");
                jointNames.Add("robot11_link_1");

                
                break;
            }
            case "abbGofa":
            {
                //Add specific joint names for the abbGofa robot
                Debug.Log("AddJointNamesList: abbGofa");
                jointNames.Add("link_1");
                jointNames.Add("link_2");
                jointNames.Add("link_3");
                jointNames.Add("link_4");
                jointNames.Add("link_5");
                jointNames.Add("link_6");
                break;
            }
        }      
        return jointNames;  
    }

}