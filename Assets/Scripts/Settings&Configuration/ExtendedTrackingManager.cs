using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ExtendedTrackingManager : MonoBehaviour
{
    private static bool isXTEnabled = false;
    public static bool IsXTEnabled { 
        get { return isXTEnabled; } 
        set { isXTEnabled = value; 
            if(FindFirstObjectByType<ARPlaneManager>() != null)
                FindFirstObjectByType<ARPlaneManager>().enabled = IsXTEnabled; } }
    private ARTrackedImageManager imageManager;

    private void Start()
    {
        imageManager = GetComponent<ARTrackedImageManager>();
        FindFirstObjectByType<ARPlaneManager>().enabled = isXTEnabled;
    }

    public void OnTrackedImagesChanged() //Llamada cada vez que un marcador se actualiza
    {
        foreach(ARTrackable trackedImage in imageManager.trackables)
        {
            if (trackedImage.GetComponent<PlayableUnit>() == null) continue;
            trackedImage.GetComponent<PlayableUnit>().ManageTracking(trackedImage);
        }
    }

    private bool detected = false;
    public void OnPlaneDetected()
    {
        if (detected) return;
        detected = true;
        Debug.Log("PLANE DETECTED: " + GetComponent<ARPlaneManager>().trackables.count);
    }
}
