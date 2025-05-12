using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Transform cameraTransform;
    
    void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        this.transform.forward = (this.transform.position - cameraTransform.position).normalized;
    }
}
