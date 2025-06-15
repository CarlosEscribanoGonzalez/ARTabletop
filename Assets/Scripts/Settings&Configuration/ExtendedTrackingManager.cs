using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ExtendedTrackingManager : MonoBehaviour
{
    private static bool isXTEnabled = false;
    private ARTrackedImageManager imageManager;
    private static ARPlaneManager planeManager;
    private static ARAnchorManager anchorManager;
    public static bool IsXTEnabled
    {
        get { return isXTEnabled; }
        set
        {
            isXTEnabled = value;
            if (planeManager != null)
            {
                planeManager.enabled = value;
                if (value) ResetPlanesAndAnchors();
            }
        }
    }
    public static bool ISXTReady => planeManager.trackables.count > 0;

    private void Start()
    {
        imageManager = GetComponent<ARTrackedImageManager>();
        planeManager = GetComponent<ARPlaneManager>();
        anchorManager = GetComponent<ARAnchorManager>();
        FindFirstObjectByType<ARPlaneManager>().enabled = isXTEnabled;
    }

    private void OnApplicationFocus(bool focus)
    {
        Debug.Log("APPLICATION FOCUS: " + focus);
        if (IsXTEnabled && focus) StartCoroutine(ResetCoroutine());
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

    private static void ResetPlanesAndAnchors()
    { 
        foreach (ARPlane p in planeManager.trackables) Destroy(p.gameObject);
        foreach (ARAnchor a in anchorManager.trackables) Destroy(a.gameObject);
        FindFirstObjectByType<ARSession>()?.Reset();
    }

    IEnumerator ResetCoroutine()
    {
        IsXTEnabled = false;
        ResetPlanesAndAnchors();
        yield return null;
        IsXTEnabled = true;
    }
}
