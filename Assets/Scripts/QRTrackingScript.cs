using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class QRTrackingScript : MonoBehaviour
{
    public string GameobjectName;
    public GameObject Elements;

    public List<Vector3> positions;

    public Vector3 pos;

    private string lastQrName = "random";
    
    // Start is called before the first frame update
    void start()
    {   

    }

    // Update is called once per frame
    void Update()
    {
        // positions = new List<Vector3>();
        pos = Vector3.zero;
        // Debug.Log("New Update");

        //TODO: Change to make this looking for range of number of children.
        for (int i = 0; i < 6; i++)
        {
            GameObject qrObject = GameObject.Find("Marker_" + i.ToString());
            
            Debug.Log("Looking Counter:" + i.ToString());
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
                pos = TranslatedPosition(qrObject);
                Quaternion rot = qrObject.transform.rotation; //* qrObject.GetComponent<MarkerData>().MarkerQuatRotation;
                Elements.transform.rotation = rot;
                Elements.transform.position = pos;
                Debug.Log($"TRANSFORMING ROTATION {qrObject.name}");
            }
        }

    }

    private Vector3 TranslatedPosition(GameObject gobject)
    {
        Vector3 pos = gobject.transform.position + (gobject.transform.rotation * gobject.GetComponent<MarkerData>().translationVector);
        return pos;
        
    }
    
    public void printQRCode(string GameobjectName)
    {
        Debug.Log("QR Code Detected");
        Debug.Log(GameobjectName);
        string objectName = gameObject.name;
        Debug.Log("QR Code Detected on " + objectName);
    }
}
