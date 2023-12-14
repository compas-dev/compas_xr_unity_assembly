using System.Collections;
using System.Collections.Generic;
using JSON;
using Unity.Mathematics;
using UnityEngine;
using Vuforia;
using Instantiate;

public class QRTrackingScript : MonoBehaviour
{
    public string GameobjectName;
    private GameObject Elements;

    public InstantiateObjects instantiateObjects;

    public Dictionary<string, QRcode> QRCodeDataDict = new Dictionary<string, QRcode>();

    public List<Vector3> positions;

    public Vector3 pos;

    private string lastQrName = "random";
    
    // Start is called before the first frame update
    void Start()
    {   
        //Find the Instantiate Objects game object to call methods from inside the script
        instantiateObjects = GameObject.Find("Instantiate").GetComponent<InstantiateObjects>();
        
        //Find Elements Game Object
        Elements = GameObject.Find("Elements");
    }

    void Update()
    {

        if (QRCodeDataDict.Count > 0 && Elements != null)
        {
            pos = Vector3.zero;
            Debug.Log("QRCodeDataDict.Count > 0" + QRCodeDataDict.Count);
            Debug.Log($"Elements is: {Elements}");

            foreach (string key in QRCodeDataDict.Keys)
            {
                GameObject qrObject = GameObject.Find("Marker_" + key);
                Debug.Log($"YOUR KEY IS {key}");                

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
                    Vector3 position_data = instantiateObjects.getPosition(QRCodeDataDict[key].point);

                    //Fetch rotation data from the dictionary
                    InstantiateObjects.Rotation rotationData = instantiateObjects.getRotation(QRCodeDataDict[key].xaxis, QRCodeDataDict[key].yaxis);
                    
                    Quaternion rotationQuaternion = instantiateObjects.FromRhinoToUnity(rotationData, false);
                    
                    // (Vector3 x_lh,Vector3 y_lh,Vector3 z_lh) = instantiateObjects.rhToLh(x_rh,y_rh);
                    // Quaternion rotationQuaternion = instantiateObjects.rotateInstance(y_lh, z_lh);
                    // Quaternion rotationQuaternion

                    //Set Design Objects rotation to the rotation based on Observed rotation and Inverse rotation of physical QR
                    Quaternion rot = qrObject.transform.rotation * Quaternion.Inverse(rotationQuaternion);
                    Elements.transform.rotation = rot;

                    //Translate the position of the object based on the observed position and the inverse rotation of the physical QR
                    pos = TranslatedPosition(qrObject, position_data, rotationQuaternion);

                    //Set the position of the design object to the translated position                    
                    Elements.transform.position = pos;

                    Debug.Log($"QR: Translation from QR object: {qrObject.name}");
                }
            }
        }

    }

    private Vector3 TranslatedPosition(GameObject gobject, Vector3 position, Quaternion Individualrotation)
    {
        //Position determined by positioning the QR codes observed position and rotation and translating by position vector and the inverse of the QR rotation
        Vector3 pos = gobject.transform.position + (gobject.transform.rotation * Quaternion.Inverse(Individualrotation) * -position);
        return pos;
    }
    
    public void OnTrackingInformationReceived(object source, TrackingDataDictEventArgs e)
    {
        Debug.Log("Database is loaded." + " " + "Number of QR codes stored as a dict= " + e.QRCodeDataDict.Count);
        QRCodeDataDict = e.QRCodeDataDict;
    }
                    

}
