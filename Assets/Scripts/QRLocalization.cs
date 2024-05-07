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
                        
                        ObjectTransformations.TranslateGameObjectByImageTarget(Elements, qrObject, QRCodeDataDict[key].part.frame.point, QRCodeDataDict[key].part.frame.xaxis, QRCodeDataDict[key].part.frame.yaxis);
                        ObjectTransformations.TranslateGameObjectByImageTarget(UserObjects, qrObject, QRCodeDataDict[key].part.frame.point, QRCodeDataDict[key].part.frame.xaxis, QRCodeDataDict[key].part.frame.yaxis);
                        ObjectTransformations.TranslateGameObjectByImageTarget(ObjectLengthsTags, qrObject, QRCodeDataDict[key].part.frame.point, QRCodeDataDict[key].part.frame.xaxis, QRCodeDataDict[key].part.frame.yaxis);
                        ObjectTransformations.TranslateGameObjectByImageTarget(PriorityViewerObjects, qrObject, QRCodeDataDict[key].part.frame.point, QRCodeDataDict[key].part.frame.xaxis, QRCodeDataDict[key].part.frame.yaxis);
                        ObjectTransformations.TranslateGameObjectByImageTarget(ActiveRobotObjects, qrObject, QRCodeDataDict[key].part.frame.point, QRCodeDataDict[key].part.frame.xaxis, QRCodeDataDict[key].part.frame.yaxis);

                        //Update position of line objects in the scene
                        if (uiFunctionalities.PriorityViewerToggleObject.GetComponent<Toggle>().isOn)
                        {
                            instantiateObjects.UpdatePriorityLine(uiFunctionalities.SelectedPriority ,instantiateObjects.PriorityViewrLineObject);
                        }
                        if (uiFunctionalities.ObjectLengthsToggleObject.GetComponent<Toggle>().isOn)
                        {
                            instantiateObjects.UpdateObjectLengthsLines(uiFunctionalities.CurrentStep, instantiateObjects.ObjectLengthsTags.FindObject("P1Tag"), instantiateObjects.ObjectLengthsTags.FindObject("P2Tag"));
                        }

                        Debug.Log($"QR: Translation from QR object: {qrObject.name}");
                    }
                }
            }

        }
        public void OnTrackingInformationReceived(object source, TrackingDataDictEventArgs e)
        {
            Debug.Log("Database is loaded." + " " + "Number of QR codes stored as a dict= " + e.QRCodeDataDict.Count);
            QRCodeDataDict = e.QRCodeDataDict;
        }

    }
}
