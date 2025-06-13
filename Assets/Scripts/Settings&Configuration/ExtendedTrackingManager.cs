using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ExtendedTrackingManager : MonoBehaviour
{
    public static bool IsXTEnabled { get; set; } = false;
    private ARTrackedImageManager imageManager;
    private ARPlaneManager planeManager;
    private ARAnchorManager anchorManager;
    private ARAnchor anchor;

    private void Start()
    {
        imageManager = GetComponent<ARTrackedImageManager>();
        anchorManager = FindFirstObjectByType<ARAnchorManager>();
        planeManager = FindFirstObjectByType<ARPlaneManager>();
    }

    public void OnTrackedImagesChanged() //Llamada cada vez que un marcador se actualiza
    {
        foreach(ARTrackable trackedImage in imageManager.trackables)
        {
            if (trackedImage.GetComponentInChildren<AGameUnit>() != null && trackedImage.GetComponentInChildren<AGameUnit>().InForceMaintain)
                trackedImage.gameObject.SetActive(true); //Si está en ForceMaintian estará siempre activo
            //Si no está en ForceMaintain y el extended tracking está desactivado cuando el marker se pierde de vista el objeto desaparece
            else if (!IsXTEnabled) trackedImage.gameObject.SetActive(trackedImage.trackingState == TrackingState.Tracking);
            if (true) return;
            else
            {
                if (planeManager.trackables.count == 0 || trackedImage.trackingState == TrackingState.Tracking)
                {
                    anchor = null;
                    return;
                }
                else
                {
                    if (anchor != null) return;
                    ARPlane nearestPlane = null;
                    float minDistance = 1000;
                    foreach (ARPlane plane in planeManager.trackables)
                    {
                        if (Vector3.Distance(plane.transform.position, trackedImage.transform.position) < minDistance)
                        {
                            nearestPlane = plane;
                            minDistance = Vector3.Distance(plane.transform.position, trackedImage.transform.position);
                        }
                    }
                    Debug.Log("PLANO MÁS CERCANO: " + nearestPlane.name);
                    Pose pose = new Pose(nearestPlane.transform.position, Quaternion.identity);
                    anchor = anchorManager.AttachAnchor(nearestPlane, pose);

                    Debug.Log("ANCHOR: " + anchor);
                    if (anchor != null)
                    {
                        trackedImage.transform.SetParent(anchor.transform);
                        Debug.Log("OBJETO PUESTO CON PLANO COMO PADRE");
                        //trackedImage.transform.localPosition = Vector3.zero;
                        //transform.localScale /= 10;
                    }
                    else Debug.LogError("ERROR: ANCHOR ES NULO");
                }
            }
        }
    }

    public void OnPlaneDetected()
    {
        Debug.Log("PLANE DETECTED: " + planeManager.trackables.count);
        if (planeManager.trackables.count >= 3) planeManager.enabled = false;
    }
}
