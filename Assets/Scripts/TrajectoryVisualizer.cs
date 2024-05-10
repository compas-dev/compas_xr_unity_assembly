using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.Urdf;
using CompasXR.Core;
using CompasXR.UI;
using CompasXR.Core.Data;
using CompasXR.Core.Extentions;
using Unity.Android.Gradle.Manifest;
using CompasXR.Robots.MqttData;

namespace CompasXR.Robots
{
    /*
    * CompasXR.Robots : Is the namespace for all Classes that
    * controll the primary functionalities releated to the use of robots in the CompasXR Application.
    * Functionalities, such as robot communication, robot visualization, and robot interaction.
    */
    public class TrajectoryVisualizer : MonoBehaviour
    {
        /*
        The TrajectoryVisualizer class is responsible for managing the active robot in the scene,
        instantiating and visualizing robot trajectories, and controling placing active robot objects in the scene.
        */

        //Other script objects
        private InstantiateObjects instantiateObjects;
        private MqttTrajectoryManager mqttTrajectoryManager;
        private UIFunctionalities uiFunctionalities;

        //GameObjects for storing the active robot objects in the scene
        public GameObject ActiveRobotObjects;
        public GameObject ActiveRobot;
        public GameObject ActiveTrajectoryParentObject;
        private GameObject BuiltInRobotsParent;

        //Dictionary for storing URDFLinkNames associated with JointNames. Updated by recursive method from updating robot.
        public Dictionary<string, string> URDFLinkNames = new Dictionary<string, string>();
        public int? previousSliderValue;
        public Dictionary<string, string> URDFRenderComponents = new Dictionary<string, string>();

        //List of available robots
        public List<string> RobotURDFList = new List<string> {"UR3", "UR5", "UR10e", "ETHZurichRFL"};
            
        ////////////////////////////////////////// Monobehaviour Methods ////////////////////////////////////////////////////////
        void Start()
        {
            OnStartInitilization();
        }

        ////////////////////////////////////////// Initilization & Selection //////////////////////////////////////////////////////
        private void OnStartInitilization()
        {
            /*
            OnStartInitilization is called at the start of the script,
            and is responsible for finding and setting the necessary dependencies to objects that exist in the scene.
            */
            instantiateObjects = GameObject.Find("Instantiate").GetComponent<InstantiateObjects>();
            uiFunctionalities = GameObject.Find("UIFunctionalities").GetComponent<UIFunctionalities>();
            mqttTrajectoryManager = GameObject.Find("MQTTTrajectoryManager").GetComponent<MqttTrajectoryManager>();
            BuiltInRobotsParent = GameObject.Find("RobotPrefabs");
            ActiveRobotObjects = GameObject.Find("ActiveRobotObjects");
        }
        public void SetActiveRobotFromDropdown(string robotName, bool yRotation, bool visibility = true)
        {
            /*
            SetActiveRobotFromDropdown is called from the UI Dropdown and is responsible for setting the active robot in the scene.
            */
            if(URDFLinkNames.Count > 0)
            {
                URDFLinkNames.Clear();
            }
            if(URDFRenderComponents.Count > 0)
            {
                URDFRenderComponents.Clear();
            }
            SetActiveRobot(BuiltInRobotsParent, robotName, yRotation, ActiveRobotObjects, ref ActiveRobot, ref ActiveTrajectoryParentObject, instantiateObjects.InactiveRobotMaterial, visibility);
        }
        private void SetActiveRobot(GameObject BuiltInRobotsParent, string robotName, bool yRotation, GameObject ActiveRobotObjectsParent, ref GameObject ActiveRobot, ref GameObject ActiveTrajectoryParentObject, Material material, bool visibility)
        {
            /*
            SetActiveRobot is responsible for setting the active robot in the scene.
            */
            GameObject selectedRobot = BuiltInRobotsParent.FindObject(robotName);

            if(selectedRobot != null)
            {
                if(ActiveRobot != null)
                {
                    Destroy(ActiveRobot);
                }
                if(ActiveTrajectoryParentObject != null)
                {
                    Destroy(ActiveTrajectoryParentObject);
                }
                GameObject temporaryRobot = Instantiate(selectedRobot, ActiveRobotObjectsParent.transform.position, ActiveRobotObjectsParent.transform.rotation);
                if(yRotation)
                {
                    temporaryRobot.transform.Rotate(0, 90, 0);
                }

                ActiveRobot = Instantiate(new GameObject(), ActiveRobotObjectsParent.transform.position, ActiveRobotObjectsParent.transform.rotation);
                ActiveRobot.name = "ActiveRobot";
                ActiveRobot.transform.SetParent(ActiveRobotObjectsParent.transform);
                ActiveTrajectoryParentObject = Instantiate(new GameObject(), ActiveRobot.transform.position, ActiveRobot.transform.rotation);
                ActiveTrajectoryParentObject.name = "ActiveTrajectory";
                ActiveTrajectoryParentObject.transform.SetParent(ActiveRobotObjectsParent.transform);

                mqttTrajectoryManager.serviceManager.ActiveRobotName = robotName;

                temporaryRobot.transform.SetParent(ActiveRobot.transform);
                URDFManagement.ColorURDFGameObject(temporaryRobot, material, ref URDFRenderComponents);
                temporaryRobot.SetActive(visibility);
            }
            else
            {
                Debug.Log($"SetActiveRobot: Robot {robotName} not found in the BuiltInRobotsParent.");

                string message = "WARNING: Active Robot could not be found. Confirm with planner which Robot is in use, or load robot.";
                UserInterface.SignalOnScreenMessageFromPrefab(ref uiFunctionalities.OnScreenErrorMessagePrefab, ref uiFunctionalities.ActiveRobotCouldNotBeFoundWarningMessage, "ActiveRobotCouldNotBeFoundWarningMessage", uiFunctionalities.MessagesParent, message, $"SetActiveRobot: Robot {robotName} could not be found");
            }
        }

