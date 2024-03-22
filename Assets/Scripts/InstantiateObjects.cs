using System;
using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Firebase.Database;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;
using ApplicationInfo;
using JSON;
using Helpers;
using Dummiesman;
using TMPro;
using ApplicationModeControler;
using UnityEngine.Events;
using UnityEngine.Analytics;
using UnityEngine.InputSystem;
using System.Xml.Linq;


//scripts to initiate all geometries in the scene
namespace Instantiate
{
    public class InstantiateObjects : MonoBehaviour
    {
        
        //Other Sript Objects
        public DatabaseManager databaseManager;
        public UIFunctionalities UIFunctionalities;

        //Object Materials
        public Material BuiltMaterial;
        public Material UnbuiltMaterial;
        public Material HumanBuiltMaterial;
        public Material HumanUnbuiltMaterial;
        public Material RobotBuiltMaterial;
        public Material RobotUnbuiltMaterial;
        public Material LockedObjectMaterial;
        public Material SearchedObjectMaterial;
        public Material ActiveRobotMaterial;
        public Material InactiveRobotMaterial;
        public Material OutlineMaterial;

        //Parent Objects
        public GameObject QRMarkers; 
        public GameObject Elements;
        public GameObject ActiveUserObjects;

        //Events
        public delegate void InitialElementsPlaced(object source, EventArgs e);
        public event InitialElementsPlaced PlacedInitialElements;

        //Make Initial Visulization controler
        public ModeControler visulizationController = new ModeControler();

        //Private in script use objects
        private GameObject IdxImage;
        private GameObject PriorityImage;
        public GameObject MyUserIndacator;
        private GameObject OtherUserIndacator;
        public GameObject ObjectLengthsTags;
        public GameObject PriorityViewrLineObject;
        public GameObject PriorityViewerPointsObject;

        //Struct for storing Rotation Values
        public struct Rotation
        {
            public Vector3 x;
            public Vector3 y;
            public Vector3 z;
        }

        public void Awake()
        {
            //Initilization Method for finding objects and materials
            OnAwakeInitilization();
        }
        
