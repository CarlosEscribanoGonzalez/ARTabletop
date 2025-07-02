using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ExtendedTrackingManager : MonoBehaviour
{
    [SerializeField] private Canvas rngButtonsCanvas;
    private static bool isXTEnabled = false;
    private ARTrackedImageManager imageManager;
    private static ARPlaneManager planeManager;
    private static ARAnchorManager anchorManager;
    private static Canvas msgCanvas;
    private static Dictionary<Canvas, bool> allCanvasStates = new();
    private static Dictionary<GameObject, bool> allCamsStates = new(); //Cámaras rng del dado y la moneda
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
                if (value)
                {
                    ResetPlanesAndAnchors();
                    detected = false;
                }
            }
        }
    }
    public static bool IsXTReady => planeManager.trackables.count > 0;

    private void Start()
    {
        imageManager = GetComponent<ARTrackedImageManager>();
        planeManager = GetComponent<ARPlaneManager>();
        anchorManager = GetComponent<ARAnchorManager>();
        FindFirstObjectByType<ARPlaneManager>().enabled = isXTEnabled;
        rngButtonsCanvas.enabled = true;
        msgCanvas = GetComponentInChildren<Canvas>();
        IsXTEnabled = isXTEnabled;
        allCanvasStates.Clear();
        foreach(var canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (canvas == msgCanvas) continue;
            allCanvasStates.Add(canvas, canvas.enabled);
            if(IsXTEnabled) canvas.enabled = false;
        }
        allCamsStates.Clear();
        foreach(var rngCam in FindObjectsByType<RNGCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            allCamsStates.Add(rngCam.gameObject, rngCam.gameObject.activeSelf);
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus && IsXTEnabled && IsXTReady) StartCoroutine(ResetCoroutine()); 
    }

    public void OnTrackedImagesChanged() //Llamada cada vez que un marcador se actualiza
    {
        foreach(ARTrackable trackedImage in imageManager.trackables)
        {
            if (trackedImage.GetComponent<PlayableUnit>() == null) continue;
            trackedImage.GetComponent<PlayableUnit>().ManageTracking(trackedImage);
        }
    }

    public void OnConfigCanceled()
    {
        IsXTEnabled = false;
        RestoreCanvas();
        FindFirstObjectByType<Settings>().SetXTToggle(false);
    }

    private static bool detected = false;
    public void OnPlaneDetected()
    {
        if (detected || planeManager.trackables.count == 0) return;
        RestoreCanvas();
        msgCanvas.enabled = false;
        detected = true;
        Debug.Log("PLANE DETECTED: " + GetComponent<ARPlaneManager>().trackables.count);
    }

    private static void RestoreCanvas()
    {
        foreach (var canvas in allCanvasStates.Keys)
        {
            canvas.enabled = allCanvasStates[canvas];
        }
        foreach(var cam in allCamsStates.Keys)
        {
            cam.SetActive(allCamsStates[cam]);
        }
    }

    private static void ResetPlanesAndAnchors()
    {
        int initPlanes = planeManager.trackables.count;
        foreach (ARPlane p in planeManager.trackables) if(p != null) Destroy(p.gameObject);
        foreach (ARAnchor a in anchorManager.trackables) if(a != null) Destroy(a.gameObject);
        if(initPlanes > 0) FindFirstObjectByType<ARSession>()?.Reset();
        //Si todos los canvas están apagados significa que ResetPlanesAndAnchors ya fue llamado, así que su estado previo no se reescribe
        if (allCanvasStates.Keys.Where((c) => c.enabled == false).Count() == allCanvasStates.Keys.Count) 
            return;
        foreach (var canvas in allCanvasStates.Keys.ToList())
        {
            allCanvasStates[canvas] = canvas.enabled;
            canvas.enabled = false;
        }
        foreach(var cam in allCamsStates.Keys.ToList())
        {
            allCamsStates[cam] = cam.activeSelf;
            cam.SetActive(false);
        }
    }

    IEnumerator ResetCoroutine()
    {
        IsXTEnabled = false;
        ResetPlanesAndAnchors();
        yield return null;
        IsXTEnabled = true;
    }
}
