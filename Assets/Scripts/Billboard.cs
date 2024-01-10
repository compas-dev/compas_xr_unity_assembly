using UnityEngine;

public class Billboard : MonoBehaviour
{
    //TODO: Put this somewhere else. Put in instantiate objects... might need to be in the update.
    void LateUpdate()
    {
        if (Camera.main != null)
        {
            // Make the text face the camera
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
        }
    }
}