    /////////////////////////////// INSTANTIATE OBJECTS //////////////////////////////////////////
        private void OnAwakeInitilization()
        {
            //Find Additional Scripts.
            databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
            UIFunctionalities = GameObject.Find("UIFunctionalities").GetComponent<UIFunctionalities>();

            //Find Parent Object to Store Our Items in.
            Elements = GameObject.Find("Elements");
            QRMarkers = GameObject.Find("QRMarkers");
            ActiveUserObjects = GameObject.Find("ActiveUserObjects");

            //Find Initial Materials
            BuiltMaterial = GameObject.Find("Materials").FindObject("Built").GetComponentInChildren<Renderer>().material;
            UnbuiltMaterial = GameObject.Find("Materials").FindObject("Unbuilt").GetComponentInChildren<Renderer>().material;
            HumanBuiltMaterial = GameObject.Find("Materials").FindObject("HumanBuilt").GetComponentInChildren<Renderer>().material;
            HumanUnbuiltMaterial = GameObject.Find("Materials").FindObject("HumanUnbuilt").GetComponentInChildren<Renderer>().material;
            RobotBuiltMaterial = GameObject.Find("Materials").FindObject("RobotBuilt").GetComponentInChildren<Renderer>().material;
            RobotUnbuiltMaterial = GameObject.Find("Materials").FindObject("RobotUnbuilt").GetComponentInChildren<Renderer>().material;
            LockedObjectMaterial = GameObject.Find("Materials").FindObject("LockedObjects").GetComponentInChildren<Renderer>().material;
            SearchedObjectMaterial = GameObject.Find("Materials").FindObject("SearchedObjects").GetComponentInChildren<Renderer>().material;
            ActiveRobotMaterial = GameObject.Find("Materials").FindObject("ActiveRobot").GetComponentInChildren<Renderer>().material;
            InactiveRobotMaterial = GameObject.Find("Materials").FindObject("InactiveRobot").GetComponentInChildren<Renderer>().material;
            OutlineMaterial = GameObject.Find("Materials").FindObject("OutlineMaterial").GetComponentInChildren<Renderer>().material;
            
            //Find GameObjects fo internal use
            IdxImage = GameObject.Find("ImageTagTemplates").FindObject("Circle");
            PriorityImage = GameObject.Find("ImageTagTemplates").FindObject("Triangle");
            MyUserIndacator = GameObject.Find("UserIndicatorPrefabs").FindObject("MyUserIndicatorPrefab");
            OtherUserIndacator = GameObject.Find("UserIndicatorPrefabs").FindObject("OtherUserIndicatorPrefab");
            ObjectLengthsTags = GameObject.Find("ObjectLengthsTags");
            PriorityViewrLineObject = GameObject.Find("PriorityViewerObjects").FindObject("PriorityViewerLine");
            PriorityViewerPointsObject = GameObject.Find("PriorityViewerObjects").FindObject("PriorityViewerPoints");

            //Set Initial Visulization Modes
            visulizationController.VisulizationMode = VisulizationMode.BuiltUnbuilt;
            visulizationController.TouchMode = TouchMode.None;
        }
        public void placeElements(List<Step> DataItems) 
        {
            int i = 0;
            foreach (Step step in DataItems)
                {
                    placeElement(i.ToString(), step);
                    i++;
                }

        }
        public void placeElement(string Key, Step step)
        {
            Debug.Log($"Placing Element: {step.data.element_ids[0]} from Step: {Key}");

            //get position
            Vector3 positionData = getPosition(step.data.location.point);
            
            //get rotation
            Rotation rotationData = getRotation(step.data.location.xaxis, step.data.location.yaxis);
            
            //Define Object Rotation
            Quaternion rotationQuaternion = FromRhinotoUnityRotation(rotationData, databaseManager.objectOrientation);

            //instantiate a geometry at this position and rotation
            GameObject geometry_object = gameobjectTypeSelector(step);

            if (geometry_object == null)
            {
                Debug.Log($"This key:{step.data.element_ids[0]} from Step: {Key} is null");
                return;
            }

            //Instantiate new gameObject from the existing selected gameobjects.
            GameObject elementPrefab = Instantiate(geometry_object, positionData, rotationQuaternion);
            
            // Destroy Initial gameobject that is made.
            if (geometry_object != null)
            {
                Destroy(geometry_object);
            }

            //Set parent and name
            elementPrefab.transform.SetParent(Elements.transform, false);
            
            //Name the object afte the step number... might be better to get the step_id in the building plan from Chen.
            elementPrefab.name = Key;

            //Get the nested gameobject from the .Obj so we can adapt colors only the first object
            GameObject geometryObject = elementPrefab.FindObject(step.data.element_ids[0] + " Geometry");
            
            //Create 3D Index Text
            CreateTextForGameObjectOnInstantiation(elementPrefab, step.data.element_ids[0], 0.155f, $"{Key}", $"{elementPrefab.name}IdxText", 0.5f);
            CreateBackgroundImageForText(ref IdxImage, elementPrefab, 0.155f, $"{elementPrefab.name}IdxImage", false);

            //Create Priority Text
            CreateTextForGameObjectOnInstantiation(elementPrefab, step.data.element_ids[0], 0.15f, $"{step.data.priority}", $"{elementPrefab.name}PriorityText", 0.5f);
            CreateBackgroundImageForText(ref PriorityImage, elementPrefab, 0.15f, $"{elementPrefab.name}PriorityImage", false);

            //Case Switches to evaluate color and touch modes.
            ObjectColorandTouchEvaluater(visulizationController.VisulizationMode, visulizationController.TouchMode, step, Key, geometryObject);
            
            //Check if the visulization tags mode is on
            if (UIFunctionalities.IDToggleObject.GetComponent<Toggle>().isOn)
            {
                //Set tag and Image visibility if the mode is on
                elementPrefab.FindObject(elementPrefab.name + "IdxText").gameObject.SetActive(true);
                elementPrefab.FindObject(elementPrefab.name + "IdxImage").gameObject.SetActive(true);
            }

            //If Priority Viewer toggle is on then color the add additional color based on priority:
            if (UIFunctionalities.PriorityViewerToggleObject.GetComponent<Toggle>().isOn)
            {
                ColorObjectByPriority(UIFunctionalities.SelectedPriority, step.data.priority.ToString(), Key, geometryObject);

                //Set tag and Image visibility if the mode is on
                elementPrefab.FindObject(elementPrefab.name + "PriorityText").gameObject.SetActive(true);
                elementPrefab.FindObject(elementPrefab.name + "PriorityImage").gameObject.SetActive(true);
            }

            //If the object is equal to the current step also color it human or robot and instantiate an arrow again.
            if (Key == UIFunctionalities.CurrentStep)
            {
                ColorHumanOrRobot(step.data.actor, step.data.is_built, geometryObject);
                UserIndicatorInstantiator(ref MyUserIndacator, elementPrefab, Key, Key, "ME", 0.25f);
            }
        }
        public void placeElementAssembly(string Key, Node node)
        {
            Debug.Log($"Placing element {node.type_id}");

            //get position
            Vector3 positionData = getPosition(node.part.frame.point);
            
            //get rotation
            Rotation rotationData = getRotation(node.part.frame.xaxis, node.part.frame.yaxis);
            
            //Define Object Rotation
            Quaternion rotationQuaternion = FromRhinotoUnityRotation(rotationData, databaseManager.objectOrientation);

            //instantiate a geometry at this position and rotation
            GameObject geometry_object = gameobjectTypeSelectorAssembly(node);

            if (geometry_object == null)
            {
                Debug.Log($"This key is null {node.type_id}");
                return;
            }

            //Instantiate new gameObject from the existing selected gameobjects.
            GameObject elementPrefab = Instantiate(geometry_object, positionData, rotationQuaternion);
            
            // Destroy Initial gameobject that is made.
            if (geometry_object != null)
            {
                Destroy(geometry_object);
            }

            //Set parent and name
            elementPrefab.transform.SetParent(Elements.transform, false);
            
            //Name the object after the node number
            elementPrefab.name = node.type_id;

            //Get the nested Object from the .Obj so we can adapt colors only the first object
            GameObject child_object = elementPrefab.transform.GetChild(0).gameObject;

            //Color it Built or Unbuilt
            ColorBuiltOrUnbuilt(node.attributes.is_built, child_object);
        }
        public void placeElementsDict(Dictionary<string, Step> BuildingPlanDataDict)
        {
            if (BuildingPlanDataDict != null)
            {
                Debug.Log($"Number of key-value pairs in the dictionary = {BuildingPlanDataDict.Count}");
                
                //loop through the dictionary and print out the key
                foreach (KeyValuePair<string, Step> entry in BuildingPlanDataDict)
                {
                    if (entry.Value != null)
                    {
                        placeElement(entry.Key, entry.Value);
                    }

                }
                //Trigger event that all initial objects have been placed
                OnInitialObjectsPlaced();
            }
            else
            {
                Debug.LogWarning("The dictionary is null");
            }
        }   
        public GameObject gameobjectTypeSelector(Step step)
        {

            if (step == null)
            {
                Debug.LogWarning("Step is null. Cannot determine GameObject type.");
                return null;
            }

            GameObject element;

            switch (step.data.geometry)
                {
                    //TODO:REVIEW THE SIZE AND SCALE OF THESE
                    case "0.Cylinder":
                        //Create Empty gameObject to store the cylinder (Named by Step Number)
                        element = new GameObject();
                        element.transform.position = Vector3.zero;
                        element.transform.rotation = Quaternion.identity;
                        
                        //Define the Size of the Cylinder from the data values
                        float cylinderRadius = databaseManager.AssemblyDataDict[step.data.element_ids[0].ToString()].attributes.width;
                        float cylinderHeight = databaseManager.AssemblyDataDict[step.data.element_ids[0].ToString()].attributes.height;
                        Vector3 cylindersize = new Vector3(cylinderRadius*2, cylinderHeight, cylinderRadius*2);
                        
                        //Create, Scale, & name child object (Named by Assembly ID)
                        GameObject cylinderObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        cylinderObject.transform.localScale = cylindersize;
                        cylinderObject.name = step.data.element_ids[0].ToString() + " Geometry";

                        //Add a collider to the gameobject
                        BoxCollider cylinderCollider = cylinderObject.AddComponent<BoxCollider>();
                        Vector3 cylinderSize = cylinderObject.GetComponent<MeshRenderer>().bounds.size;
                        Vector3 cylinderColliderSize = new Vector3(cylinderSize.x*1.1f, cylinderSize.y*1.2f, cylinderSize.z*1.2f);
                        cylinderCollider.size = cylinderColliderSize;

                        //Set the cylinder as a child of the empty gameObject
                        cylinderObject.transform.SetParent(element.transform);

                        break;

                    case "1.Box":                    
                        //Create Empty gameObject to store the cylinder (Named by step number)
                        element = new GameObject();
                        element.transform.position = Vector3.zero;
                        element.transform.rotation = Quaternion.identity;
                        
                        //Define the Size of the Cube from the data values
                        Vector3 cubesize = new Vector3(databaseManager.AssemblyDataDict[step.data.element_ids[0].ToString()].attributes.width, databaseManager.AssemblyDataDict[step.data.element_ids[0].ToString()].attributes.height, databaseManager.AssemblyDataDict[step.data.element_ids[0].ToString()].attributes.length);
                        
                        //Create, Scale, & name Box object (Named by Assembly ID)
                        GameObject boxObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        boxObject.transform.localScale = cubesize;
                        boxObject.name = step.data.element_ids[0].ToString() + " Geometry";

                        //Add a collider to the gameobject
                        BoxCollider boxCollider = boxObject.AddComponent<BoxCollider>();
                        Vector3 boxSize = boxObject.GetComponent<MeshRenderer>().bounds.size;
                        Vector3 boxColliderSize = new Vector3(boxSize.x*1.1f, boxSize.y*1.2f, boxSize.z*1.2f);
                        boxCollider.size = boxColliderSize;

                        //Set the cylinder as a child of the empty gameObject
                        boxObject.transform.SetParent(element.transform);

                        break;

                    case "2.ObjFile":

                        string basepath = Application.persistentDataPath;
                        string folderpath = Path.Combine(basepath, "Object_Storage");
                        string filepath = Path.Combine(folderpath, step.data.element_ids[0]+".obj");

                        if (File.Exists(filepath))
                        {
                            element =  new OBJLoader().Load(filepath);
                        }
                        else
                        {
                            element = null;
                            Debug.Log ("ObjPrefab is null");
                        }

                        //Change Objects Name to the name of the key from the assembly and Add collider
                        if (element!=null && element.transform.childCount > 0)
                        {
                            //Set name of the child to the Element ID name.
                            GameObject child_object = element.transform.GetChild(0).gameObject;
                            child_object.name = step.data.element_ids[0].ToString() + " Geometry";

                            //Add a collider to the object
                            BoxCollider collider = child_object.AddComponent<BoxCollider>();
                            Vector3 MeshSize = child_object.GetComponent<MeshRenderer>().bounds.size;
                            Vector3 colliderSize = new Vector3(MeshSize.x*1.1f, MeshSize.y*1.2f, MeshSize.z*1.2f);
                            collider.size = colliderSize;

                            Debug.Log($"Atempting to add colider to object {element.name}");

                        }
                        
                        break;

                    case "3.Mesh":
                        //TODO: CONFIRM FETCH OBJECT AS OBJ OR CREATE OBJECT FROM PROVIDED DATA.
                        element = null;
                        break;

                    default:
                        Debug.LogWarning($"No element type found for type {step.data.geometry}");
                        return null;
                }

                Debug.Log($"Element type {step.data.geometry}");
                return element;
            
        }
        public GameObject gameobjectTypeSelectorAssembly(Node node)
        {

            if (node == null)
            {
                Debug.LogWarning("Node is null. Cannot determine GameObject type.");
                return null;
            }

            GameObject element;

            switch (node.type_data)
                {
                    //TODO:REVIEW THE SIZE AND SCALE OF THESE 
                    case "0.Cylinder":
                        //Define the Size of the Cylinder from the data values
                        float cylinderRadius = node.attributes.width;
                        float cylinderHeight = node.attributes.height;
                        Vector3 cylindersize = new Vector3(cylinderRadius*2, cylinderHeight, cylinderRadius*2);
                        
                        //Create and Scale Element
                        element = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        element.transform.localScale = cylindersize;
                        break;

                    case "1.Box":                    
                        //Define the Size of the Cube from the data values
                        Vector3 cubesize = new Vector3(node.attributes.width, node.attributes.height, node.attributes.length);
                        
                        //Create and Scale Element
                        element = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        element.transform.localScale = cubesize;
                        break;

                    case "2.ObjFile":

                        string basepath = Application.persistentDataPath;
                        string folderpath = Path.Combine(basepath, "Object_Storage");
                        string filepath = Path.Combine(folderpath, node.type_id+".obj");

                        if (File.Exists(filepath))
                        {
                            element =  new OBJLoader().Load(filepath);
                            
                        }
                        else
                        {
                            element = null;
                            Debug.Log ("ObjPrefab is null");
                        }
                        
                        break;

                    case "3.Mesh":
                        //TODO: CONFIRM FETCH OBJECT AS OBJ OR CREATE OBJECT FROM PROVIDED DATA.
                        element = null;
                        break;

                    default:
                        Debug.LogWarning($"No element type found for type node: {node.type_id} of type: {node.type_data}");
                        return null;
                }

                Debug.Log($"Element: {node.type_id} type: {node.type_data}");
                return element;
            
        }      
        public GameObject Create3DTextAsGameObject(string text, string gameObjectName, float fontSize, TextAlignmentOptions textAlignment, Color textColor, Vector3 position, Quaternion rotation, bool isBillboard, bool isVisible, GameObject parentObject=null, bool storePositionData=true)
        {
            // Create a new GameObject for the text
            GameObject textContainer = new GameObject(gameObjectName);

            // Set the position and rotation of the text
            textContainer.transform.position = position;
            textContainer.transform.rotation = rotation;

            // Add TextMeshPro component to the GameObject
            TextMeshPro textMesh = textContainer.AddComponent<TextMeshPro>();
            textMesh.text = text;
            textMesh.fontSize = fontSize;
            textMesh.autoSizeTextContainer = true;
            textMesh.alignment = textAlignment;
            textMesh.color = textColor;

            // Add billboard effect(object rotating with camera)
            if (isBillboard)
            {
                textContainer.AddComponent<HelpersExtensions.Billboard>();
            }

            //Set parent if there is a parent object
            if (parentObject != null)
            {
                textContainer.transform.SetParent(parentObject.transform);
            }
            
            // Add Position data class on the object
            if (storePositionData)
            {
                HelpersExtensions.ObjectPositionInfo positionData = textContainer.AddComponent<HelpersExtensions.ObjectPositionInfo>();
                positionData.StorePositionRotationScale(textContainer.transform.localPosition, textContainer.transform.localRotation, textContainer.transform.localScale);
            }

            // Set the visiblity based on the input
            textContainer.SetActive(isVisible);

            return textContainer;
        }
        public GameObject InstantiateObjectFromPrefabRefrence(ref GameObject prefabReference, string gameObjectName, Vector3 position, Quaternion rotation, GameObject parentObject=null)
        {
            //Instantiate the prefab at the position and rotation
            GameObject instantiatedObject = Instantiate(prefabReference, position, rotation);

            //Set the name of the instantiated object
            instantiatedObject.name = gameObjectName;

            //Set the parent of the instantiated object
            if (parentObject != null)
            {
                instantiatedObject.transform.SetParent(parentObject.transform);
            }

            return instantiatedObject;
        }
        private void CreateTextForGameObjectOnInstantiation(GameObject gameObject, string assemblyID, float offsetDistance, string text, string textObjectName, float fontSize)
        {              
            // Calculate the center of the GameObject
            GameObject childobject = gameObject.FindObject(assemblyID + " Geometry");

            //Get the center of the child object
            Vector3 center = FindGameObjectCenter(childobject);

            // Offset the position of center by a distance
            Vector3 offsetPosition = OffsetPositionVectorByDistance(center, offsetDistance, "y");

            //Create 3D Text
            GameObject TextContainer = Create3DTextAsGameObject(
                text, textObjectName, fontSize,
                TextAlignmentOptions.Center, Color.white, offsetPosition,
                Quaternion.identity, true, false, gameObject);
        }
        private void CreateBackgroundImageForText(ref GameObject inputImg, GameObject parentObject, float verticalOffset,string imgObjectName, bool isVisible=true, bool isBillboard=true, bool storePositionData=true)
        {            
            //Find the element ID from the step associated with this geometry
            string elementID = databaseManager.BuildingPlanDataItem.steps[parentObject.name].data.element_ids[0];

            // Find the gameObjects center
            Vector3 centerPosition = FindGameObjectCenter(parentObject.FindObject(elementID + " Geometry"));

            // Define the vertical offset 
            Vector3 offsetPosition = OffsetPositionVectorByDistance(centerPosition, verticalOffset, "y");

            // Instantiate the image object at the offset position
            GameObject imgObject = InstantiateObjectFromPrefabRefrence(ref inputImg, imgObjectName, offsetPosition, Quaternion.identity, parentObject);

            // Add billboard effect
            if (isBillboard)
            {
                HelpersExtensions.Billboard billboard = imgObject.AddComponent<HelpersExtensions.Billboard>();
            }

            // Add Position data class on the object
            if (storePositionData)
            {
                HelpersExtensions.ObjectPositionInfo positionData = imgObject.AddComponent<HelpersExtensions.ObjectPositionInfo>();
                positionData.StorePositionRotationScale(imgObject.transform.localPosition, imgObject.transform.localRotation, imgObject.transform.localScale);
            }

            //set visibility on instantiation
            imgObject.SetActive(isVisible);
        }
        public void UserIndicatorInstantiator(ref GameObject UserIndicator, GameObject parentObject, string stepKey, string namingBase, string inGameText, float fontSize)
        {            
            if (UserIndicator == null)
            {
                Debug.LogError("Could Not find UserIndicator.");
                return;
            }

            //Find the center of the Item key object
            Step step = databaseManager.BuildingPlanDataItem.steps[stepKey];
            GameObject element = Elements.FindObject(stepKey);
            GameObject geometryObject = element.FindObject(step.data.element_ids[0] + " Geometry");
            if (geometryObject == null)
            {
                Debug.LogError("Geometry Object not found.");
                return;
            }
            
            Vector3 objectCenter = FindGameObjectCenter(geometryObject);
            Vector3 arrowOffset = OffsetPositionVectorByDistance(objectCenter, 0.13f, "y");

            //Define rotation for the gameObject.
            Quaternion rotationQuaternion = Quaternion.identity;
            // Quaternion rotationQuaternion = GetQuaternionFromStepKey(stepKey);

            //Set new arrow item
            GameObject newArrow = null;

            // Instantiate arrow at the offset position
            newArrow = InstantiateObjectFromPrefabRefrence(ref UserIndicator, namingBase+" Arrow", arrowOffset, rotationQuaternion, parentObject);

            //Add billboard effect to the indicator
            newArrow.AddComponent<HelpersExtensions.Billboard>();

            //Create 3D Text
            GameObject IndexTextContainer = Create3DTextAsGameObject(
                inGameText, $"{namingBase} UserText", fontSize,
                TextAlignmentOptions.Center, Color.white, newArrow.transform.position,
                newArrow.transform.rotation, true, true, newArrow);

            //Offset the position of the text based on the user indicator object
            OffsetGameObjectPositionByExistingObjectPosition(IndexTextContainer, newArrow, 0.12f , "y");

            //Set Active
            newArrow.SetActive(true);
        }
        public void CreateNewUserObject(string UserInfoname, string itemKey)
        {
            GameObject userObject = new GameObject(UserInfoname);

            //Set parent
            userObject.transform.SetParent(ActiveUserObjects.transform);

            //Set position and rotation
            userObject.transform.position = Vector3.zero;
            userObject.transform.rotation = Quaternion.identity;

            //Instantiate Arrow
            UserIndicatorInstantiator(ref OtherUserIndacator, userObject, itemKey, UserInfoname, UserInfoname, 0.15f);
        }
        public (Vector3, Vector3) FindP1orP2Positions(string key, bool isP2)
        {
            //Find Gameobject Associated with that step
            GameObject element = Elements.FindObject(key);
            Step step = databaseManager.BuildingPlanDataItem.steps[key];

            //Find gameobject center
            Vector3 center = FindGameObjectCenter(element.FindObject(step.data.element_ids[0] + " Geometry"));

            //Find length from assembly dictionary
            float length = databaseManager.AssemblyDataDict[step.data.element_ids[0]].attributes.length;

            Vector3 ptPosition = new Vector3(0, 0, 0);
            //Calculate position of P1 or P2 
            if(!isP2)
            {                
                ptPosition = center + element.transform.right * (length / 2)* -1;
            }
            else
            {
                ptPosition = center + element.transform.right * (length / 2);
            }

            //Adjust P1 and P2 to be the same xz position as the elements for distance calculation
            Vector3 ElementsPosition = Elements.transform.position;
            Vector3 ptPositionAdjusted = new Vector3(0,0,0);
            if (ptPosition != Vector3.zero)
            {
                ptPositionAdjusted = new Vector3(ptPosition.x, ElementsPosition.y, ptPosition.z); //TODO: IT MIGHT MAKE MORE SENSE TO FLIP THIS THE OTHER WAY FOR DISTANCE CALCULATION ex. (ElementsPosition.x, P2Position.y, ElementsPosition.z)
            }
            else
            {
                Debug.LogError("P1 or P2 Position is null.");
            }

            return (ptPosition, ptPositionAdjusted);
        }
        public void CalculateandSetLengthPositions(string key)
        {
            //Find P1 and P2 Positions
            (Vector3 P1Position, Vector3 P1Adjusted) = FindP1orP2Positions(key, false);
            (Vector3 P2Position, Vector3 P2Adjusted) = FindP1orP2Positions(key, true);

            //Set Positions of P1 and P2
            ObjectLengthsTags.FindObject("P1Tag").transform.position = P1Position;
            ObjectLengthsTags.FindObject("P2Tag").transform.position = P2Position;
            
            //Check if the component has a billboard component and if it doesn't add it.
            if (ObjectLengthsTags.FindObject("P1Tag").GetComponent<HelpersExtensions.Billboard>() == null)
            {
                ObjectLengthsTags.FindObject("P1Tag").AddComponent<HelpersExtensions.Billboard>();
            }
            if (ObjectLengthsTags.FindObject("P2Tag").GetComponent<HelpersExtensions.Billboard>() == null)
            {
                ObjectLengthsTags.FindObject("P2Tag").AddComponent<HelpersExtensions.Billboard>();
            }

            //Get distance between position of P1, P2 and position of elements
            float P1distance = Vector3.Distance(P1Position, P1Adjusted);
            float P2distance = Vector3.Distance(P2Position, P2Adjusted);

            //Draw lines between the two points for P1
            LineRenderer P1Line = ObjectLengthsTags.FindObject("P1Tag").GetComponent<LineRenderer>();
            P1Line.useWorldSpace = true;
            P1Line.SetPosition(0, P1Position);
            P1Line.SetPosition(1, P1Adjusted);

            //Draw lines between the two points for P2
            LineRenderer P2Line = ObjectLengthsTags.FindObject("P2Tag").GetComponent<LineRenderer>();
            P2Line.useWorldSpace = true;
            P2Line.SetPosition(0, P2Position);
            P2Line.SetPosition(1, P2Adjusted);

            //Update Distance Text
            UIFunctionalities.SetObjectLengthsText(P1distance, P2distance);
        }
        public void UpdateObjectLengthsLines(string currentStep, GameObject p1LineObject, GameObject p2LineObject)
        {
            //Find P1 positions & update line
            (Vector3 P1Position, Vector3 P1Adjusted) = FindP1orP2Positions(currentStep, false);
            List<Vector3> P1Positions = new List<Vector3> { P1Position, P1Adjusted };
            UpdateLinePositionsByVectorList(P1Positions, p1LineObject);

            //FindPostions of P2 & Update line
            (Vector3 P2Position, Vector3 P2Adjusted) = FindP1orP2Positions(currentStep, true);
            List<Vector3> P2Positions = new List<Vector3> { P2Position, P2Adjusted };
            UpdateLinePositionsByVectorList(P2Positions, p2LineObject);

        }
        public void CreatePriorityViewerItems(string selectedPriority, ref GameObject lineObject, Color lineColor, float lineWidth, float ptRadius, Color ptColor, GameObject ptsParentObject)
        {
            //Fetch priority item from PriorityTreeDIct
            List<string> priorityList = databaseManager.PriorityTreeDict[selectedPriority];

            //draw a line between points
            DrawLinefromKeyswithGameObjectReference(priorityList, ref lineObject, lineColor, lineWidth, true, ptRadius, ptColor, ptsParentObject);
        }
        public void DrawLinefromKeyswithGameObjectReference(List<string> keyslist, ref GameObject lineObject, Color lineColor, float lineWidth, bool createPoints=true, float? ptRadius=null, Color? ptColor=null, GameObject ptsParentObject=null)
        {
            //Create a new line object
            LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();

            //Add a line renderer component to the line object if it is null
            if (lineRenderer == null)
            {
                Debug.Log("LineRenderer is null. for object: " + lineObject.name);
                lineRenderer = lineObject.AddComponent<LineRenderer>();
            }

            //If the gameobject reference contains children
            if (ptsParentObject && ptsParentObject.transform.childCount > 0)
            {
                //Destroy all children
                foreach (Transform child in ptsParentObject.transform)
                {
                    Destroy(child.gameObject);
                }
            }

            //Set the line color and width
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;

            //Check list length
            int listLength = keyslist.Count;

            //Only draw the line if the list length is greater then 1
            if (listLength > 1)
            {
                //Set the line positions & point positions
                lineRenderer.positionCount = keyslist.Count;

                for (int i = 0; i < keyslist.Count; i++)
                {
                    Debug.Log("KeysList: " + keyslist[i]);

                    GameObject element = Elements.FindObject(keyslist[i]);
                    Vector3 center = FindGameObjectCenter(element.FindObject(databaseManager.BuildingPlanDataItem.steps[keyslist[i]].data.element_ids[0] + " Geometry"));
                    lineRenderer.SetPosition(i, center);

                    //Create points if the bool is true
                    if (createPoints)
                    {
                        if(ptRadius != null && ptColor != null)
                        {
                            CreateSphereAtPosition(center, ptRadius.Value, ptColor.Value, keyslist[i] + "Point", ptsParentObject);
                        }
                        else
                        {
                            Debug.Log("DrawLineFromKeys: Point Radius and Color not provided.");
                        }
                    }
                }

                //Set the line object to visible incase it is on an automatic update when it is not visible.
                lineObject.SetActive(true);
            }
            else
            {
                //If create points set reference line to not visible, but create the sphere for the object
                if(listLength != 0)
                {                        
                    if(createPoints)
                    {
                        if(ptRadius != null && ptColor != null)
                        {
                            //Set the line object to not visible because there is just 1 point and cannot create a line.
                            lineObject.SetActive(false);

                            //Get the center of the only object in the list
                            GameObject element = Elements.FindObject(keyslist[0]);
                            Vector3 center = FindGameObjectCenter(element.FindObject(databaseManager.BuildingPlanDataItem.steps[keyslist[0]].data.element_ids[0] + " Geometry"));

                            //Create singular point if it is only 1.
                            CreateSphereAtPosition(center, ptRadius.Value, ptColor.Value, keyslist[0] + "Point", ptsParentObject);
                        }
                        else
                        {
                            Debug.Log("DrawLineFromKeys: Point Radius and Color not provided.");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("DrawLineFromKeys: List length is 0.");
                }
            }

        }
        public void UpdatePriorityLine(string selectedPriority, GameObject lineObject)
        {
            Debug.Log($"UpdatingPriorityLine: priority {selectedPriority}");
            
            //Fetch priority item from PriorityTreeDIct
            List<Vector3> priorityObjectPositions = GetPositionsFromPriorityGroup(selectedPriority);

            //Update the line positions
            UpdateLinePositionsByVectorList(priorityObjectPositions, lineObject);
        }
        public void UpdateLinePositionsByVectorList(List<Vector3> posVectorList, GameObject lineObject)
        {
            //Create a new line object
            LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();

            //Check list length
            int listLength = posVectorList.Count;

            //Only draw the line if the list length is greater then 1
            if (listLength > 1)
            {

                for (int i = 0; i < posVectorList.Count; i++)
                {
                    lineRenderer.SetPosition(i, posVectorList[i]);
                }
            }
            else
            {
                Debug.LogWarning("UpdateLinePositionsByVectorList: List length is 0.");
            }
        }
        public GameObject CreateSphereAtPosition(Vector3 position, float radius, Color color, string name=null, GameObject parentObject=null)
        {
            //Create a new sphere
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = position;
            sphere.transform.localScale = new Vector3(radius, radius, radius);
            sphere.GetComponent<Renderer>().material.color = color;

            //Set the name of the sphere
            if (name != null)
            {
                sphere.name = name;
            }

            //Set the parent of the sphere
            if (parentObject != null)
            {
                sphere.transform.SetParent(parentObject.transform);
            }

            return sphere;
        }

    /////////////////////////////// POSITION AND ROTATION ////////////////////////////////////////
        public Quaternion FromRhinotoUnityRotation(Rotation rotation, bool objZ_up)
        {   
            //Set Unity Rotation
            Rotation rotationLh = rhToLh(rotation.x , rotation.y);

            Rotation Zrotation = ZRotation(rotationLh);

            Rotation ObjectRotation;

            if (objZ_up == true)
            {
                ObjectRotation = XRotation(Zrotation);
            }
            else
            {
                ObjectRotation = Zrotation;
            }

            //Rotate Instance
            Quaternion rotationQuaternion = GetQuaternion(ObjectRotation.y, ObjectRotation.z);

            return rotationQuaternion;
        } 
        public Quaternion FromUnityRotation(Rotation rotation)
        {   
            //Right hand to left hand conversion
            Rotation rotationLh = rhToLh(rotation.x , rotation.y);

            //Set Unity Rotation
            Quaternion rotationQuaternion = GetQuaternion(rotationLh.y, rotationLh.z);

            return rotationQuaternion;
        } 
        public Vector3 getPosition(float[] pointlist)
        {
            Vector3 position = new Vector3(pointlist[0], pointlist[2], pointlist[1]);
            return position;
        }
        public Rotation getRotation(float[] x_vecdata, float [] y_vecdata)
        {
            Vector3 x_vec_right = new Vector3(x_vecdata[0], x_vecdata[1], x_vecdata[2]);
            Vector3 y_vec_right  = new Vector3(y_vecdata[0], y_vecdata[1], y_vecdata[2]);
            
            Rotation rotationRH;
            
            rotationRH.x = x_vec_right;
            rotationRH.y = y_vec_right;
            //This is never used just needed to satisfy struct code structure.
            rotationRH.z = Vector3.zero;
            
            return rotationRH;
        } 
        public Rotation rhToLh(Vector3 x_vec_right, Vector3 y_vec_right)
        {        
            Vector3 x_vec = new Vector3(x_vec_right[0], x_vec_right[2], x_vec_right[1]);
            Vector3 z_vec = new Vector3(y_vec_right[0], y_vec_right[2], y_vec_right[1]);
            Vector3 y_vec = Vector3.Cross(z_vec, x_vec);

            Rotation rotationLh;
            rotationLh.x = x_vec;
            rotationLh.z = z_vec;
            rotationLh.y = y_vec;


            return rotationLh;
        } 
        public Quaternion GetQuaternion(Vector3 y_vec, Vector3 z_vec)
        {
            Quaternion rotation = Quaternion.LookRotation(z_vec, y_vec);
            return rotation;
        }

        //Methods for obj imort correction.
        public Rotation ZRotation(Rotation ObjectRotation)
        {
            //Deconstruct Rotation Struct into Vector3
            Vector3 x_vec = ObjectRotation.x;
            Vector3 z_vec = ObjectRotation.z;
            Vector3 y_vec = ObjectRotation.y;
            
            //FIRST ROTATE 180 DEGREES AROUND Z AXIS
            Quaternion z_rotation = Quaternion.AngleAxis(180, z_vec);
            x_vec = z_rotation * x_vec;
            y_vec = z_rotation * y_vec;
            z_vec = z_rotation * z_vec;

            //Reconstruct new rotation struct from manipulated vectors
            Rotation ZXrotation;
            ZXrotation.x = x_vec;
            ZXrotation.y = y_vec;
            ZXrotation.z = z_vec;

            return ZXrotation;
        }
        public Rotation XRotation(Rotation ObjectRotation)
        {
            //Deconstruct Rotation Struct into Vector3
            Vector3 x_vec = ObjectRotation.x;
            Vector3 z_vec = ObjectRotation.z;
            Vector3 y_vec = ObjectRotation.y;

            //THEN ROTATE 90 DEGREES AROUND X AXIS
            Quaternion rotation_x = Quaternion.AngleAxis(90f, x_vec);
            x_vec = rotation_x * x_vec;
            y_vec = rotation_x * y_vec;
            z_vec = rotation_x * z_vec;

            //Reconstruct new rotation struct from manipulated vectors
            Rotation ZXrotation;
            ZXrotation.x = x_vec;
            ZXrotation.y = y_vec;
            ZXrotation.z = z_vec;

            return ZXrotation;
        }

        //Methods for position and rotation of unity game objects
        public Vector3 FindGameObjectCenter(GameObject gameObject)
        {
            Renderer renderer = gameObject.GetComponentInChildren<Renderer>();
            if (renderer == null)
            {
                Debug.LogError("Renderer not found in the parent object.");
                return Vector3.zero;
            }
            Vector3 center = renderer.bounds.center;
            return center;
        }
        public Vector3 OffsetPositionVectorByDistance(Vector3 position, float offsetDistance, string axis)
        {
            // Offset the position based on input values.
            switch (axis)
            {
                case "x":
                {
                    Vector3 offsetPosition = new Vector3(position.x + offsetDistance, position.y, position.z);
                    return offsetPosition;
                }
                case "y":
                {
                    Vector3 offsetPosition = new Vector3(position.x, position.y + offsetDistance, position.z);
                    return offsetPosition;
                }
                case "z":
                {
                    Vector3 offsetPosition = new Vector3(position.x, position.y, position.z + offsetDistance);
                    return offsetPosition;
                }
            }
            return Vector3.zero;
        }
        public void OffsetGameObjectPositionByExistingObjectPosition(GameObject gameObject, GameObject existingObject, float offsetDistance, string axis)
        {
            // Calculate the center of the existing GameObject
            Vector3 center = FindGameObjectCenter(existingObject);

            // Offset the position based on input values.
            switch (axis)
            {
                case "x":
                {
                    Vector3 offsetPosition = new Vector3(center.x + offsetDistance, center.y, center.z);
                    gameObject.transform.position = offsetPosition;
                    break;
                }
                case "y":
                {
                    Vector3 offsetPosition = new Vector3(center.x, center.y + offsetDistance, center.z);
                    gameObject.transform.position = offsetPosition;
                    break;
                }
                case "z":
                {
                    Vector3 offsetPosition = new Vector3(center.x, center.y, center.z + offsetDistance);
                    gameObject.transform.position = offsetPosition;
                    break;
                }
            }
        }
        public Quaternion GetQuaternionFromStepKey(string key)
        {
            //Calculate Right Hand Rotation
            Rotation rotationrh = getRotation(databaseManager.BuildingPlanDataItem.steps[key].data.location.xaxis, databaseManager.BuildingPlanDataItem.steps[key].data.location.yaxis); 
            
            //Convert to Left Hand Rotation
            Rotation rotationlh = rhToLh(rotationrh.x , rotationrh.y);
            
            //GetQuaterion from Left Hand Rotation
            Quaternion rotationQuaternion = GetQuaternion(rotationlh.y, rotationlh.z);

            return rotationQuaternion;
        }
        public List<Vector3> GetPositionsFromPriorityGroup(string priorityGroup)
        {
            List<Vector3> positions = new List<Vector3>();

            //Get the list of keys from the priority group
            List<string> keys = databaseManager.PriorityTreeDict[priorityGroup];

            //Get the positions of the keys
            foreach (string key in keys)
            {
                GameObject element = Elements.FindObject(key);
                Vector3 center = FindGameObjectCenter(element.FindObject(databaseManager.BuildingPlanDataItem.steps[key].data.element_ids[0] + " Geometry"));
                positions.Add(center);
            }

            return positions;
        }

    /////////////////////////////// Material and colors ////////////////////////////////////////
        public void ObjectColorandTouchEvaluater(VisulizationMode visualizationMode, TouchMode touchMode, Step step, string key, GameObject geometryObject)
        {
            //Set Color Based on Visulization Mode
            switch (visulizationController.VisulizationMode)
            {
                case VisulizationMode.BuiltUnbuilt:
                    ColorBuiltOrUnbuilt(step.data.is_built, geometryObject);
                    break;
                case VisulizationMode.ActorView:
                    ColorHumanOrRobot(step.data.actor, step.data.is_built, geometryObject);
                    break;
            }

            //Set Touch mode based on Touch Mode
            switch (visulizationController.TouchMode)
            {
                case TouchMode.None:
                    //Do nothing
                    break;
                case TouchMode.ElementEditSelection:
                    //Color the object if it is a lower priority then the current one.
                    if (step.data.priority > Convert.ToInt16(databaseManager.CurrentPriority))
                    {    
                        ColorObjectByLowerPriority(key, databaseManager.CurrentPriority, geometryObject);
                    }
                    break;
            }
        }
        public void ColorBuiltOrUnbuilt (bool built, GameObject gamobj)
        {
            //Get Object Renderer
            Renderer m_renderer= gamobj.GetComponentInChildren<MeshRenderer>();
            
            if (built)
            {          
                //Color For built Objects
                m_renderer.material = BuiltMaterial; 
            }
        
            else
            {
                //Color For Unbuilt Objects
                m_renderer.material = UnbuiltMaterial;
            }
        }
        public void ColorHumanOrRobot (string placed_by, bool Built, GameObject gamobj)
        {
            
            //Get Object Renderer
            Renderer m_renderer= gamobj.GetComponentInChildren<Renderer>();
            
            if (placed_by == "HUMAN")
            {
                if(Built)
                {
                    //Color For Built Human Objects
                    m_renderer.material = HumanBuiltMaterial;
                }
                else
                {
                    //Color For Unbuilt Human Objects
                    m_renderer.material = HumanUnbuiltMaterial; 
                }
            }
            else
            {
                if(Built)
                {
                    //Color For Built Robot Objects
                    m_renderer.material = RobotBuiltMaterial;
                }
                else
                {
                    //Color For Unbuilt Robot Objects
                    m_renderer.material = RobotUnbuiltMaterial;
                }
            }
        }
        public void ColorObjectByPriority(string SelectedPriority, string StepPriority,string Key, GameObject gamobj)
        {
            //Get Object Renderer
            Renderer m_renderer= gamobj.GetComponentInChildren<Renderer>();

            //If the steps priority is not the same as the selected priority then color it grey
            if (StepPriority != SelectedPriority)
            {

                //Color the object with only outline material
                m_renderer.material = OutlineMaterial;

            }
            else
            {
                //Find the item in the dictionary
                Step step = databaseManager.BuildingPlanDataItem.steps[Key];
                string elementID = step.data.element_ids[0];

                //Color based on visulization mode
                ObjectColorandTouchEvaluater(visulizationController.VisulizationMode, visulizationController.TouchMode, step, Key, gamobj.FindObject(elementID + " Geometry"));
            }
        }
        public void ColorObjectByLowerPriority(string key, string currentPriority, GameObject gameObject)
        {
            //Get Object Renderer
            Renderer m_renderer= gameObject.GetComponentInChildren<Renderer>();

            //Step for the for key
            Step step = databaseManager.BuildingPlanDataItem.steps[key];

            if(currentPriority != null)
            {
                //Color based if it is lower then current priority
                if (step.data.priority > Convert.ToInt16(currentPriority))
                {
                    //If the steps priority is not the same as the selected priority then color it grey
                    Debug.Log($"ColorObjectByLowerPriority: Coloring object {gameObject.name} grey");

                    //Create a new color for the object based on its current color, and add a greyscale blend factor
                    Color objectAdjustedColor = AdjustColorByGreyscale(m_renderer.material.color, 0.45f);

                    //Set the object to the new color
                    m_renderer.material.color = objectAdjustedColor;
                }
                else
                {
                    Debug.Log($"ColorObjectByLowerPriority: Gameobject {key} is of a lower priority then current step.");
                }
            }
            else
            {
                Debug.Log("ColorObjectByLowerPriority: Current Priority is null.");
            }
        }
        public void ApplyColorBasedOnBuildState()
        {
            if (databaseManager.BuildingPlanDataItem.steps != null)
            {
                foreach (KeyValuePair<string, Step> entry in databaseManager.BuildingPlanDataItem.steps)
                {
                    GameObject gameObject = GameObject.Find(entry.Key);

                    if (gameObject != null && gameObject.name != UIFunctionalities.CurrentStep)
                    {
                        ColorBuiltOrUnbuilt(entry.Value.data.is_built, gameObject.FindObject(entry.Value.data.element_ids[0]));

                        //Check if Priority Viewer is on and color based on priority also if it is.
                        if (UIFunctionalities.PriorityViewerToggleObject.GetComponent<Toggle>().isOn)
                        {
                            //Color based on Priority
                            ColorObjectByPriority(UIFunctionalities.SelectedPriority, entry.Value.data.priority.ToString(), entry.Key, gameObject.FindObject(entry.Value.data.element_ids[0]));
                        }
                    }
                }
            }
        }
        public void ApplyColorBasedOnActor()
        {
            if (databaseManager.BuildingPlanDataItem.steps != null)
            {
                foreach (var entry in databaseManager.BuildingPlanDataItem.steps)
                {
                    GameObject gameObject = GameObject.Find(entry.Key);
                    
                    if (gameObject != null && gameObject.name != UIFunctionalities.CurrentStep)
                    {
                        ColorHumanOrRobot(entry.Value.data.actor, entry.Value.data.is_built, gameObject.FindObject(entry.Value.data.element_ids[0]));

                        //Check if Priority Viewer is on and color based on priority if it is.
                        if (UIFunctionalities.PriorityViewerToggleObject.GetComponent<Toggle>().isOn)
                        {
                            //Color based on priority
                            ColorObjectByPriority(UIFunctionalities.SelectedPriority, entry.Value.data.priority.ToString(), entry.Key, gameObject.FindObject(entry.Value.data.element_ids[0]));
                        }
                    }
                }
            }
        }
        public void ApplyColorBasedOnPriority(string SelectedPriority)
        {
            Debug.Log($"Applying color based on priority: {SelectedPriority}.");
            if (databaseManager.BuildingPlanDataItem.steps != null)
            {
                foreach (var entry in databaseManager.BuildingPlanDataItem.steps)
                {
                    GameObject gameObject = GameObject.Find(entry.Key);
                    
                    //If the objects are not null color by priority function.
                    if (gameObject != null)
                    {
                        if (entry.Key != UIFunctionalities.CurrentStep)
                        {
                            //Color based on priority
                            ColorObjectByPriority(SelectedPriority, entry.Value.data.priority.ToString(), entry.Key, gameObject.FindObject(entry.Value.data.element_ids[0] + " Geometry"));
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Could not find object with key: {entry.Key}");
                    }
                }
            }
        }
        public void ApplyColortoPriorityGroup(string selectedPriorityGroup, string newPriorityGroup, bool newPriority=false)
        {
            //Get the list of keys from the priority group
            List<string> priorityList = databaseManager.PriorityTreeDict[selectedPriorityGroup];

            //Loop through keys in the priority list to color
            foreach (string key in priorityList)
            {
                GameObject gameObject = GameObject.Find(key);

                if (gameObject != null)
                {
                    if (key != UIFunctionalities.CurrentStep)
                    {
                        if (newPriority)
                        {
                            //Color the object based on current app settings
                            ObjectColorandTouchEvaluater(visulizationController.VisulizationMode, visulizationController.TouchMode, databaseManager.BuildingPlanDataItem.steps[key], key, gameObject.FindObject(databaseManager.BuildingPlanDataItem.steps[key].data.element_ids[0]));
                        }
                        else
                        {
                            //Color the object based on the priority group
                            Debug.Log("SetPriority" + selectedPriorityGroup + "Priority of selected item: " + databaseManager.BuildingPlanDataItem.steps[key].data.priority.ToString() + " Key: " + key + " GameObject: " + gameObject.FindObject(databaseManager.BuildingPlanDataItem.steps[key].data.element_ids[0]) + " PriorityGroup: ");
                            ColorObjectByPriority(newPriorityGroup, databaseManager.BuildingPlanDataItem.steps[key].data.priority.ToString(), key, gameObject.FindObject(databaseManager.BuildingPlanDataItem.steps[key].data.element_ids[0]));
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"Could not find object with key: {key}");
                }
            }
        }
        public void ApplyColorForHigherPriority(string CurrentPriority)
        {
            Debug.Log($"Applying color for touch : {CurrentPriority}.");
            if (databaseManager.BuildingPlanDataItem.steps != null)
            {
                foreach (var entry in databaseManager.BuildingPlanDataItem.steps)
                {
                    GameObject gameObject = GameObject.Find(entry.Key);
                    
                    //If the objects are not null color by priority function.
                    if (gameObject != null)
                    {
                        //Color object if it is of a higher priority then the current priority
                        ColorObjectByLowerPriority(entry.Key, CurrentPriority, gameObject);
                    }
                    else
                    {
                        Debug.LogWarning($"Could not find object with key: {entry.Key}");
                    }
                }
            }
        }
        public Color AdjustColorByGreyscale(Color originalColor, float factor)
        {
            //If Color is not white (Built and Unbuilt Colors)
            if (originalColor.r != 1.000f && originalColor.g != 1.000f && originalColor.b != 1.000f)
            {
                // Factor should be between 0 and 1, where 0 is unchanged and 1 is fully gray
                factor = Mathf.Clamp01(factor);

                // Convert the original color to grayscale
                float grayscaleValue = originalColor.r * 0.3f + originalColor.g * 0.59f + originalColor.b * 0.11f;

                // Blend the grayscale color with the original color based on the factor
                float blendedR = originalColor.r * (1 - factor) + grayscaleValue * factor;
                float blendedG = originalColor.g * (1 - factor) + grayscaleValue * factor;
                float blendedB = originalColor.b * (1 - factor) + grayscaleValue * factor;

                // Ensure color values are in the valid range (0 to 1)
                blendedR = Mathf.Clamp01(blendedR);
                blendedG = Mathf.Clamp01(blendedG);
                blendedB = Mathf.Clamp01(blendedB);

                // Create and return the new color
                Color newColor = new Color(blendedR, blendedG, blendedB, originalColor.a);
                
                Debug.Log($"Original Color: {originalColor} New Color: {newColor}");
                
                return newColor;
            }
            else
            {
                Color newColor = new Color(originalColor.r * factor, originalColor.g * factor, originalColor.b * factor, originalColor.a);
                return newColor;
            }
        }
    
    /////////////////////////////// EVENT HANDLING ////////////////////////////////////////
        public void OnDatabaseInitializedDict(object source, BuildingPlanDataDictEventArgs e)
        {
            Debug.Log("Database is loaded." + " " + "Number of nodes stored as a dict= " + e.BuildingPlanDataItem.steps.Count);
            placeElementsDict(e.BuildingPlanDataItem.steps);
        }
        public void OnDatabaseUpdate(object source, UpdateDataItemsDictEventArgs eventArgs)
        {
            Debug.Log("Database is loaded." + " " + "Key of node updated= " + eventArgs.Key);
            if (eventArgs.NewValue == null)
            {
                Debug.Log("Object will be removed");
                RemoveObjects(eventArgs.Key);
            }
            else
            {
                Debug.Log("Object will be instantiated");
                InstantiateChangedKeys(eventArgs.NewValue, eventArgs.Key);
            }

        }
        public void OnUserInfoUpdate(object source, UserInfoDataItemsDictEventArgs eventArgs)
        {
            Debug.Log("User Info is loaded." + " " + "Key of node updated= " + eventArgs.Key);
            if (eventArgs.UserInfo == null)
            {
                Debug.Log($"user {eventArgs.Key} will be removed");
                RemoveObjects(eventArgs.Key);
            }
            else
            {
                if (GameObject.Find(eventArgs.Key) != null)
                {
                    //Remove existing Arrow
                    RemoveObjects(eventArgs.Key + " Arrow");

                    //Instantiate new Arrow
                    // ArrowInstantiator(GameObject.Find(eventArgs.Key), eventArgs.UserInfo.currentStep, true);
                    UserIndicatorInstantiator(ref OtherUserIndacator, GameObject.Find(eventArgs.Key), eventArgs.UserInfo.currentStep, eventArgs.Key, eventArgs.Key, 0.15f);
                }
                else
                {
                    Debug.Log($"Creating a new user object for {eventArgs.Key}");
                    CreateNewUserObject(eventArgs.Key, eventArgs.UserInfo.currentStep);
                }
            }
        }
        private void InstantiateChangedKeys(Step newValue, string key)
        {
            if (GameObject.Find(key) != null)
            {
                Debug.Log("Deleting old object with key" + key);
                GameObject oldObject = GameObject.Find(key);
                Destroy(oldObject);
            }
            else
            {
                Debug.Log( $"Could Not find Object with key: {key}");
            }
            placeElement(key, newValue);
        }
        public void RemoveObjects(string key)
        {
            //Delete old object if it already exists
            if (GameObject.Find(key) != null)
            {
                Debug.Log("Deleting old object");
                GameObject oldObject = GameObject.Find(key);
                Destroy(oldObject);
            }
            else
            {
                Debug.Log( $"Could Not find Object with key: {key}");
            }
        }
        protected virtual void OnInitialObjectsPlaced()
        {
            PlacedInitialElements(this, EventArgs.Empty);

            //Find the first unbuilt element in the database
            databaseManager.FindInitialElement();
        }
    }
}