        ////////////////////////////////////////// Robot Object Management ////////////////////////////////////////////////////////
        public void InstantiateRobotTrajectoryFromJointsDict(List<Dictionary<string, float>> TrajectoryConfigs, Frame robotBaseFrame, string trajectoryID, GameObject robotToConfigure, Dictionary<string, string> URDFLinks, GameObject parentObject, bool visibility)
        {
            /*
            InstantiateRobotTrajectoryFromJointsDict is responsible for instantiating the robot trajectory in the scene.
            */

            Debug.Log($"InstantiateRobotTrajectory: For {trajectoryID} with {TrajectoryConfigs.Count} configurations.");
            
            if (TrajectoryConfigs.Count > 0 && robotToConfigure != null && URDFLinks.Count > 0 || parentObject != null)
            {
                int trajectoryCount = TrajectoryConfigs.Count;
                for (int i = 0; i < trajectoryCount; i++)
                {
                    GameObject temporaryRobot = Instantiate(robotToConfigure, robotToConfigure.transform.position, robotToConfigure.transform.rotation);
                    temporaryRobot.name = $"Config {i}";

                    SetRobotConfigfromDictWrapper(TrajectoryConfigs[i], $"Config {i}", temporaryRobot, ref URDFLinkNames);

                    temporaryRobot.transform.SetParent(parentObject.transform);
                    URDFManagement.SetRobotLocalPositionandRotationFromFrame(robotBaseFrame, temporaryRobot);
                    temporaryRobot.SetActive(visibility);
                }
            }
            else
            {
                
                Debug.LogError("InstantiateRobotTrajectory: Trajectory is empty, robotToConfigure is null, or joint_names is empty.");
            }
        }
        public void VisualizeRobotTrajectoryFromResultMessage(GetTrajectoryResult result, Dictionary<string,string> URDFLinkNames, GameObject robotToConfigure, GameObject parentObject, bool visibility)
        {
            /*
            VisualizeRobotTrajectoryFromJointsDict is responsible for visualizing the robot trajectory in the scene.
            */
            Debug.Log($"VisualizeRobotTrajectory: For {result.TrajectoryID} with {result.Trajectory} configurations.");
            if(!ActiveRobot.transform.GetChild(0).gameObject.activeSelf)
            {
                ActiveRobot.transform.GetChild(0).gameObject.SetActive(true);
            }
            ActiveRobot.SetActive(false);
            InstantiateRobotTrajectoryFromJointsDict(result.Trajectory, result.RobotBaseFrame, result.TrajectoryID, robotToConfigure, URDFLinkNames, parentObject, visibility);     

            if(result.PickAndPlace)
            {
                Debug.Log($"VisualizeRobotTrajectory: Attaching element to end effector link for {result.TrajectoryID}.");
                AttachElementToTrajectoryEndEffectorLinks(result.ElementID, parentObject, result.EndEffectorLinkName, result.PickIndex.Value, result.Trajectory.Count);
            }
        }

