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
using Extentions;
using Dummiesman;
using TMPro;
using UnityEngine.Events;
using UnityEngine.Analytics;
using UnityEngine.InputSystem;


//scripts to initiate all geometries in the scene
namespace Instantiate
{
    public class InstantiateObjects : MonoBehaviour
    {

        //DATA STRUCTURE ITEMS
        public Dictionary<string, Node> DataItemDict;
        
        //OTHER Sript Objects
        public DatabaseManager databaseManager;
        public UIFunctionalities UIFunctionalities;

        //INPUT MATERIALS AND OBJECTS
        public Material BuiltMaterial;
        public Material UnbuiltMaterial;
        public Material HumanBuiltMaterial;
        public Material HumanUnbuiltMaterial;
        public Material RobotBuiltMaterial;
        public Material RobotUnbuiltMaterial;
        public Material LockedObjectMaterial;
        public Material SearchedObjectMaterial;

        //Parent Objects
        public GameObject QRMarkers; 
        public GameObject Elements;

        //Events
        public delegate void InitialElementsPlaced(object source, EventArgs e);
        public event InitialElementsPlaced PlacedInitialElements;
        
        //Control Visulization Modes
        public class ModeControler
        {
            public bool ActorView { get; set; }
            
            //Can be adapted to more then just a bool (for example we used multiple touch modes last project)
            public int EditMode { get; set; }
            public bool TagsMode { get; set; }
        }

        //Make Initial Visulization controler
        public ModeControler visulizationMode = new ModeControler();

        //Private IN SCRIPT USE OBJECTS
        private GameObject geometry_object;
        private GameObject IdxImage;

        public struct Rotation
        {
            public Vector3 x;
            public Vector3 y;
            public Vector3 z;
        }

        //PRIVATE IN SCRIPT USE OBJECTS
        private ARRaycastManager rayManager;

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

            //Find Initial Materials
            BuiltMaterial = GameObject.Find("Materials").FindObject("Built").GetComponentInChildren<Renderer>().material;
            UnbuiltMaterial = GameObject.Find("Materials").FindObject("Unbuilt").GetComponentInChildren<Renderer>().material;
            HumanBuiltMaterial = GameObject.Find("Materials").FindObject("HumanBuilt").GetComponentInChildren<Renderer>().material;
            HumanUnbuiltMaterial = GameObject.Find("Materials").FindObject("HumanUnbuilt").GetComponentInChildren<Renderer>().material;
            RobotBuiltMaterial = GameObject.Find("Materials").FindObject("RobotBuilt").GetComponentInChildren<Renderer>().material;
            RobotUnbuiltMaterial = GameObject.Find("Materials").FindObject("RobotUnbuilt").GetComponentInChildren<Renderer>().material;
            LockedObjectMaterial = GameObject.Find("Materials").FindObject("LockedObjects").GetComponentInChildren<Renderer>().material;
            SearchedObjectMaterial = GameObject.Find("Materials").FindObject("SearchedObjects").GetComponentInChildren<Renderer>().material;

            IdxImage = GameObject.Find("IdxTags").FindObject("Circle");


            //Set Initial Visulization Modes
            visulizationMode.ActorView = false;
            visulizationMode.TagsMode = false;
            visulizationMode.EditMode = 0;

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
            GameObject geometryObject = elementPrefab.FindObject("Geometry");

            // Create and attach text label to the GameObject
            // CreateIndexTextForGameObject(elementPrefab, step.data.element_ids[0]);
            // CreateCircleImageForTag(elementPrefab);

            //Set Color Based on Visulization Mode
            if(visulizationMode.ActorView)
            {
                ColorHumanOrRobot(step.data.actor, step.data.is_built, geometryObject);
            }
            else
            {
                ColorBuiltOrUnbuilt(step.data.is_built, geometryObject);
            }

            //Check if the visulization tags mode is on
            if (visulizationMode.TagsMode)
            {
                //Set tag and Image visibility if the mode is on
                elementPrefab.FindObject(elementPrefab.name + " Text").gameObject.SetActive(true);
                elementPrefab.FindObject(elementPrefab.name + "IdxImage").gameObject.SetActive(true);
            }

