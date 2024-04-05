using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Firebase.Database;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;
using JSON;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.InputSystem;


namespace Helpers
{
    public static class HelpersExtensions
    { 
        public static GameObject FindObject(this GameObject parent, string name)
        {
            Transform[] trs= parent.GetComponentsInChildren<Transform>(true);
            foreach(Transform t in trs){
                if(t.name == name){
                    return t.gameObject;
                }
            }
            return null;
        }

        public static float Remap(float from, float fromMin, float fromMax, float toMin,  float toMax)
        {
            var fromAbs  =  from - fromMin;
            var fromMaxAbs = fromMax - fromMin;      
        
            var normal = fromAbs / fromMaxAbs;

            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;

            var to = toAbs + toMin;
        
            return to;
        }

        public static bool IsPointerOverUIObject(Vector2 touchPosition)
        {
            //checking if we are touching a button
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = touchPosition;
            List<RaycastResult> raycastResults = new List<RaycastResult>();

            EventSystem.current.RaycastAll(pointerEventData, raycastResults);

            if (raycastResults.Count > 0)
            {
                Debug.Log($"Touch: Pointer over a UI element > 0 {raycastResults.Count}");
            }
            else
            {
                Debug.Log($"Touch: Pointer over a UI element NOT > 0 {raycastResults.Count}");
            }

            return raycastResults.Count > 0;
        }

        public static void FaceObjectToCamera(Transform transform)
        {
            if (Camera.main != null)
            {
                transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
            }
        }

        // Billboard class encapsulated within the GameObjectExtensions namespace
        public class Billboard : MonoBehaviour
        {
            void LateUpdate()
            {
                // Access the FaceObjectToCamera method from the same namespace
                FaceObjectToCamera(transform);
            }
        }

        //Class for storing the position, rotation and scale of an object
        public class ObjectPositionInfo : MonoBehaviour
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;

            public ObjectPositionInfo(Vector3 position, Quaternion rotation, Vector3 scale)
            {
                this.position = position;
                this.rotation = rotation;
                this.scale = scale;
            }

            public void StorePositionRotationScale(Vector3 position, Quaternion rotation, Vector3 scale)
            {
                this.position = position;
                this.rotation = rotation;
                this.scale = scale;
            }
        }
    }   

}

