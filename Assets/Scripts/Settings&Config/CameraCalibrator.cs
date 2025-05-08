using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class CameraCalibrator : MonoBehaviour
{
    [SerializeField] private ARCameraManager arCameraManager;

    void Start()
    {
        if (arCameraManager != null)
        {
            arCameraManager.frameReceived += OnCameraFrameReceived;
        }
    }

    void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        if (args.projectionMatrix.HasValue)
        {
            Matrix4x4 projection = args.projectionMatrix.Value;
            PlayableUnit.ScaleCameraFactor = 1/projection.m00;
            arCameraManager.frameReceived -= OnCameraFrameReceived;
            this.enabled = false;
        }
    }
}