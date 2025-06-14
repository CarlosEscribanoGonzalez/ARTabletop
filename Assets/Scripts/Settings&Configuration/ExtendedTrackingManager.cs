using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ExtendedTrackingManager : MonoBehaviour
{
    public static bool IsXTEnabled { get; set; } = false;
    private ARTrackedImageManager imageManager;

    private void Start()
    {
        imageManager = GetComponent<ARTrackedImageManager>();
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
