using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QRTrackingScript : MonoBehaviour
{
    public string GameobjectName;
    public GameObject Elements;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Vector3 TranslatedPosition(GameObject gobject)
    {
        Vector3 pos = gobject.transform.position + (gobject.transform.rotation * -gobject.transform.position);
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