        public void AttachElementToTrajectoryEndEffectorLinks(string stepID, GameObject trajectoryParent, string endEffectorLinkName, int pickIndex, int trajectoryCount)
        {
            /*
            AttachElementToTrajectoryEndEffectorLinks is responsible for attaching an element to the end effector link in the trajectory GameObject.
            */

            int lastConfigIndex = trajectoryCount - 1;
            GameObject stepElement = GameObject.Find(stepID);
            Debug.Log($"AttachElementToTrajectoryEndEffectorLinks: GameObject Position {stepElement.transform.position} and Rotation {stepElement.transform.rotation}.");
            GameObject endEffectorLink = trajectoryParent.FindObject($"Config {lastConfigIndex}").FindObject(endEffectorLinkName);
            GameObject newStepElment = Instantiate(stepElement, stepElement.transform.localPosition, stepElement.transform.localRotation);
            newStepElment.transform.SetParent(endEffectorLink.transform, true);

            //Locla roation of the object to the end effector link
            // Vector3 position = newStepElment.transform.localPosition;
            // Quaternion rotation = newStepElment.transform.localRotation;

            // for(int i = lastConfigIndex-1; i >= pickIndex; i--)
            // {
                // GameObject currentEndEffectorLink = trajectoryParent.FindObject($"Config {i}").FindObject(endEffectorLinkName);
                // GameObject newStepElmentCopy = Instantiate(stepElement, stepElement.transform.position, stepElement.transform.rotation);
                // newStepElmentCopy.transform.SetParent(currentEndEffectorLink.transform, true);
                // newStepElmentCopy.transform.localPosition = position;
                // newStepElmentCopy.transform.localRotation = rotation;
            // }

        }
        public void DestroyActiveRobotObjects()
        {
            /*
            DestroyActiveRobotObjects is responsible for destroying the active robot objects in the scene.
            */

            if(ActiveRobot != null)
            {
                Destroy(ActiveRobot);
            }
            if(ActiveTrajectoryParentObject != null)
            {
                Destroy(ActiveTrajectoryParentObject);
            }
        }
        public void DestroyActiveTrajectoryChildren()
        {
            /*
            DestroyActiveTrajectoryChildren is responsible for destroying child objects in the trajectory parent.
            */
            if(ActiveTrajectoryParentObject != null)
            {
                foreach (Transform child in ActiveTrajectoryParentObject.transform)
                {
                    Destroy(child.gameObject);
                }
            }
        }
        public void SetRobotConfigfromDictWrapper(Dictionary<string, float> config, string configName, GameObject robotToConfigure, ref Dictionary<string, string> urdfLinkNames)
        {
            /*
            SetRobotConfigfromDictWrapper is responsible for setting the robot configuration from a dictionary.
            */

            Debug.Log($"SetRobotConfigfromDictWrapper: Visulizing robot configuration for gameObject {robotToConfigure.name}.");
            
            if (urdfLinkNames.Count == 0)
            {
                URDFManagement.FindLinkNamesFromJointNames(robotToConfigure.transform, config, ref urdfLinkNames);
            }
            if(URDFManagement.ConfigJointsEqualURDFLinks(config, urdfLinkNames))
            {
                URDFManagement.SetRobotConfigfromJointsDict(config, robotToConfigure, urdfLinkNames);
            }
            else
            {
                if(uiFunctionalities.ConfigDoesNotMatchURDFStructureWarningMessageObject == null)
                {
                    string message = $"WARNING: {configName} structure does not match the URDF structure and will not be visualized.";
                    UserInterface.SignalOnScreenMessageFromPrefab(ref uiFunctionalities.OnScreenErrorMessagePrefab, ref uiFunctionalities.ConfigDoesNotMatchURDFStructureWarningMessageObject, "ConfigDoesNotMatchURDFStructureWarningMessage", uiFunctionalities.MessagesParent, message, "SetRobotConfigfromDictWrapper: Config does not match URDF");
                }
                else if(uiFunctionalities.ConfigDoesNotMatchURDFStructureWarningMessageObject.activeSelf == false)
                {
                    string message = $"WARNING: {configName} structure does not match the URDF structure and will not be visualized.";
                    UserInterface.SignalOnScreenMessageFromPrefab(ref uiFunctionalities.OnScreenErrorMessagePrefab, ref uiFunctionalities.ConfigDoesNotMatchURDFStructureWarningMessageObject, "ConfigDoesNotMatchURDFStructureWarningMessage", uiFunctionalities.MessagesParent, message, "SetRobotConfigfromDictWrapper: Config does not match URDF");
                }

                Debug.LogWarning($"SetRobotConfigfromDictWrapper: Config dict {config.Count} (Count) and LinkNames dict {urdfLinkNames.Count} (Count) for search do not match.");
            }
        }
        public void ColorRobotConfigfromSliderInput(int sliderValue, Material inactiveMaterial, Material activeMaterial, ref int? previousSliderValue)
        {
            /*
            ColorRobotConfigfromSlider is responsible for coloring the robot configuration from the slider input for trajectory review.
            */
            Debug.Log($"ColorRobotConfigfromSlider: Coloring robot config {sliderValue} for active trajectory.");
            if(previousSliderValue != null)
            {
                GameObject previousRobotGameObject = ActiveTrajectoryParentObject.FindObject($"Config {previousSliderValue}");
                URDFManagement.ColorURDFGameObject(previousRobotGameObject, inactiveMaterial, ref URDFRenderComponents);
            }

            GameObject robotGameObject = ActiveTrajectoryParentObject.FindObject($"Config {sliderValue}");
            if (robotGameObject == null)
            {
                Debug.Log($"ColorRobotConfigfromSlider: Robot GameObject not found for Config {sliderValue}.");
            }
            URDFManagement.ColorURDFGameObject(robotGameObject, activeMaterial, ref URDFRenderComponents);
            previousSliderValue = sliderValue;
        }

    }

