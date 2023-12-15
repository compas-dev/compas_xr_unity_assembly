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

        //INPUT MATERIALS AND OBJECTS
        public Material BuiltMaterial;
        public Material UnbuiltMaterial;
        public Material HumanBuiltMaterial;
        public Material HumanUnbuiltMaterial;
        public Material RobotBuiltMaterial;
        public Material RobotUnbuiltMaterial;

        //Parent Objects
        public GameObject QRMarkers; 
        public GameObject Elements;

        //EVENTS
        public delegate void InitialElementsPlaced(object source, EventArgs e);
        public event InitialElementsPlaced PlacedInitialElements;
        
        //Private IN SCRIPT USE OBJECTS
        private GameObject geometry_object;
        private string CurrentStep;
        private string LastWrittenStep = "0";

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
        
    /////////////////////////////// INSTANTIATE OBJECTS ////////////////////////////////////////
    
        private void OnAwakeInitilization()
        {
            //Find Database Manager script to write functions.
            databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
            
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

            //Find QRMarkers Parent Object
            QRMarkers = GameObject.Find("QRMarkers");

        }
        public void placeElements(List<Step> DataItems) 
        {
            foreach (Step step in DataItems)
                {
                    // placeElement(step);
                }

        }
        
        //TODO: Place Elements buildingplan and assembly.
        public void placeElement(string Key, Step step)
        {
            Debug.Log($"Placing element {step.data.element_ids[0]}");

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
                Debug.Log($"This key is null {step.data.element_ids[0]}");
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
            
            //TODO: NAME AFTER THE STEP ID
            elementPrefab.name = step.data.element_ids[0];

            //Get the nested Object from the .Obj so we can adapt colors only the first object
            GameObject child_object = elementPrefab.transform.GetChild(0).gameObject;

            if(CurrentStep != null && CurrentStep == Key)
            {
                //Color it Human or Robot Built
                // ColorHumanOrRobot(step.data.actor, step.data.is_built, child_object);
                FindCurrentStep();

            }
            else
            {
                //Color it Built or Unbuilt
                ColorBuiltOrUnbuilt(step.data.is_built, child_object);
            }

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
                Debug.LogWarning("Node is null. Cannot determine GameObject type.");
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

        //These functions are done to fix discrepencies from obj import.
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

            Debug.Log($"Coloring {gamobj.name} as {m_renderer.material.name}");
            Debug.Log($"Color of {gamobj.name} is {m_renderer.material.color}");
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
        public Material CreateMaterial(float red, float green, float blue, float alpha)
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

    /////////////////////////////////// UICONTROL ////////////////////////////////////////
        public void FindCurrentStep()
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
                    SetCurrentStep(i.ToString());
                    
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
        public void NextElementButton()
        {
            for (int i =0 ; i < databaseManager.BuildingPlanDataDict.Count; i++)
            {
                //Set data items
                Step step = databaseManager.BuildingPlanDataDict[i.ToString()];
                string ObjectKey = step.data.element_ids[0];

                //Find Gameobject
                GameObject element = Elements.FindObject(ObjectKey);

                if(element != null)
                {
                    //ONLY WAY TO FIX IF SOMEONE CHANGES THE ONE YOU ARE WORKING ON.
                    if(step.data.is_built == true)
                    {
                        //Color Previous step object as built or unbuilt
                        ColorBuiltOrUnbuilt(step.data.is_built, element.FindObject("Mesh 0"));
                    }
                    //Find the first unbuilt element
                    else
                    {
                        // Set First Found Element as Current Step
                        step.data.is_built = true;

                        //SET LAST WRITTEN ELEMENT
                        LastWrittenStep = i.ToString();

                        //WRITE INFORMATION TO DATABASE...HAS TO STAY HERE
                        databaseManager.PushAllData(databaseManager.dbreference_buildingplan.Child(i.ToString()), JsonConvert.SerializeObject(databaseManager.BuildingPlanDataDict[i.ToString()]));
                        
                        //TODO: THIS IS VERY DUMB, BUT IT WORKS... IT CANNOT BE AS SIMPLE AS ADDING 1 THOUGH.
                        FindCurrentStep();
                        
                        break;
                    }
                }

            }

        }
        public void SetCurrentStep(string key)
        {
            //Set current element name
            CurrentStep = key;

            //Find the step in the dictoinary
            Step step = databaseManager.BuildingPlanDataDict[key];

            //Find Gameobject Associated with that step
            GameObject element = Elements.FindObject(step.data.element_ids[0]);

            if(element != null)
            {
                //Color it Human or Robot Built
                ColorHumanOrRobot(step.data.actor, step.data.is_built, element.FindObject("Mesh 0"));
                Debug.Log($"Current Step is {CurrentStep}");
            }

            //Update Onscreen Text

            //Push Current key to the firebase

            
        }
        public void PreviousElementButton()
        {
            if(LastWrittenStep != null)
            {
                //Find Gameobject
                GameObject element = Elements.FindObject(databaseManager.BuildingPlanDataDict[LastWrittenStep].data.element_ids[0]);
                GameObject previouselement = Elements.FindObject(databaseManager.BuildingPlanDataDict[CurrentStep].data.element_ids[0]);

                if(element != null && previouselement != null)
                {
                    //Set the element to unbuilt
                    Step step = databaseManager.BuildingPlanDataDict[LastWrittenStep];
                    step.data.is_built = false;

                    //Push to the database
                    databaseManager.PushAllData(databaseManager.dbreference_buildingplan.Child(LastWrittenStep), JsonConvert.SerializeObject(databaseManager.BuildingPlanDataDict[LastWrittenStep]));

                    //PreviousStep Data
                    Step previousstep = databaseManager.BuildingPlanDataDict[CurrentStep];

                    //Color Previous step object as built or unbuilt
                    ColorBuiltOrUnbuilt(previousstep.data.is_built, previouselement);

                    //Set the current element to the last written element
                    SetCurrentStep(LastWrittenStep);
                }
            }
        }

    /////////////////////////////// EVENT HANDLING ////////////////////////////////////////
        public void OnDatabaseInitializedDict(object source, DataItemDictEventArgs e)
        {
            Debug.Log("Database is loaded." + " " + "Number of nodes stored as a dict= " + e.BuildingPlanDataDict.Count);
            placeElementsDict(e.BuildingPlanDataDict);
        }
        public void OnDatabaseUpdate(object source, UpdateDataItemsDictEventArgs eventArgs)
        {
            Debug.Log("Database is loaded." + " " + "Key of node updated= " + eventArgs.Key);
            if (eventArgs.NewValue == null)
            {
                Debug.Log("Object will be removed");
                RemoveObjects(eventArgs.NewValue.data.element_ids[0]);
            }
            else
            {
                Debug.Log("Object will be instantiated");
                InstantiateChangedKeys(eventArgs.NewValue, eventArgs.Key);
            }

        }
        private void InstantiateChangedKeys(Step newValue, string key)
        {
            if (GameObject.Find(newValue.data.element_ids[0]) != null)
            {
                Debug.Log("Deleting old object with key" + newValue.data.element_ids[0]);
                GameObject oldObject = GameObject.Find(newValue.data.element_ids[0]);
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
            FindCurrentStep();
        }
    }
}
