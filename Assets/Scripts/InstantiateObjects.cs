using System.Collections.Generic;
using UnityEngine;
using JSON;
using Helpers;

//Notes: The entirity of the file is structured as a class that inherits from MonoBehaviour (Unity's base class for scripts)
//Notes: Script is used for instantiating and controling Objects, position, rotation, and color.

namespace Instantiate
{
    public class InstantiateObjects : MonoBehaviour
    {
        //Notes: Class Member Variables, used to store data and references ex. public DatabaseReference dbreferenceMyFramesData;
        //Notes: The public keyword is named an access modifier, it determines how Member variables, methods, and classes can be accessed from other classes.

        //Notes: Given: Struct for storing Rotation Values...
        /*
            - A struct (short for "structure") is a composite data type in programming that groups together variables of different data types. 
            - It allows for the creation of custom data structures encapsulating related data. 
            - Structs are typically lightweight and efficient, commonly used for small, self-contained data representations.
        */
        public struct Rotation
        {
            public Vector3 x;
            public Vector3 y;
            public Vector3 z;
        }

        //TODO: MAS: 4. To store parent object that contains our children    
        public GameObject MyRuntimeParentStorageObject;

        //TODO: MAS: 4. Create Reference to my prefab object
        public GameObject MyPrefabObject;

        //Notes: Built in Unity methods (methods that come from the inheritance of the MonoBehaviour class)
        /*
            Notes: Awake is called when the script instance is being loaded. This occurs before any Start methods are called.
            Notes: Start is called on the frame when a script is enabled just before any of the Update methods are called the first time.
            Notes: Update is called every frame, if the MonoBehaviour is enabled.
            Notes: OnDestroy is called when the MonoBehaviour will be destroyed.
            Notes: OnEnable is called when the object becomes enabled and active.
        */
        void Awake()
        {
            //Method called as a wrapper for all conditions that have to happen in the awake.
            OnAwakeInitilization();
        }
        
    /////////////////////////////// Initilization and Set up Actions //////////////////////////////
    
        public void OnAwakeInitilization()
        {
            //TODO: MAS: 4. Find the runtime object storage object
            MyRuntimeParentStorageObject = GameObject.Find("MyRunTimeObjectStorage");

            //TODO: MAS: 4. Find the prefab object
            MyPrefabObject = GameObject.Find("MyPrefabObject");

            if(MyRuntimeParentStorageObject != null || MyPrefabObject != null)
            {
                Debug.Log("Objects are not null");
            }
            else
            {
                Debug.LogWarning("An Object is null");
            }
        }

    /////////////////////////////// Multiple Object Methods ///////////////////////////////////////
        
        //TODO: MAS: 5. Write logic to iterate through the dictionary and place an element for each object in our dictionary.
        public void PlacePrefabsfromFrameDict(Dictionary<string, Frame> MyFrameDictionary, GameObject prefabObject, GameObject ParentObject)
        {
            //Check if the frame dictionary is null
            if (MyFrameDictionary != null)
            {
                
                //loop through the dictionary and place the prefab
                foreach(KeyValuePair<string, Frame> frame in MyFrameDictionary)
                {
                    //Instantiate prefab object
                    InstantiatePrefabFromFrameData(frame.Key, frame.Value, prefabObject, ParentObject);
                }

            }
            else
            {
                //GIVE ME A WARNING.
                Debug.LogWarning("My Frame Dictionary is null");
            }
        }
        public void MoveAllChildrenByVector(GameObject parentObject)
        {
            //Write an if condition to check if the parent object has children
            if (parentObject.transform.childCount > 0)
            {
                //Write A for loop to iterate through all of the children in the parent object.
                foreach (Transform child in parentObject.transform)
                {
                    //Call the method to move the object by a vector.
                    MoveObjectbyVector(child.gameObject, child.transform.up, 0.001f);
                }
            }
        }

        //TODO: MAS: 7. Write logic for a method that will destroy all children inside of a parent object
        public void DestroyAllChildren(GameObject parentObject)
        {
            //Write an if condition to check if the parent object has children
            if(parentObject.transform.childCount > 0)
            {
                //Write A for loop to iterate through all of the children in the parent object.
                foreach(Transform child in parentObject.transform)
                {
                    //call a method to destroy the game object (Hint: use the Destroy method from Unity's API)
                    Destroy(child.gameObject);
                }
            }
        }

    /////////////////////////////// Single GameObject Methods ////////////////////////////////////////
        public void InstantiatePrefabFromFrameData(string Key, Frame frame, GameObject prefabObject, GameObject ParentObject)
        {
            Debug.Log($"InstantiatePrefabFromFrameData: Instantiating prefab at {Key}");

            //Checking if any of the objects are null
            if(prefabObject == null || ParentObject == null || frame == null || Key == null)
            {
                Debug.LogError("InstantiatePrefabFromFrameData: One of your inputs is null");
                return;
            }

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
        public void MoveObjectbyVector(GameObject gameObject, Vector3 vectorToMoveBy, float distanceToMove)
        {
            Vector3 newPosition = gameObject.transform.position + vectorToMoveBy * distanceToMove;
            gameObject.transform.position = newPosition;
        }

        //TODO: MAS: (IF We Have time) Write a method that will color an object by an input material
        public void ColorObjectbyInputMaterial(GameObject gamobj, Material material)
        {
            //Get the MeshRenderer component from the game object
                      
            //Assign the MeshRenderer the material

        }

    /////////////////////////////// POSITION AND ROTATION ////////////////////////////////////////

        //Notes: Given: All methods in this section are used to convert position and rotation data from Unity to Rhino and vice versa...

        //Notes: Methods for position and rotation conversions from rhino to unity
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

        //Notes: Methods for obj imort correction.
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

        //Notes: Methods for position and rotation conversions from unity to rhino
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
