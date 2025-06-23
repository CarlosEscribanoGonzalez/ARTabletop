using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class CameraCalibrator : MonoBehaviour
{
    [SerializeField] private ARCameraManager arCameraManager; //C�mara asociada a la c�mara del dispositivo
    private Camera secondCam;
    private ScreenOrientation prevOrientation;

    void Start()
    {
        secondCam = GameObject.FindWithTag("SecondCam").GetComponent<Camera>();
        arCameraManager.frameReceived += OnCameraFrameReceived; //Cuando recibe un frame de la c�mara del dispositivo esta se calibra
        prevOrientation = Screen.orientation;
    }

    private void Update()
    {
        if(Screen.orientation != prevOrientation)
        {
            prevOrientation = Screen.orientation;
            arCameraManager.frameReceived += UpdateSecondCam;
        }
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        if (args.projectionMatrix.HasValue) 
        {
            Matrix4x4 projection = args.projectionMatrix.Value; //Accede a la matriz de proyecci�n
            float currentFoV = 2f * Mathf.Atan(1f / projection[1, 1]) * Mathf.Rad2Deg;
            float fovFactor = Mathf.Tan(Mathf.Deg2Rad * 60 / 2f) /
                           Mathf.Tan(Mathf.Deg2Rad * currentFoV / 2f);
            FindFirstObjectByType<XROrigin>().transform.localScale = Vector3.one * fovFactor;
            PlayableUnit.ScaleCameraFactor = 1 / fovFactor;
            arCameraManager.frameReceived -= OnCameraFrameReceived;
            secondCam.projectionMatrix = projection;
        }
    }

    private void UpdateSecondCam(ARCameraFrameEventArgs args)
    {
        Debug.Log("Second cam proj matrix updated");
        secondCam.projectionMatrix = args.projectionMatrix.Value;
        arCameraManager.frameReceived -= UpdateSecondCam;
    }
}