    public static class URDFManagement
    {
        /*
        * URDFManagement : Is a static class that contains methods for managing URDF objects in the scene.
        * URDFManagement is responsible for finding, setting, coloring, and visualizing robot configurations in the scene.
        */
        public static void SetRobotConfigfromList(List<float> config, GameObject URDFGameObject, List<string> jointNames)
        {
            /*
            SetRobotConfigfromList is responsible for setting the robot configuration to the URDF from a list of joint values.
            */
            Debug.Log($"SetRobotConfigFromList: Visulizing robot configuration for gameObject {URDFGameObject.name}.");
            int configCount = config.Count;

            for (int i = 0; i < configCount; i++)
            {
                GameObject joint = URDFGameObject.FindObject(jointNames[i]);
                if (joint)
                {
                    JointStateWriter jointStateWriter = joint.GetComponent<JointStateWriter>();
                    UrdfJoint urdfJoint = joint.GetComponent<UrdfJoint>();
                    if (!jointStateWriter)
                    {
                        jointStateWriter = joint.AddComponent<JointStateWriter>();    
                    }
                    
                    jointStateWriter.Write(config[i]);
                }  
                else
                {
                    Debug.Log($"SetRobotConfigfromList: Joint {joint.name} not found in the robotToConfigure.");
                }
            }

        }
        public static void SetRobotConfigfromJointsDict(Dictionary<string, float> config, GameObject URDFGameObject, Dictionary<string, string> linkNamesStorageDict)
        {
            /*
            SetRobotConfigfromJointsDict is responsible for setting the robot configuration to the URDF from a dictionary of joint values.
            */
            Debug.Log($"SetRobotConfigFromDict: Visulizing robot configuration for gameObject {URDFGameObject.name}.");    

            foreach (KeyValuePair<string, float> jointDescription in config)
            {
                string jointName = jointDescription.Key;
                float jointValue = jointDescription.Value;
                string urdfLinkName = linkNamesStorageDict[jointName];
                GameObject urdfLinkObject = URDFGameObject.FindObject(urdfLinkName);

                if (urdfLinkObject)
                {
                    JointStateWriter jointStateWriter = urdfLinkObject.GetComponent<JointStateWriter>();
                    if (!jointStateWriter)
                    {
                        jointStateWriter = urdfLinkObject.AddComponent<JointStateWriter>();    
                    }
                    jointStateWriter.Write(jointValue);
                }  
                else
                {
                    Debug.LogWarning($"SetRobotConfigfromDict: URDF Link {urdfLinkObject.name} not found in the robotToConfigure.");
                }
            }

        }
        public static void FindAllMeshRenderersInURDFGameObject(Transform currentTransform, Dictionary<string,string> URDFRenderComponents)
        {
            /*
            * FindAllMeshRenderersInURDFGameObject is responsible for finding all MeshRenderers in the URDF GameObject.
            * The method is called recursively to search through all children of the URDF GameObject due to its nested structure.
            */

            Debug.Log($"FindMeshRenderers: Searching for Mesh Renderer in {currentTransform.gameObject.name}.");
            MeshRenderer meshRenderer = currentTransform.GetComponentInChildren<MeshRenderer>();

            if (meshRenderer != null)
            {
                int instanceID = meshRenderer.GetInstanceID();
                if (!URDFRenderComponents.ContainsKey(instanceID.ToString()))
                {
                    meshRenderer.gameObject.name = meshRenderer.gameObject.name + $"_{instanceID.ToString()}";
                    URDFRenderComponents.Add(instanceID.ToString(), meshRenderer.gameObject.name);
                }
            }
            if (currentTransform.childCount > 0)
            {
                foreach (Transform child in currentTransform)
                {
                    FindAllMeshRenderersInURDFGameObject(child, URDFRenderComponents);
                }
            }
        }
        public static void SetRobotLocalPositionandRotationFromFrame(Frame robotBaseFrame, GameObject robotToPosition)
        {
            /*
            * SetRobotPosition is responsible for setting the robot position and rotation from the robot baseframe from a RightHanded Plane.
            */

            Debug.Log($"SetRobotPosition: Setting the robot {robotToPosition.name} to position and rotation from robot baseframe.");
            
            Vector3 positionData = ObjectTransformations.GetPositionFromRightHand(robotBaseFrame.point);
            ObjectTransformations.Rotation rotationData = ObjectTransformations.GetRotationFromRightHand(robotBaseFrame.xaxis, robotBaseFrame.yaxis);
            Quaternion rotationQuaternion = ObjectTransformations.GetQuaternionFromFrameDataForUnityObject(rotationData);
            robotToPosition.transform.localPosition = positionData;
            robotToPosition.transform.localRotation = rotationQuaternion;
        }
        public static void ColorURDFGameObject(GameObject RobotParent, Material material, ref Dictionary<string, string> URDFRenderComponentsStorageDict)
        {
            /*
            * ColorURDFGameObject is responsible for coloring the URDF GameObject with a material.
            * If the URDFRenderComponentsStorageDict is empty, the method will search through the URDF GameObject to find all MeshRenderers.
            */
            if (URDFRenderComponentsStorageDict.Count == 0)
            {
                foreach (Transform child in RobotParent.transform)
                {
                    URDFManagement.FindAllMeshRenderersInURDFGameObject(child, URDFRenderComponentsStorageDict);
                }
            }

            foreach (KeyValuePair<string, string> component in URDFRenderComponentsStorageDict)
            {
                string gameObjectName = component.Value;
                GameObject gameObject = RobotParent.FindObject(gameObjectName);

                if (gameObject)
                {
                    MeshRenderer meshRenderer = gameObject.GetComponentInChildren<MeshRenderer>();
                    if (meshRenderer)
                    {
                        meshRenderer.material = material;
                    }
                    else
                    {
                        Debug.Log($"ColorRobot: MeshRenderer not found for {gameObject} when searching through URDF list.");
                    }
                }
            }
        }
        public static void FindLinkNamesFromJointNames(Transform currentTransform, Dictionary<string, float> config, ref Dictionary<string,string> URDFLinkNamesStorageDict)
        {
            /*
            * FindLinkNamesFromJointNames is responsible for finding the URDF Link names from the Joint names in the URDF GameObject.
            * The method is called recursively to search through all children of the URDF GameObject due to its nested structure.
            */
            UrdfJoint urdfJoint = currentTransform.GetComponent<UrdfJoint>();
            if (urdfJoint != null)
            {
                if(config.ContainsKey(urdfJoint.JointName) && !URDFLinkNamesStorageDict.ContainsKey(urdfJoint.JointName))
                {
                    Debug.Log($"FindLinkNames: Found UrdfJointName {urdfJoint.JointName} in URDF on GameObject {currentTransform.gameObject.name}.");
                    URDFLinkNamesStorageDict.Add(urdfJoint.JointName, currentTransform.gameObject.name);
                }
            }
            if (currentTransform.childCount > 0)
            {
                foreach (Transform child in currentTransform)
                {
                    FindLinkNamesFromJointNames(child, config, ref URDFLinkNamesStorageDict);
                }
            }
            else
            {
                Debug.Log($"FindLinkNames: No UrdfJoint found in URDF on GameObject {currentTransform.gameObject.name}");
            }

        }
        public static bool ConfigJointsEqualURDFLinks(Dictionary<string, float> config, Dictionary<string,string> URDFLinkNamesDict)
        {
            
            /*
            * ConfigJointsEqualURDFLinks is responsible for checking if the joint names in the config dictionary match the URDF Link names.
            */
            bool isEqual = true;
            foreach (KeyValuePair<string, float> joint in config)
            {
                string jointName = joint.Key;
                if(URDFLinkNamesDict.ContainsKey(jointName))
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

    }
}
