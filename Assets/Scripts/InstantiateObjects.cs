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
using TMPro;

//scripts to initiate all geometries in the scene

public class InstantiateObjects : MonoBehaviour
{

    //DATA STRUCTURE ITEMS
    public Dictionary<string, Node> DataItemDict;

    //INPUT MATERIALS AND OBJECTS
    public Material BuiltMaterial;
    public Material UnbuiltMaterial;
    public Material HumanBuiltMaterial;
    public Material HumanUnbuiltMaterial;
    public Material RobotBuiltMaterial;
    public Material RobotUnbuiltMaterial;

    public GameObject StoredObjects; //Parent for testing placement.
    public GameObject Elements; //Our Parent Object for the Elements
    public GameObject Joints; //Our Parent Object for the joints
    public GameObject Robots; //Our parent object for the robots

    public delegate void InitialElementsPlaced(object source, EventArgs e);
    public event InitialElementsPlaced PlacedInitialElements;

    
    private GameObject geometry_object; 

    public Material testMaterial;

    public GameObject geometry_1;
    public GameObject geometry_2;
    public GameObject geometry_3;

    public bool isTextAndImageVisible = false;


    //PRIVATE IN SCRIPT USE OBJECTS
    private ARRaycastManager rayManager;
    
    public VisualizationMode currentMode = VisualizationMode.BuiltUnbuilt;
    private DatabaseManager databaseManager;

    private GameObject circleImageTemplate;

    public void Awake()
    {   
        // Find and assign the DatabaseManager script
        databaseManager = FindObjectOfType<DatabaseManager>();
        if (databaseManager == null)
        {
            Debug.LogError("DatabaseManager script not found in the scene.");
            return;
        }

        // Find the "ImageTags" GameObject
        GameObject imageTags = GameObject.Find("ImageTags");
        if (imageTags == null)
            {
                Debug.LogError("ImageTags GameObject not found in the scene.");
                return;
            }

        // Find the "CircleImage" child within "ImageTags"
        circleImageTemplate = imageTags.transform.Find("circleImage")?.gameObject;
        if (circleImageTemplate == null)
            {
                Debug.LogError("CircleImage GameObject not found as a child of ImageTags.");
                return;
            }


        //Find Parent Object to Store Our Items in.
        Elements = GameObject.Find("Elements");

        //Find Initial Materials
        BuiltMaterial = GameObject.Find("Materials").FindObject("Built").GetComponentInChildren<Renderer>().material;
        UnbuiltMaterial = GameObject.Find("Materials").FindObject("Unbuilt").GetComponentInChildren<Renderer>().material;
        HumanBuiltMaterial = GameObject.Find("Materials").FindObject("HumanBuilt").GetComponentInChildren<Renderer>().material;
        HumanUnbuiltMaterial = GameObject.Find("Materials").FindObject("HumanUnbuilt").GetComponentInChildren<Renderer>().material;
        RobotBuiltMaterial = GameObject.Find("Materials").FindObject("RobotBuilt").GetComponentInChildren<Renderer>().material;
        RobotUnbuiltMaterial = GameObject.Find("Materials").FindObject("RobotUnbuilt").GetComponentInChildren<Renderer>().material;
    }
    

/////////////////////////////// INSTANTIATE OBJECTS ////////////////////////////////////////
    public void placeElements(List<Step> DataItems) 
    {
        foreach (Step step in DataItems)
        {
            placeElement(step);
        }

    }

    public void placeElement(Step step)
    {
        Debug.Log($"Placing element {step.data.element_ids[0]}");

        //get position
        Vector3 position = getPosition(step);
        
        //get rotation
        (Vector3 x_rh,Vector3 y_rh,Vector3 z_rh) = getRotation(step);
        (Vector3 x_lh,Vector3 y_lh,Vector3 z_lh) = rhToLh(x_rh,y_rh,z_rh);
        Quaternion rotation = rotateInstance(x_lh,y_lh,z_lh);

        //instantiate a geometry at this position and rotation
        GameObject geometry_object = gameobjectTypeSelector(step);

        if (geometry_object == null)
        {
            Debug.Log($"This key is null {step.data.element_ids[0]}");
            return;
        }

        //Instantiate new gameObject from the existing selected gameobjects.
        GameObject elementPrefab = Instantiate(geometry_object, position, rotation);

        
        //Destroy Initial gameobject that is made.
        if (geometry_object != null)
        {
            Destroy(geometry_object);
        }

        //Set parent and name
        elementPrefab.transform.SetParent(Elements.transform, false);
        elementPrefab.name = step.data.element_ids[0];

        //Get the nested Object from the .Obj so we can adapt colors
        GameObject child_object = elementPrefab.FindObject("Mesh 0");


        //Set the color of the objects based on the current mode
        switch (currentMode)
        {
            case VisualizationMode.BuiltUnbuilt:
                ColorBuiltOrUnbuilt(step.data.is_built, child_object);
                break;

            case VisualizationMode.ActorView:
                ColorHumanOrRobot(step.data.actor, step.data.is_built, child_object);
                break;
        }
        
        // Create and attach text label to the GameObject
        CreateIndexTextForGameObject(elementPrefab, step.data.element_ids[0]);
        CreateCircleImageForTag(elementPrefab);
        
        // Set initial visibility based on the current state
        SetInitialVisibility(elementPrefab);
    }

