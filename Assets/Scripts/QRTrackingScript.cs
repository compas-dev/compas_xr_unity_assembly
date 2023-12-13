using System.Collections;
using System.Collections.Generic;
using JSON;
using Unity.Mathematics;
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


                    //TESTING
                    (Vector3 x_rh,Vector3 y_rh) = instantiateObjects.getRotation(QRCodeDataDict[key].xaxis, QRCodeDataDict[key].yaxis);
                    (Vector3 x_lh,Vector3 y_lh,Vector3 z_lh) = instantiateObjects.rhToLh(x_rh,y_rh);    
                    // (Vector3 x_lh,Vector3 y_lh,Vector3 z_lh) = QRrhToLh(x_rh,y_rh);
                    // (Vector3 x_lh_rot,Vector3 y_lh_rot,Vector3 z_lh_rot) = instantiateObjects.rotateVectors(x_lh,y_lh,z_lh);
                    // Quaternion rotation_data = instantiateObjects.rotateInstance(y_lh,z_lh);
                    Quaternion rotation_data = instantiateObjects.rotateInstance(y_lh, z_lh);
                    // Quaternion rotation_data = new Quaternion(QRCodeDataDict[key].quaternion[1],QRCodeDataDict[key].quaternion[2], QRCodeDataDict[key].quaternion[3], QRCodeDataDict[key].quaternion[0]);
                    

                    //Inverted both of these rotations.... I am not sure if this is meant to tell me a larger issue or not...
                    Quaternion rot = qrObject.transform.rotation * Quaternion.Inverse(rotation_data);//* qrObject.GetComponent<MarkerData>().MarkerQuatRotation; *instantiate //TODO: Check this rotation with multiple QR codes at weird angles.
                    Elements.transform.rotation = rot;

                    
                    pos = TranslatedPosition(qrObject, position_data, rotation_data);

                    
                    Debug.Log($"ROTATION Combined is:  {rot}");
                    // Debug.Log($"ROTATION data is:  {rotation_data}");
                    Debug.Log($"ROTATION qrObject is:  {qrObject.transform.rotation}");
                    
                    Elements.transform.position = pos;
                    Debug.Log($"TRANSFORMING ROTATION {qrObject.name}");
                }
            }
        }

    }

    private Vector3 TranslatedPosition(GameObject gobject, Vector3 position, Quaternion Individualrotation)
    {
        //Added individual rotation of the object... adding the objects rotation to the before moving its position.
        Vector3 pos = gobject.transform.position + (gobject.transform.rotation * Quaternion.Inverse(Individualrotation) * -position); //gobject.GetComponent<MarkerData>().translationVector
        return pos;
        
    }
    
    public void OnTrackingInformationReceived(object source, TrackingDataDictEventArgs e)
    {
        Debug.Log("Database is loaded." + " " + "Number of QR codes stored as a dict= " + e.QRCodeDataDict.Count);
        QRCodeDataDict = e.QRCodeDataDict;
    }

    public (Vector3, Vector3, Vector3) QRrhToLh(Vector3 x_vec_right, Vector3 y_vec_right)
    {        
        Vector3 x_vec = new Vector3(x_vec_right[0], x_vec_right[2], x_vec_right[1]);
        Vector3 z_vec = new Vector3(y_vec_right[0], y_vec_right[2], y_vec_right[1]);
        //TODO: This line below could be a problem line. places elements correctly for timbers, but I am not sure if it only works for timbers or all assemblies.
        Vector3 y_vec = Vector3.Cross(z_vec, x_vec).normalized;
        return (x_vec, y_vec, z_vec);
    }
                    

}
