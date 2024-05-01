using UnityEngine;
using CompasXR.Core.Data;

namespace CompasXR.Core
{
        public static class ObjectTransformations
    {
        //Struct for storing Rotation Values
        public struct Rotation
        {
            public Vector3 x;
            public Vector3 y;
            public Vector3 z;
        }
        public static Quaternion FromRhinotoUnityRotation(Rotation rotation, bool z_to_y_remapped)
        {   
            //Set Unity Rotation
            Rotation rotationLh = RightHandToLeftHand(rotation.x , rotation.y);

            Rotation Zrotation = ZRotation(rotationLh);

            Rotation ObjectRotation;

            if (!z_to_y_remapped == true)
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
        public static Quaternion FromUnityRotation(Rotation rotation)
        {   
            //Right hand to left hand conversion
            Rotation rotationLh = RightHandToLeftHand(rotation.x , rotation.y);

            //Set Unity Rotation
            Quaternion rotationQuaternion = GetQuaternion(rotationLh.y, rotationLh.z);

            return rotationQuaternion;
        } 
        public static Vector3 GetPositionFromRightHand(float[] pointlist)
        {
            Vector3 position = new Vector3(pointlist[0], pointlist[2], pointlist[1]);
            return position;
        }
        public static Rotation GetRotationFromRightHand(float[] x_vecdata, float [] y_vecdata)
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
        public static Rotation RightHandToLeftHand(Vector3 x_vec_right, Vector3 y_vec_right)
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
        public static Quaternion GetQuaternion(Vector3 y_vec, Vector3 z_vec)
        {
            Quaternion rotation = Quaternion.LookRotation(z_vec, y_vec);
            return rotation;
        }

        //Methods for obj imort correction.
        public static Rotation ZRotation(Rotation ObjectRotation)
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
        public static Rotation XRotation(Rotation ObjectRotation)
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
        public static Vector3 FindGameObjectCenter(GameObject gameObject)
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

        public static Vector3 OffsetPositionVectorByDistance(Vector3 position, float offsetDistance, string axis)
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
        public static void OffsetGameObjectPositionByExistingObjectPosition(GameObject gameObject, GameObject existingObject, float offsetDistance, string axis)
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

        //Methods for converting Unity GameObjects to Rhino Frame Data
        public static Frame ConvertGameObjectToRightHandFrameData(GameObject gameObject)
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
        public static (float[], float[], float[]) FromUnityToRhinoConversion(GameObject gameObject)
        {
            //Convert position
            float[] position = GetPositionFromLeftHand(gameObject);

            //Convert rotation
            Rotation rotation = GetRotationFromLeftHand(gameObject);

            //Convert to Rhino Rotation
            (float[] x_vecdata, float[] y_vecdata) = LeftHandToRightHand(rotation.x, rotation.z);

            return (position, x_vecdata, y_vecdata);
        }
        public static float[] GetPositionFromLeftHand(GameObject gameObject)
        {
            //Get the position of the object and convert to float array
            Vector3 objectPosition = gameObject.transform.position;
            float [] objectPositionArray = new float[3] {objectPosition.x, objectPosition.y,  objectPosition.z};

            //Convert to Vector3
            float[] convertedPosition = new float [3] {objectPositionArray[0], objectPositionArray[2], objectPositionArray[1]};

            return convertedPosition;
        }
        public static Rotation GetRotationFromLeftHand(GameObject gameObject)
        {
            //Convert x and z vectors to world space
            Vector3 objectWorldZ = gameObject.transform.TransformDirection(gameObject.transform.forward);
            Vector3 objectWorldX = gameObject.transform.TransformDirection(gameObject.transform.right);

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
        public static (float[], float[]) LeftHandToRightHand(Vector3 x_vec_left, Vector3 z_vec_left)
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
