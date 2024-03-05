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


public class TrajectoryVisulizer : MonoBehaviour
{
    //Other script objects
    private InstantiateObjects instantiateObjects;

    //GameObjects for storing the active robot objects in the scene
    public GameObject ActiveRobotObjects;
    public GameObject ActiveRobot;
    public GameObject ActiveTrajectory;
    private GameObject BuiltInRobotsParent;
    
    //List for storing the joint names of the active robot...
    //TODO: Ideally I could send this in the message as a dictionary so it is easier to find and more flexible, but for some reason joint names on CAD and in Unity do not match.
    public List<string> JointNames;
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

    private void OnStartInitilization()
    {
        
        //Find Objects for retreiving and storing the active robots in the scene
        instantiateObjects = GameObject.Find("Instantiate").GetComponent<InstantiateObjects>();
        BuiltInRobotsParent = GameObject.Find("RobotPrefabs");
        ActiveRobotObjects = GameObject.Find("ActiveRobotObjects");

        //TODO: THESE METHODS SHOULD BE WRAPPED INTO AN EVENT THAT IS TRIGGERED WHEN THE ROBOT IS SELECTED.
        //Get the joint names for the active robot
        JointNames = AddJointNamesList("ETHZurichRFL", JointNames);

        //Instantiate the active robot in the ActiveRobotObjectsParent
        SetActiveRobot(BuiltInRobotsParent, "ETHZurichRFL", ActiveRobotObjects, ref ActiveRobot, ref ActiveTrajectory, instantiateObjects.InactiveRobotMaterial);
        //TODO: THESE METHODS SHOULD BE WRAPPED INTO AN EVENT THAT IS TRIGGERED WHEN THE ROBOT IS SELECTED.

    }

    private void SetActiveRobot(GameObject BuiltInRobotsParent, string robotName, GameObject ActiveRobotObjectsParent, ref GameObject ActiveRobot, ref GameObject ActiveTrajectory, Material material)
    {
        //Set the active robot in the scene
        GameObject selectedRobot = BuiltInRobotsParent.FindObject(robotName);

        //Instantiate a new robot object in the ActiveRobotObjectsParent
        GameObject temporaryRobot = Instantiate(selectedRobot, selectedRobot.transform.position, selectedRobot.transform.rotation);

        //Create the active robot parent object and Active trajectory 
        ActiveRobot = Instantiate(new GameObject(), ActiveRobotObjectsParent.transform.position, ActiveRobotObjectsParent.transform.rotation);
        ActiveRobot.name = "ActiveRobot";
        ActiveRobot.transform.SetParent(ActiveRobotObjectsParent.transform);
        ActiveTrajectory = Instantiate(new GameObject(), ActiveRobot.transform.position, ActiveRobot.transform.rotation);
        ActiveTrajectory.name = "ActiveTrajectory";
        ActiveTrajectory.transform.SetParent(ActiveRobotObjectsParent.transform);

        //Set temporary Robots parent to the ActiveRobot.
        temporaryRobot.transform.SetParent(ActiveRobot.transform);

        //Color the active robot
        ColorRobot(temporaryRobot, material, ref URDFRenderComponents);
    }

    public void VisulizeRobotTrajectory(List<List<float>> TrajectoryConfigs, string trajectoryID, GameObject robotToConfigure, List<string> joint_names, GameObject parentObject, bool visibility) //TODO: THIS COULD POSSIBLY BE A DICT OF CONFIGS w/ JOINT NAMES.
    {
        Debug.Log($"VisulizeRobotTrajectory: For {trajectoryID} with {TrajectoryConfigs.Count} configurations.");

        if (TrajectoryConfigs.Count > 0 && robotToConfigure != null && joint_names.Count > 0 || parentObject != null)
        {
            //Get the number of configurations in the trajectory
            int trajectoryCount = TrajectoryConfigs.Count;

            //Find the parent object for holding trajectory Objects
            for (int i = 0; i < trajectoryCount; i++)
            {
                //Instantiate a new robot object in the ActiveRobotObjectsParent
                GameObject temporaryRobot = Instantiate(robotToConfigure, robotToConfigure.transform.position, robotToConfigure.transform.rotation);
                temporaryRobot.name = $"Config {i}";

                //Visulize the robot configuration
                VisulizeRobotConfig(TrajectoryConfigs[i], temporaryRobot, joint_names);

                //Set temporary Robots parent to the ActiveRobot.
                temporaryRobot.transform.SetParent(parentObject.transform);
                temporaryRobot.SetActive(visibility);
            }
        }
        else
        {
            Debug.Log("VisulizeRobotTrajectory: Trajectory is empty, robotToConfigure is null, or joint_names is empty.");
        }
    }

    public void SetActiveRobotPosition(Frame robotBaseFrame, ref GameObject ActiveRobot)
    {
        Debug.Log("SetActiveRobotPosition: Setting the active robot position.");

        //Fetch position data from the dictionary
        Vector3 positionData = instantiateObjects.getPosition(robotBaseFrame.point);

        //Fetch rotation data from the dictionary
        InstantiateObjects.Rotation rotationData = instantiateObjects.getRotation(robotBaseFrame.xaxis, robotBaseFrame.yaxis);
        
        //Convert Firebase rotation data to Quaternion rotation. Additionally
        Quaternion rotationQuaternion = instantiateObjects.FromUnityRotation(rotationData);

        //Set the local position and rotation of the active robot, so it it is in relation to the robot base frame and its parent object.
        ActiveRobot.transform.localPosition = positionData;
        ActiveRobot.transform.localRotation = rotationQuaternion;

        Debug.Log("THIS IS WHERE YOU UPDATE THE ROBOTS POSITION BASED ON THE INFO.");
    }

    //TODO: TRAJECTORY SHOULD BECOME A DICT OF CONFIGS + JointNames?. IF I CAN COORDINATE WITH THE PLANNING... Problem is this locks it into only working for compas... less open for other libraries.
    public void VisulizeRobotConfig(List<float> config, GameObject robotToConfigure, List<string> jointNames) //TODO: THIS COULD POSSIBLY BE A DICT OF CONFIGS w/ JOINT NAMES.
    {
        Debug.Log($"VisulizeRobotConfig: Visulizing robot configuration for gameObject {robotToConfigure.name}.");
        
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
    void FindMeshRenderers(Transform currentTransform, ref Dictionary<string,string> URDFRenderComponents)
    {
        Debug.Log("FindMeshRenderers: Searching through URDF for MeshRenderers.");
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
                Debug.Log($"FindMeshRenderers: Searching through URDF for MeshRenderers in {currentTransform.gameObject.name}.");
                Debug.Log($"FindMeshRenderers: Child count = {currentTransform.childCount}.");
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
            case "UR10": //TODO: CHECK JOINT NAMES
            {
                //Add specific joint names for the UR5 robot
                Debug.Log("AddJointNamesList: UR10");
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