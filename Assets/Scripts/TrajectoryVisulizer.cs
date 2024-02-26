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
    //TODO: Ideally I could send this in the message so it is easier to find and more flexible, but for some reason joint names on CAD and in Unity do not match.
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
        JointNames = AddJointNamesList("UR5", JointNames);

        //Instantiate the active robot in the ActiveRobotObjectsParent
        SetActiveRobot(BuiltInRobotsParent, "UR5", ActiveRobotObjects, ref ActiveRobot, ref ActiveTrajectory);
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
            int trajectoryCount = TrajectoryConfigs.Count -1;

            //Find the parent object for holding trajectory Objects
            for (int i = 0; i < trajectoryCount; i++)
            {
                //Instantiate a new robot object in the ActiveRobotObjectsParent
                GameObject temporaryRobot = Instantiate(robotToConfigure, robotToConfigure.transform.position, robotToConfigure.transform.rotation);
                temporaryRobot.name = $"Config {i}";

                //Visulize the robot configuration
                // VisulizeRobotConfig(TrajectoryConfigs[i].ToArray(), temporaryRobot, joint_names); //TODO: Possibly Stuck here?

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
    public void VisulizeRobotConfig(float[] config, GameObject robotToConfigure, List<string> joint_names) //TODO: THIS COULD POSSIBLY BE A DICT OF CONFIGS w/ JOINT NAMES.
    {
        int count =0;
        foreach (string name in joint_names) //TODO: LOOP THROUGH THE JOINTS OF THE URDF BY NAME.
        {
            GameObject joint = robotToConfigure.FindObject(name); //TODO: FIND OBJECT WITH A SPECIFIC JOINT NAME FROM THIS URDF.
            if (joint)
            {
                joint.GetComponent<JointStateWriter>().Write(config[count]); //TODO: WRITES THE CONFIGURATION TO THE JOINT. BASED ON THE LIST ORDER.
            }  
            count ++;
        }

    }

    public List<string> AddJointNamesList(string robotName, List<string> jointNames)
    {
        switch(robotName)
        {
            case "UR3": //TODO: CHECK JOINT NAMES
            {
                //Add specific joint names for the UR5 robot
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
                jointNames.Add("link_0");
                jointNames.Add("link_1");
                jointNames.Add("link_2");
                jointNames.Add("link_3");
                jointNames.Add("link_4");
                jointNames.Add("link_5");
                jointNames.Add("link_6");
                jointNames.Add("link_7");
                jointNames.Add("link_8");
                jointNames.Add("link_9");
                jointNames.Add("link_10");
                jointNames.Add("link_11");
                jointNames.Add("link_12");
                jointNames.Add("link_13");
                jointNames.Add("link_14");
                jointNames.Add("link_15");
                jointNames.Add("link_16");
                jointNames.Add("link_17");
                jointNames.Add("link_18");
                jointNames.Add("link_19");
                jointNames.Add("link_20");
                jointNames.Add("link_21");
                jointNames.Add("link_22");
                jointNames.Add("link_23");
                jointNames.Add("link_24");
                jointNames.Add("link_25");
                jointNames.Add("link_26");
                jointNames.Add("link_27");
                jointNames.Add("link_28");
                jointNames.Add("link_29");
                jointNames.Add("link_30");
                jointNames.Add("link_31");
                jointNames.Add("link_32");
                jointNames.Add("link_33");

                break;
            }
            case "abbGofa":
            {
                //Add specific joint names for the abbGofa robot
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

