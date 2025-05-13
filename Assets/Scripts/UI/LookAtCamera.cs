using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Transform cameraTransform; //Transform de la cámara principal
    
    void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        //Se actualiza el forward del objeto para que este siempre "mire" hacia la cámara
        this.transform.forward = (this.transform.position - cameraTransform.position).normalized; 
    }
}
