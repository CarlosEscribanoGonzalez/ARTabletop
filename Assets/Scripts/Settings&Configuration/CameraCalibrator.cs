using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class CameraCalibrator : MonoBehaviour
{
    [SerializeField] private ARCameraManager arCameraManager; //Cámara asociada a la cámara del dispositivo

    void Start()
    {
        arCameraManager.frameReceived += OnCameraFrameReceived; //Cuando recibe un frame de la cámara del dispositivo esta se calibra
    }

    void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        if (args.projectionMatrix.HasValue) 
        {
            Matrix4x4 projection = args.projectionMatrix.Value; //Accede a la matriz de proyección
            //Cambia el factor de escala de las PlayableUnits según la matriz de proyección
            //De esta forma los objetos se ven del mismo tamaño en distintos dispositivos
            PlayableUnit.ScaleCameraFactor = 1/projection.m00; 
            //Una vez se ha calculado ScaleCameraFactor la función se desuscribe del evento y el componente se desactiva
            //Para evitar consumo de recursos innecesarios
            arCameraManager.frameReceived -= OnCameraFrameReceived;
            this.enabled = false;
        }
    }
}