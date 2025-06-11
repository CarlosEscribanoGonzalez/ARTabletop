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
            if (trackedImage.GetComponentInChildren<AGameUnit>() != null && trackedImage.GetComponentInChildren<AGameUnit>().InForceMaintain)
                trackedImage.gameObject.SetActive(true); //Si est� en ForceMaintian estar� siempre activo
            //Si no est� en ForceMaintain y el extended tracking est� desactivado cuando el marker se pierde de vista el objeto desaparece
            else if (!IsXTEnabled) trackedImage.gameObject.SetActive(trackedImage.trackingState == TrackingState.Tracking);
            else
            {

            }
        }
    }
}
