using System.Collections;
using System.Collections.Generic;
using JSON;
using UnityEngine;
using Vuforia;

public class QRTrackingScript : MonoBehaviour
{
    public string GameobjectName;
    private GameObject Elements;

    private InstantiateObjects instantiateObjects;

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
                
                Debug.Log("Looking Counter:" + key);
                Debug.Log("Looking for:" + qrObject.name +"and"+ qrObject.transform.position.ToString());
                

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
                    Vector3 position_data = instantiateObjects.getPosition(QRCodeDataDict[key].point);
                    pos = TranslatedPosition(qrObject, position_data);
                    Quaternion rot = qrObject.transform.rotation; //* qrObject.GetComponent<MarkerData>().MarkerQuatRotation; *instantiate //TODO: Check this rotation with multiple QR codes at weird angles.
                    Elements.transform.rotation = rot;
                    Elements.transform.position = pos;
                    Debug.Log($"TRANSFORMING ROTATION {qrObject.name}");
                }
            }
        }

    }

    private Vector3 TranslatedPosition(GameObject gobject, Vector3 position)
    {
        Vector3 pos = gobject.transform.position + (gobject.transform.rotation * -position); //gobject.GetComponent<MarkerData>().translationVector
        return pos;
        
    }
    
    public void OnTrackingInformationReceived(object source, TrackingDataDictEventArgs e)
    {
        Debug.Log("Database is loaded." + " " + "Number of QR codes stored as a dict= " + e.QRCodeDataDict.Count);
        QRCodeDataDict = e.QRCodeDataDict;
    }

    // //TESTING
    // (Vector3 x_rh,Vector3 y_rh,Vector3 z_rh) = instantiateObjects.getRotation(QRCodeDataDict[key].xaxis, QRCodeDataDict[key].yaxis);
    // (Vector3 x_lh,Vector3 y_lh,Vector3 z_lh) = instantiateObjects.rhToLh(x_rh,y_rh,z_rh);
    // Quaternion rotation_data = instantiateObjects.rotateInstance(x_lh,y_lh,z_lh);
                    

}
