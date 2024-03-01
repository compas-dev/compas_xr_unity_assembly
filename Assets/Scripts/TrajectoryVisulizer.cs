using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSON;
using MQTTDataCompasXR;
using Helpers;
using RosSharp.RosBridgeClient;
using Newtonsoft.Json;


public class TrajectoryVisulizer : MonoBehaviour
{
    //GameObjects for storing the active robot objects in the scene
    public GameObject ActiveRobotObjects;
    public GameObject ActiveRobot;
    public GameObject ActiveTrajectory;
    private GameObject BuiltInRobotsParent;
    
    //List for storing the joint names of the active robot...
    //TODO: Ideally I could send this in the message as a dictionary so it is easier to find and more flexible, but for some reason joint names on CAD and in Unity do not match.
    public List<string> JointNames;

        
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
        BuiltInRobotsParent = GameObject.Find("Robots");
        ActiveRobotObjects = GameObject.Find("ActiveRobotObjects");

        //TODO: THESE METHODS SHOULD BE WRAPPED INTO AN EVENT THAT IS TRIGGERED WHEN THE ROBOT IS SELECTED.
        //Get the joint names for the active robot
        JointNames = AddJointNamesList("ETHZurichRFL", JointNames);

        //Instantiate the active robot in the ActiveRobotObjectsParent
        SetActiveRobot(BuiltInRobotsParent, "ETHZurichRFL", ActiveRobotObjects, ref ActiveRobot, ref ActiveTrajectory);
        //TODO: THESE METHODS SHOULD BE WRAPPED INTO AN EVENT THAT IS TRIGGERED WHEN THE ROBOT IS SELECTED.

    }

    private void SetActiveRobot(GameObject BuiltInRobotsParent, string robotName, GameObject ActiveRobotObjectsParent, ref GameObject ActiveRobot, ref GameObject ActiveTrajectory)
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
    }

    public void VisulizeRobotTrajectory(List<List<float>> TrajectoryConfigs, string trajectoryID, GameObject robotToConfigure, List<string> joint_names, GameObject parentObject, bool visibility) //TODO: THIS COULD POSSIBLY BE A DICT OF CONFIGS w/ JOINT NAMES.
    {
        Debug.Log($"VisulizeRobotTrajectory: For {trajectoryID} with {TrajectoryConfigs.Count} configurations.");

        if (TrajectoryConfigs.Count > 0 && robotToConfigure != null && joint_names.Count > 0 || parentObject != null)
        {
            //Get the number of configurations in the trajectory
            int trajectoryCount = TrajectoryConfigs.Count;

            //Find the parent object for holding trajectory Objects
            for (int i = 0; i < trajectoryCount -1; i++)
            {
                //Instantiate a new robot object in the ActiveRobotObjectsParent
                GameObject temporaryRobot = Instantiate(robotToConfigure, robotToConfigure.transform.position, robotToConfigure.transform.rotation);
                temporaryRobot.name = $"Config {i}";

                //Visulize the robot configuration
                VisulizeRobotConfig(TrajectoryConfigs[i], temporaryRobot, joint_names); //TODO: Possibly Stuck here?

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

    //TODO: TRAJECTORY SHOULD BECOME A DICT OF CONFIGS. IF I CAN COORDINATE WITH THE PLANNING.
    public void VisulizeRobotConfig(List<float> config, GameObject robotToConfigure, List<string> jointNames) //TODO: THIS COULD POSSIBLY BE A DICT OF CONFIGS w/ JOINT NAMES.
    {
        //Get the number of joints in the list
        int configCount = config.Count;

        //Find the parent object for holding trajectory Objects
        for (int i = 0; i < configCount -1; i++) //TODO: LOOP THROUGH THE JOINTS OF THE URDF BY NAME.
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
                jointNames.Add("bridge1");
                jointNames.Add("bridge2");
                jointNames.Add("robot11_xy_cart");
                jointNames.Add("robot12_xy_cart");
                jointNames.Add("robot11_base");
                jointNames.Add("robot11_link_1");
                jointNames.Add("robot11_link_2");
                jointNames.Add("robot11_link_3");
                jointNames.Add("robot11_link_4");
                jointNames.Add("robot11_link_5");
                jointNames.Add("robot11_link_6");
                jointNames.Add("robot12_base");
                jointNames.Add("robot12_link_1");
                jointNames.Add("robot12_link_2");
                jointNames.Add("robot12_link_3");
                jointNames.Add("robot12_link_4");
                jointNames.Add("robot12_link_5");
                jointNames.Add("robot12_link_6");
                jointNames.Add("robot21_xy_cart");
                jointNames.Add("robot22_xy_cart");
                jointNames.Add("robot21_base");
                jointNames.Add("robot21_link_1");
                jointNames.Add("robot21_link_2");
                jointNames.Add("robot21_link_3");
                jointNames.Add("robot21_link_4");
                jointNames.Add("robot21_link_5");
                jointNames.Add("robot21_link_6");
                jointNames.Add("robot22_base");
                jointNames.Add("robot22_link_1");
                jointNames.Add("robot22_link_2");
                jointNames.Add("robot22_link_3");
                jointNames.Add("robot22_link_4");
                jointNames.Add("robot22_link_5");
                jointNames.Add("robot22_link_6");

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