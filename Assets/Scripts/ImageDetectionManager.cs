using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageDetectionManager : MonoBehaviour
{
    private ARTrackedImageManager imageManager;

    private void Start()
    {
        imageManager = GetComponent<ARTrackedImageManager>();
    }

    public void OnTrackedImagesChanged()
    {
        foreach(ARTrackable trackedImage in imageManager.trackables)
        {
            trackedImage.transform.gameObject.SetActive(trackedImage.trackingState == TrackingState.Tracking);
        }
    }
}
