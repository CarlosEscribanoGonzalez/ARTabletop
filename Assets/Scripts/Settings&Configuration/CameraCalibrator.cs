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
            //Cambia el factor de escala de las PlayableUnits seg�n la matriz de proyecci�n
            //De esta forma los objetos se ven del mismo tama�o en distintos dispositivos
            if (Screen.width <= Screen.height) //Depende de la orientaci�n se toman valores distintos de la matriz para la escala de los objetos
                PlayableUnit.ScaleCameraFactor = 1 / projection.m00; //Portrait coge la escala horizontal
            else PlayableUnit.ScaleCameraFactor = 1 / projection.m11; //Landscape la vertical
            //Una vez se ha calculado ScaleCameraFactor la funci�n se desuscribe del evento y el componente se desactiva
            //Para evitar consumo de recursos innecesarios
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