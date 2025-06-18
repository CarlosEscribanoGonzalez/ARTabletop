using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class DebugOnAwake : MonoBehaviour
{
    void Awake()
    {
        Debug.Log(transform.parent.GetComponent<ARTrackedImage>().referenceImage.name);
    }
}
