using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Vuforia;
using UnityEngine.UI;
using CompasXR.UI;
using CompasXR.Core.Data;
using CompasXR.Core.Extentions;

namespace CompasXR.Core
{
    /*
    * CompasXR.Core : Is the Primary namespace for all Classes that
    * controll the primary functionalities of the CompasXR Application.
    */
    public class QRLocalization : MonoBehaviour
    {
        //Public GameObjects
        private GameObject Elements;
        private GameObject UserObjects;
        private GameObject ObjectLengthsTags;
        private GameObject PriorityViewerObjects;
        private GameObject ActiveRobotObjects;

        //Public Scripts
        public InstantiateObjects instantiateObjects;
        public UIFunctionalities uiFunctionalities;
        public DatabaseManager databaseManager;

        //Public Dictionaries
        public Dictionary<string, Node> QRCodeDataDict = new Dictionary<string, Node>();

        //In script use variables
        public Vector3 pos;

        private string lastQrName = "random";
        

        void Start()
        {   
            //Find Other scripts in the scene
            instantiateObjects = GameObject.Find("Instantiate").GetComponent<InstantiateObjects>();
            uiFunctionalities = GameObject.Find("UIFunctionalities").GetComponent<UIFunctionalities>();
            databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
            
            //Find GameObjects that need to be transformed
            Elements = GameObject.Find("Elements");
            UserObjects = GameObject.Find("ActiveUserObjects");
            ObjectLengthsTags = GameObject.Find("ObjectLengthsTags");
            PriorityViewerObjects = GameObject.Find("PriorityViewerObjects");
            ActiveRobotObjects = GameObject.Find("ActiveRobotObjects");

        }
        void Update()
        {

            if (QRCodeDataDict.Count > 0 && Elements != null)
            {
                pos = Vector3.zero;

                foreach (string key in QRCodeDataDict.Keys)
                {
                    GameObject qrObject = GameObject.Find("Marker_" + key);              

                    if (qrObject != null && qrObject.transform.position != Vector3.zero)
                    {
                        if(qrObject.name != lastQrName)
                        {
                            GameObject lastQrObject = GameObject.Find(lastQrName);

                            if (lastQrObject != null)
                            {
                                lastQrObject.transform.position = Vector3.zero;
                                lastQrObject.transform.rotation = Quaternion.identity;
                            }

                            lastQrName = qrObject.name;
                        }
                        
                        //Fetch position data from the dictionary
                        ObjectTransformations.Rotation rotationData = ObjectTransformations.GetRotationFromRightHand(QRCodeDataDict[key].part.frame.xaxis, QRCodeDataDict[key].part.frame.yaxis);
                        Quaternion rotationQuaternion = ObjectTransformations.FromUnityRotation(rotationData);
                        Quaternion rot = qrObject.transform.rotation * Quaternion.Inverse(rotationQuaternion);
                        
                        //Transform the rotation of game objects that need to be transformed
                        Elements.transform.rotation = rot;
                        UserObjects.transform.rotation = rot;
                        ObjectLengthsTags.transform.rotation = rot;
                        PriorityViewerObjects.transform.rotation = rot;
                        ActiveRobotObjects.transform.rotation = rot;

                        //Translate the position of the object based on the observed position and the inverse rotation of the physical QR
                        Vector3 positionData = ObjectTransformations.GetPositionFromRightHand(QRCodeDataDict[key].part.frame.point);
                        pos = TranslateGameObjectsPositionFromImageTarget(qrObject, positionData, rotationQuaternion);

                        //Set the position of the gameobjects object to the translated position                    
                        Elements.transform.position = pos;
                        UserObjects.transform.position = pos;
                        ObjectLengthsTags.transform.position = pos;
                        PriorityViewerObjects.transform.position = pos;
                        ActiveRobotObjects.transform.position = pos;

                        //Update position of line objects in the scene
                        if (uiFunctionalities.PriorityViewerToggleObject.GetComponent<Toggle>().isOn)
                        {
                            instantiateObjects.UpdatePriorityLine(uiFunctionalities.SelectedPriority ,instantiateObjects.PriorityViewrLineObject);
                        }
                        if (uiFunctionalities.ObjectLengthsToggleObject.GetComponent<Toggle>().isOn)
                        {
                            instantiateObjects.UpdateObjectLengthsLines(uiFunctionalities.CurrentStep, instantiateObjects.ObjectLengthsTags.FindObject("P1Tag"), instantiateObjects.ObjectLengthsTags.FindObject("P2Tag"));
                        }
                    }
                }
            }

        }
        private Vector3 TranslateGameObjectsPositionFromImageTarget(GameObject gobject, Vector3 position, Quaternion Individualrotation)
        {
            Vector3 pos = gobject.transform.position + (gobject.transform.rotation * Quaternion.Inverse(Individualrotation) * -position);
            return pos;
        }
        public void OnTrackingInformationReceived(object source, TrackingDataDictEventArgs e)
        {
            Debug.Log("Database is loaded." + " " + "Number of QR codes stored as a dict= " + e.QRCodeDataDict.Count);
            QRCodeDataDict = e.QRCodeDataDict;
        }

    }
}
