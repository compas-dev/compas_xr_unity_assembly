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
// using System.Numerics;


//scripts to initiate all geometries in the scene

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


    //PRIVATE IN SCRIPT USE OBJECTS
    private ARRaycastManager rayManager;

    public void Awake()
    {
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

        //Find Database Manager script to write functions.
        databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();

        
        GameObject frame = GameObject.Find("Frame_test");

        GameObject test = Instantiate(frame, frame.transform.position, frame.transform.rotation);
        GameObject test_2 = Instantiate(frame, frame.transform.position, frame.transform.rotation);
        Debug.Log($"This is your frame object {test} and this is the second one {test_2}");

        Vector3 cross_1 = Vector3.Cross(new Vector3(1f,0f,0f), new Vector3(0f,0f,1f));
        Vector3 cross_2 = Vector3.Cross(new Vector3(0f,0f,1f), new Vector3(1f,0f,0f));

        Vector3 newPosition = test.transform.position + cross_1 * 0.5f;
        test.transform.position = newPosition;
        test.name = "CROSS_1";

        Vector3 newPosition_2 = test_2.transform.position + cross_2 * 0.5f;
        test_2.transform.position = newPosition_2;
        test_2.name = "CROSS_2";
    }
    

/////////////////////////////// INSTANTIATE OBJECTS ////////////////////////////////////////
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
        Vector3 position = getPosition(step.data.location.point);
        
        //get rotation
        (Vector3 x_rh,Vector3 y_rh) = getRotation(step.data.location.xaxis, step.data.location.yaxis);
        (Vector3 x_lh,Vector3 y_lh,Vector3 z_lh) = rhToLh(x_rh,y_rh);

        //rotate vectors
        (Vector3 x_lh_rot,Vector3 y_lh_rot,Vector3 z_lh_rot) = rotateVectors(x_lh,y_lh,z_lh);
        
        //Old code
        Quaternion rotation = rotateInstance(y_lh_rot,z_lh_rot);

        //instantiate a geometry at this position and rotation
        GameObject geometry_object = gameobjectTypeSelector(step);

        if (geometry_object == null)
        {
            Debug.Log($"This key is null {step.data.element_ids[0]}");
            return;
        }

        //Instantiate new gameObject from the existing selected gameobjects.
        GameObject elementPrefab = Instantiate(geometry_object, position, rotation);
        
        GameObject testframe = GameObject.Find("Frame_test");
        
        //TODO: FRAME VISUALIZARTION
        GameObject randomObject = Instantiate(testframe, position, rotation);
        randomObject.transform.SetParent(Elements.transform, false);
        randomObject.name = $"Frame Test: {step.data.element_ids[0]}";
        GameObject childobject = randomObject.FindObject("default");
        MeshRenderer objectrenderer = childobject.GetComponentInChildren<MeshRenderer>();
        objectrenderer.enabled = true;
        
        // Destroy Initial gameobject that is made.
        // if (geometry_object != null)
        // {
        //     Destroy(geometry_object);
        // }

        //Set parent and name
        elementPrefab.transform.SetParent(Elements.transform, false);
        
        //TODO: NAME AFTER THE STEP ID
        elementPrefab.name = step.data.element_ids[0];

        //Get the nested Object from the .Obj so we can adapt colors
        GameObject child_object = elementPrefab.FindObject("Mesh 0");

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
    public Vector3 getPosition(float[] pointlist)
    {
        Vector3 position = new Vector3(pointlist[0], pointlist[2], pointlist[1]);
        return position;
    }
    public (Vector3, Vector3) getRotation(float[] x_vecdata, float [] y_vecdata)
    {
        Vector3 x_vec_right = new Vector3(x_vecdata[0], x_vecdata[1], x_vecdata[2]);
        Vector3 y_vec_right  = new Vector3(y_vecdata[0], y_vecdata[1], y_vecdata[2]);
        return (x_vec_right, y_vec_right);
    } 
    public (Vector3, Vector3, Vector3) rhToLh(Vector3 x_vec_right, Vector3 y_vec_right)
    {        
        Vector3 x_vec = new Vector3(x_vec_right[0], x_vec_right[2], x_vec_right[1]);
        Vector3 z_vec = new Vector3(y_vec_right[0], y_vec_right[2], y_vec_right[1]);
        //TODO: This line below could be a problem line. places elements correctly for timbers, but I am not sure if it only works for timbers or all assemblies.
        //270 degree rotation(y_vec needs negative because this returns posative)
        //THIS IS THE CORRECT ONE.
        Vector3 y_vec = Vector3.Cross(z_vec, x_vec); //.normalized;
        //270 degree rotation (y_vec stays positive because this is negative)
        // Vector3 y_vec = Vector3.Cross(x_vec, z_vec);
        return (x_vec, y_vec, z_vec);
        // return (x_vec, y_vec, z_vec);
    } 

    //THIS Function is only done to fix discrepencies from import.
    public (Vector3, Vector3, Vector3) rotateVectors(Vector3 x_vec, Vector3 y_vec, Vector3 z_vec)
    {
        //FIRST ROTATE 180 DEGREES AROUND Z AXIS
        Quaternion z_rotation = Quaternion.AngleAxis(180, z_vec);
        x_vec = z_rotation * x_vec;
        y_vec = z_rotation * y_vec;
        z_vec = z_rotation * z_vec;

        //THEN ROTATE 270 DEGREES AROUND X AXIS
        Quaternion rotation_x = Quaternion.AngleAxis(90f, x_vec);
        x_vec = rotation_x * x_vec;
        y_vec = rotation_x * y_vec;
        z_vec = rotation_x * z_vec;
        return (x_vec, y_vec, z_vec);
    }

    public Quaternion rotateInstance(Vector3 y_vec, Vector3 z_vec)
    {
        Debug.Log($"Vector_y: {y_vec}");
        Debug.Log($"Vector_z: {z_vec}");
        Quaternion rotation = Quaternion.LookRotation(z_vec, y_vec);
        return rotation;
    }

    //     public (Vector3, Vector3, Vector3) rhToLh(Vector3 x_vec_right, Vector3 y_vec_right, Vector3 z_vec_right)
    // {        
    //     Vector3 x_vec = new Vector3(x_vec_right[0], x_vec_right[2], x_vec_right[1]);
    //     Vector3 z_vec = new Vector3(y_vec_right[0], y_vec_right[2], y_vec_right[1]);
    //     //TODO: This line below could be a problem line. places elements correctly for timbers, but I am not sure if it only works for timbers or all assemblies.
    //     Vector3 y_vec = Vector3.Cross(x_vec, z_vec).normalized;
    //     return (x_vec, -y_vec, z_vec);
    // } 
    // public Quaternion rotateInstance(Vector3 x_vec, Vector3 y_vec, Vector3 z_vec)
    // {
    //     Quaternion rotation = Quaternion.LookRotation(z_vec, y_vec);
    //     return rotation;
    // }

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
