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
        
        // //Other Sript Objects
        // public DatabaseManager databaseManager;
        // public UIFunctionalities UIFunctionalities;
        // public ScrollSearchManager scrollSearchManager;

        //Object Materials
        // public Material BuiltMaterial;
        // public Material UnbuiltMaterial;
        // public Material HumanBuiltMaterial;
        // public Material HumanUnbuiltMaterial;
        // public Material RobotBuiltMaterial;
        // public Material RobotUnbuiltMaterial;
        // public Material LockedObjectMaterial;
        // public Material SearchedObjectMaterial;
        // public Material ActiveRobotMaterial;
        // public Material InactiveRobotMaterial;
        // public Material OutlineMaterial;

        // //Parent Objects
        // public GameObject QRMarkers; 
        // public GameObject Elements;
        // public GameObject ActiveUserObjects;

        //Events
        // public delegate void InitialElementsPlaced(object source, EventArgs e);
        // public event InitialElementsPlaced PlacedInitialElements;

        // //Make Initial Visulization controler
        // public ModeControler visulizationController = new ModeControler();

        // //Private in script use objects
        // private GameObject IdxImage;
        // private GameObject PriorityImage;
        // public GameObject MyUserIndacator;
        // private GameObject OtherUserIndacator;
        // public GameObject ObjectLengthsTags;
        // public GameObject PriorityViewrLineObject;
        // public GameObject PriorityViewerPointsObject;

        //Struct for storing Rotation Values
        public struct Rotation
        {
            public Vector3 x;
            public Vector3 y;
            public Vector3 z;
        }

        // public void Awake()
        // {
        //     //Initilization Method for finding objects and materials
        //     OnAwakeInitilization();
        // }

        //MAS Modifiers
        public GameObject RuntimeObjectStorageObject;
        public GameObject PrefabObject;
        
    /////////////////////////////// INSTANTIATE OBJECTS //////////////////////////////////////////
    
        //MAS Methods
        
        //TODO: Write a method to iterate through the dictionary and place an element for each one
        public void PlacePrefabsfromFrameDict(Dictionary<string, Frame> MyFrameDictionary, GameObject prefabObject, GameObject ParentObject)
        {
            //Check if the frame dictionary is null
            if (MyFrameDictionary != null)
            {
                Debug.Log($"PlaceElementsFrameDict: Number of key-value pairs in the dictionary = {MyFrameDictionary.Count}");
                
                //loop through the dictionary and print out the key
                foreach (KeyValuePair<string, Frame> entry in MyFrameDictionary)
                {
                    if (entry.Value != null)
                    {
                        //Call method to place the prefab from the frame data
                        InstantiatePrefabFromFrameData(entry.Key, entry.Value, prefabObject, RuntimeObjectStorageObject);
                    }
                    else
                    {
                        Debug.LogWarning("PlacePrefabfromFrameDict: The value is null");
                    }
                }
            }
            else
            {
                Debug.LogWarning("PlacePrefabfromFrameDict: The dictionary is null");
            }
        }

        //TODO: Write a method to place one element from the provided frame information.
        public void InstantiatePrefabFromFrameData(string Key, Frame frame, GameObject prefabObject, GameObject ParentObject)
        {
            Debug.Log($"InstantiatePrefabFromFrameData: Instantiating prefab at {Key}");

            //get position
            Vector3 positionData = getPosition(frame.point);
            
            //get rotation
            Rotation rotationData = getRotation(frame.xaxis, frame.yaxis);
            
            //Define Object Rotation
            Quaternion rotationQuaternion = FromUnityRotation(rotationData);

            //Instantiate new gameObject from the existing selected gameobjects.
            GameObject elementPrefab = Instantiate(prefabObject, positionData, rotationQuaternion);

            //Set parent and name
            elementPrefab.transform.SetParent(ParentObject.transform, false);
            
            //Name the object afte the step number... might be better to get the step_id in the building plan from Chen.
            elementPrefab.name = Key;
        }
    
        //TODO: Write a method that will loop through all the children of the runtimeObject storage and move them by a vector
        public void MoveAllChildrenByVector(GameObject parentObject)
        {
            if (parentObject.transform.childCount > 0)
            {
                foreach (Transform child in parentObject.transform)
                {
                    MoveObjectbyVector(child.gameObject, child.transform.up, 0.001f);
                }
            }
        }

        //TODO: Write a method that will destroy all children inside of a parent object
        public void DestroyAllChildren(GameObject parentObject)
        {
            if (parentObject.transform.childCount > 0)
            {
                foreach (Transform child in parentObject.transform)
                {
                    Destroy(child.gameObject);
                }
            }
        }
        
        //TODO: Write a method that will move an object by a vector
        public void MoveObjectbyVector(GameObject gameObject, Vector3 vectorToMoveBy, float distanceToMove)
        {
            Vector3 newPosition = gameObject.transform.position + vectorToMoveBy * distanceToMove;
            gameObject.transform.position = newPosition;
        }

        //TODO: Write a method that will color an object by an input material
        public void ColorObjectbyInputMaterial(GameObject gamobj, Material material)
        {
            //Get Object Renderer
            Renderer m_renderer= gamobj.GetComponentInChildren<MeshRenderer>();
                      
            //Color object by input material
            m_renderer.material = material; 
        }

    /////////////////////////////// POSITION AND ROTATION ////////////////////////////////////////

        //Methods for position and rotation conversions from rhino to unity
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

        //Methods for position and rotation conversions from unity to rhino //TODO: I think this needs local position and some sort of local rotation. To accomodate for the Localizatoin
        public Dictionary<string, Frame> ConvertParentChildrenToRhinoFrameData(GameObject parentObject, Dictionary<string, Frame> frameDict)
        {
            //Create a new dictionary to store the rhino frame data
            Dictionary<string, Frame> rhinoFrameDict = new Dictionary<string, Frame>();

            //Loop through the dictionary and find the gameObject associated with the key
            foreach (KeyValuePair<string, Frame> entry in frameDict)
            {
                //Find the gameObject associated with the key
                GameObject gameObject = parentObject.FindObject(entry.Key);

                if (gameObject != null)
                {
                    //Convert GameObject information to Float Arrays
                    Frame frame = ConvertGameObjectToRhinoFrameData(gameObject);

                    //Add the frame to the dictionary
                    rhinoFrameDict.Add(entry.Key, frame);
                }
                else
                {
                    Debug.LogWarning($"ConvertParentChildrenToRhinoFrameData: GameObject with key {entry.Key} not found.");
                }
            }

            return rhinoFrameDict;
        }
        public Frame ConvertGameObjectToRhinoFrameData(GameObject gameObject)
        {
            //Convert GameObject information to Float Arrays
            (float[] pointData, float[] xaxisData, float[] yaxisData) = FromUnityToRhinoConversion(gameObject);

            //Construct a new frame object
            Frame frame = new Frame();
            frame.point = pointData;
            frame.xaxis = xaxisData;
            frame.yaxis = yaxisData;

            return frame;
        }
        public (float[], float[], float[]) FromUnityToRhinoConversion(GameObject gameObject)
        {
            //Convert position
            float[] position = setPosition(gameObject);

            //Convert rotation
            Rotation rotation = setRotation(gameObject);

            //Convert to Rhino Rotation
            (float[] x_vecdata, float[] y_vecdata) = LhToRh(rotation.x, rotation.z);

            return (position, x_vecdata, y_vecdata);
        }
        public float[] setPosition(GameObject gameObject)
        {
            //Get the position of the object and convert to float array
            Vector3 objectPosition = gameObject.transform.position;
            float [] objectPositionArray = new float[3] {objectPosition.x, objectPosition.y,  objectPosition.z};

            //Convert to Vector3
            float[] convertedPosition = new float [3] {objectPositionArray[0], objectPositionArray[2], objectPositionArray[1]};

            return convertedPosition;
        }
        public Rotation setRotation(GameObject gameObject)
        {
            //Convert x and z vectors to world space
            Vector3 objectWorldZ = transform.TransformDirection(gameObject.transform.forward);
            Vector3 objectWorldX = transform.TransformDirection(gameObject.transform.right);

            //Convert to float array
            float[] x_vecdata = new float[3] {objectWorldX.x, objectWorldX.y, objectWorldX.z};
            float[] z_vecdata = new float[3] {objectWorldZ.x, objectWorldZ.y, objectWorldZ.z};

            Vector3 x_vec_left = new Vector3(x_vecdata[0], x_vecdata[1], x_vecdata[2]);
            Vector3 z_vec_left  = new Vector3(z_vecdata[0], z_vecdata[1], z_vecdata[2]);
            
            Rotation rotationLH;
            
            rotationLH.x = x_vec_left;
            //This is never used just needed to satisfy struct code structure.
            rotationLH.y = Vector3.zero;
            rotationLH.z = z_vec_left;
            
            return rotationLH;
        } 
        public (float[], float[]) LhToRh(Vector3 x_vec_left, Vector3 z_vec_left)
        {        
            Vector3 x_vec = new Vector3(x_vec_left[0], x_vec_left[2], x_vec_left[1]);
            Vector3 y_vec = new Vector3(z_vec_left[0], z_vec_left[2], z_vec_left[1]);
            Vector3 z_vec = Vector3.Cross(y_vec, x_vec);

            float[] x_vecdata = new float[3] {x_vec_left[0], x_vec_left[2], x_vec_left[1]};
            float[] y_vecdata = new float[3] {z_vec_left[0], z_vec_left[2], z_vec_left[1]};

            return (x_vecdata, y_vecdata);
        }

    }
}