    private void SetInitialVisibility(GameObject elementPrefab)
    {
        Transform textChild = elementPrefab.transform.Find(elementPrefab.name + " Text");
        if (textChild != null)
        {
            textChild.gameObject.SetActive(isTextAndImageVisible);
        }

        Transform circleImageChild = elementPrefab.transform.Find("circleImage(Clone)"); 
        if (circleImageChild != null)
        {
            circleImageChild.gameObject.SetActive(isTextAndImageVisible);
        }
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
        if (circleImageTemplate == null)
        {
            Debug.LogError("CircleImage template is not found or not assigned.");
            return;
        }

        // Find the center of the parent object's renderer
        Renderer renderer = parentObject.GetComponentInChildren<Renderer>();
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
        GameObject circleImage = Instantiate(circleImageTemplate, offsetPosition, Quaternion.identity, parentObject.transform);
        circleImage.SetActive(false);

        // Add billboard effect
        Billboard billboard = circleImage.AddComponent<Billboard>();
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
                    placeElement(entry.Value);
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
    public Vector3 getPosition(Step step)
    {
        Vector3 position = new Vector3(step.data.location.point[0], step.data.location.point[2], step.data.location.point[1]);
        return position;
    }

    public (Vector3, Vector3, Vector3) getRotation(Step step)
    {
        Vector3 x_vec_right = new Vector3(step.data.location.xaxis[0], step.data.location.xaxis[1], step.data.location.xaxis[2]);
        Vector3 y_vec_right  = new Vector3(step.data.location.yaxis[0], step.data.location.yaxis[1], step.data.location.yaxis[2]);
        Vector3 z_vec_right  = Vector3.Cross(y_vec_right, x_vec_right).normalized;
        return (x_vec_right, y_vec_right, z_vec_right);
    } 

    //calculate transformation form right handed to left handed coordinate system
    public (Vector3, Vector3, Vector3) rhToLh(Vector3 x_vec_right, Vector3 y_vec_right, Vector3 z_vec_right)
    {        
        Vector3 x_vec = new Vector3(x_vec_right[0], x_vec_right[2], x_vec_right[1]);
        Vector3 z_vec = new Vector3(y_vec_right[0], y_vec_right[2], y_vec_right[1]);
        Vector3 y_vec = Vector3.Cross(x_vec, z_vec).normalized;
        return (x_vec, y_vec, z_vec);
    } 

    //rotate elements 
    public Quaternion rotateInstance(Vector3 x_vec, Vector3 y_vec, Vector3 z_vec)
    {
        Quaternion rotation = Quaternion.LookRotation(z_vec, y_vec);
        return rotation;
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

    // Apply color for objects based on Built or Unbuilt state
    public void ApplyColorBasedOnBuildState()
    {
        if (databaseManager.BuildingPlanDataDict != null)
            {
                foreach (KeyValuePair<string, Step> entry in databaseManager.BuildingPlanDataDict)
                {
                    GameObject gameObject = GameObject.Find(entry.Key);
                    if (gameObject != null)
                    {
                        ColorBuiltOrUnbuilt(entry.Value.data.is_built, gameObject);
                    }
                }
            }
    }

    //Apply color for objects based on Actor View state
    public void ApplyColorBasedOnActor()
    {
        if (databaseManager.BuildingPlanDataDict != null)
            {
                foreach (var entry in databaseManager.BuildingPlanDataDict)
                {
                    GameObject gameObject = GameObject.Find(entry.Key);
                    if (gameObject != null)
                    {
                        ColorHumanOrRobot(entry.Value.data.actor, entry.Value.data.is_built, gameObject);
                    }
                }
            }
    }


    public enum VisualizationMode
    {
        BuiltUnbuilt = 0,
        ActorView = 1
    }


/////////////////////////////// MATERIAL CREATION ////////////////////////////////////////
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
        placeElement(newValue);
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
    }
}
