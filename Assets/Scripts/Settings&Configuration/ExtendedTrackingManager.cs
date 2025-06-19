using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;

public class ExtendedTrackingManager : MonoBehaviour
{
    [SerializeField] private Canvas rngButtonsCanvas;
    private static bool isXTEnabled = false;
    private ARTrackedImageManager imageManager;
    private static ARPlaneManager planeManager;
    private static ARAnchorManager anchorManager;
    private static Canvas msgCanvas;
    private static Volume dofVolume;
    private Dictionary<Canvas, bool> allCanvasStates = new();
    public static bool IsXTEnabled
    {
        get { return isXTEnabled; }
        set
        {
            isXTEnabled = value;
            if (planeManager != null)
            {
                planeManager.enabled = value;
                msgCanvas.enabled = value;
                //dofVolume.enabled = value;
                if (value)
                {
                    ResetPlanesAndAnchors();
                    detected = false;
                    FindFirstObjectByType<Settings>().GetComponent<Canvas>().enabled = false;
                }
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
        rngButtonsCanvas.enabled = true;
        msgCanvas = GetComponentInChildren<Canvas>();
        dofVolume = GetComponentInChildren<Volume>();
        IsXTEnabled = isXTEnabled;
        foreach(var canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (canvas == msgCanvas) continue;
            allCanvasStates.Add(canvas, canvas.enabled);
            canvas.enabled = false;
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus && IsXTEnabled) StartCoroutine(ResetCoroutine()); 
    }

    public void OnTrackedImagesChanged() //Llamada cada vez que un marcador se actualiza
    {
        foreach(ARTrackable trackedImage in imageManager.trackables)
        {
            if (trackedImage.GetComponent<PlayableUnit>() == null) continue;
            trackedImage.GetComponent<PlayableUnit>().ManageTracking(trackedImage);
        }
    }

    private static bool detected = false;
    public void OnPlaneDetected()
    {
        if (detected || planeManager.trackables.count == 0) return;
        foreach (var canvas in allCanvasStates.Keys)
        {
            canvas.enabled = allCanvasStates[canvas];
        }
        msgCanvas.enabled = false;
        dofVolume.enabled = false;
        detected = true;
        Debug.Log("PLANE DETECTED: " + GetComponent<ARPlaneManager>().trackables.count);
    }

    private static void ResetPlanesAndAnchors()
    {
        int initPlanes = planeManager.trackables.count;
        foreach (ARPlane p in planeManager.trackables) if(p != null) Destroy(p.gameObject);
        foreach (ARAnchor a in anchorManager.trackables) if(a != null) Destroy(a.gameObject);
        if(initPlanes > 0) FindFirstObjectByType<ARSession>()?.Reset();
    }

    IEnumerator ResetCoroutine()
    {
        foreach(var canvas in allCanvasStates.Keys.ToList())
        {
            allCanvasStates[canvas] = canvas.enabled;
            canvas.enabled = false;
        }
        IsXTEnabled = false;
        ResetPlanesAndAnchors();
        yield return null;
        IsXTEnabled = true;
    }
}
