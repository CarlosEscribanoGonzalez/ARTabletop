using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ExtendedTrackingManager : MonoBehaviour
{
    private ARTrackedImageManager imageManager;
    private ARAnchorManager anchorManager;

    private void Start()
    {
        imageManager = GetComponent<ARTrackedImageManager>();
        anchorManager = FindFirstObjectByType<ARAnchorManager>();
    }

    public void OnTrackedImagesChanged() //Llamada cada vez que un marcador se actualiza
    {
        foreach(ARTrackable trackedImage in imageManager.trackables)
        {
            if (trackedImage.GetComponentInChildren<AGameUnit>() != null && trackedImage.GetComponentInChildren<AGameUnit>().InForceMaintain) 
                trackedImage.transform.gameObject.SetActive(true); //Si est� en ForceMaintian estar� siempre activo
            //Si no est� en ForceMaintain y el extended tracking est� desactivado cuando el marker se pierde de vista el objeto desaparece
            else if (!GameSettings.Instance.ExtendedTracking) trackedImage.transform.gameObject.SetActive(trackedImage.trackingState == TrackingState.Tracking);
            /*else
            {
                if (trackedImage.trackingState != TrackingState.Tracking && !trackedImage.transform.parent.TryGetComponent<ARAnchor>(out _))
                {
                    
                }
                else if (trackedImage.trackingState == TrackingState.Tracking) 
                {
                    
                }
            }*/
        }
    }
}