            //Check if Editor Mode is on and if object priority is not the same as current step
            if (UIFunctionalities.CurrentStep != null)
            {
                if (visulizationMode.EditMode == 1 && step.data.priority != databaseManager.BuildingPlanDataItem.steps[UIFunctionalities.CurrentStep].data.priority)
                {
                    //Disable Collider if edit mode is on and object priority is not the same as current step
                    elementPrefab.FindObject("Geometry").GetComponent<Collider>().enabled = false;
                    elementPrefab.FindObject("Geometry").GetComponent<Renderer>().material = LockedObjectMaterial;
                }
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
        
        //TODO: Add Empty Parent object to the GameObject and name the child Object Geometry to match the .obj file.
        //TODO: Add a Colider - Everything but Obj files.
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
                        //Define the Size of the Cylinder from the data values
                        float cylinderRadius = DataItemDict[step.data.element_ids[0].ToString()].attributes.width;
                        float cylinderHeight = DataItemDict[step.data.element_ids[0].ToString()].attributes.height;
                        Vector3 cylindersize = new Vector3(cylinderRadius*2, cylinderHeight, cylinderRadius*2);
                        
                        //Create and Scale Element
                        element = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        element.transform.localScale = cylindersize;
                        break;

                    case "1.Box":                    
                        //Define the Size of the Cube from the data values
                        Vector3 cubesize = new Vector3(DataItemDict[step.data.element_ids[0].ToString()].attributes.width, DataItemDict[step.data.element_ids[0].ToString()].attributes.height, DataItemDict[step.data.element_ids[0].ToString()].attributes.length);
                        
                        //Create and Scale Element
                        element = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        element.transform.localScale = cubesize;
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

                        //Change Objects Name and Add collider
                        if (element!=null && element.transform.childCount > 0)
                        {
                            GameObject child_object = element.transform.GetChild(0).gameObject;
                            child_object.name = "Geometry";

                            //Add a collider to the object
                            BoxCollider collider = child_object.AddComponent<BoxCollider>();

                            //Mesh Object size to define the size of the collider
                            Vector3 MeshSize = child_object.GetComponent<MeshRenderer>().bounds.size;

                            //Scale Original Size by just a bit to make sure the collider is not too small.
                            Vector3 colliderSize = new Vector3(MeshSize.x*1.1f, MeshSize.y*1.2f, MeshSize.z*1.2f);

                            //Set the collider size
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
        private void CreateIndexTextForGameObject(GameObject gameObject, string text)
        {
            // Create a new GameObject for the text
            GameObject IndexTextContainer = new GameObject(gameObject.name + " Text");
            TextMeshPro IndexText = IndexTextContainer.AddComponent<TextMeshPro>();

            IndexText.text = text;
            IndexText.fontSize = 1f;
            IndexText.alignment = TextAlignmentOptions.Center;

            // Calculate the center of the GameObject
            GameObject childobject = gameObject.FindObject("Mesh 0");
            Renderer renderer = childobject.GetComponent<Renderer>();
            Vector3 center = Vector3.zero;
            center = renderer.bounds.center;

            // Offset the position slightly above the GameObject
            float verticalOffset = 0.13f;
            Vector3 textPosition = new Vector3(center.x, center.y + verticalOffset, center.z);

            IndexTextContainer.transform.position = textPosition;
            IndexTextContainer.transform.rotation = Quaternion.identity;
            IndexTextContainer.transform.SetParent(gameObject.transform);

            // Add billboard effect(object rotating with camera)
            Billboard billboard = IndexTextContainer.AddComponent<Billboard>();
        
            // Initially set the text as inactive
            IndexTextContainer.SetActive(false);   

        }
        private void CreateCircleImageForTag(GameObject parentObject)
        {
            if (IdxImage == null)
            {
                Debug.LogError("CircleImage template is not found or not assigned.");
                return;
            }

            // Find the center of the parent object's renderer
            Renderer renderer = parentObject.FindObject("Geometry").GetComponentInChildren<Renderer>();
            if (renderer == null)
            {
                Debug.LogError("Renderer not found in the parent object.");
                return;
            }

            Vector3 centerPosition = renderer.bounds.center;

            // Define the vertical offset 
            float verticalOffset = 0.13f;
            Vector3 offsetPosition = new Vector3(centerPosition.x, centerPosition.y + verticalOffset, centerPosition.z);

            // Instantiate the image object at the offset position
            GameObject circleImage = Instantiate(IdxImage, offsetPosition, Quaternion.identity, parentObject.transform);

            //Set name and parent
            circleImage.transform.SetParent(parentObject.transform);
            circleImage.name = $"{parentObject.name}IdxImage";

            // Add billboard effect
            Billboard billboard = circleImage.AddComponent<Billboard>();

            //Set Initial Visivility to false
            circleImage.SetActive(false);
        }
   
    /////////////////////////////// POSITION AND ROTATION ////////////////////////////////////////
        //Handle rotation of objects from Rhino to Unity. With option to add additional rotation around for .obj files.
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

        //Functions for obj imort correctoin.
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

    /////////////////////////////// Material and colors ////////////////////////////////////////
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
        public Material CreateMaterial(float red, float green, float blue, float alpha) //TODO: Color is incorrect
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.SetColor("_Color",  new Color(red, green, blue, alpha));
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;

            return mat;
        }

        // Apply color for objects based on Built or Unbuilt state
        public void ApplyColorBasedOnBuildState()
        {
            if (databaseManager.BuildingPlanDataItem.steps != null)
            {
                foreach (KeyValuePair<string, Step> entry in databaseManager.BuildingPlanDataItem.steps)
                {
                    GameObject gameObject = GameObject.Find(entry.Key);

                    if (gameObject != null && gameObject.name != UIFunctionalities.CurrentStep)
                    {
                        ColorBuiltOrUnbuilt(entry.Value.data.is_built, gameObject.FindObject("Geometry"));
                    }
                }
            }
        }

        //Apply color for objects based on Actor View state
        public void ApplyColorBasedOnActor()
        {
            if (databaseManager.BuildingPlanDataItem.steps != null)
            {
                foreach (var entry in databaseManager.BuildingPlanDataItem.steps)
                {
                    GameObject gameObject = GameObject.Find(entry.Key);
                    
                    if (gameObject != null && gameObject.name != UIFunctionalities.CurrentStep)
                    {
                        ColorHumanOrRobot(entry.Value.data.actor, entry.Value.data.is_built, gameObject.FindObject("Geometry"));
                    }
                }
            }
        }

    /////////////////////////////// EVENT HANDLING ////////////////////////////////////////
        public void OnDatabaseInitializedDict(object source, DataItemDictEventArgs e)
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
        private void RemoveObjects(string key)
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

            //TODO: FIND CURRENT STEP AND LAST BUILT STEP
            databaseManager.FindInitialElement();
        }
    }
}
