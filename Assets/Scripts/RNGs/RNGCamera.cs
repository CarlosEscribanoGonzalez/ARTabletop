using UnityEngine;

public class RNGCamera : MonoBehaviour
{
    [SerializeField] private float portraitDistance = 13f; //Posici�n local en Y cuando Screen est� en portrait
    [SerializeField] private float landscapeDistance = 7.75f; //Posic�n local en Y cuando Screen est� en landscape
    [SerializeField] private Vector3 portraitRotation = new Vector3(90, 90, 0); //Rotaci�n de la c�mara en portrait
    [SerializeField] private Vector3 landscapeRotation = new Vector3(90, 0, 0); //Rotaci�n de la c�mara en landscape
    private Camera cam; //C�mara que apunta a los dados
    private Canvas camBackground; //Canvas en screen space - camera que contiene el background para que se vea bien

    private void Awake()
    {
        cam = GetComponent<Camera>();
        camBackground = GetComponentInChildren<Canvas>();
    }

    private void Update()
    {
        //Se configura la altura y orientaci�n de la c�mara seg�n la orientaci�n de la pantalla para que siempre se vean todos los dados
        float distance;
        Vector3 rotation;
        if(Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
        {
            distance = portraitDistance;
            rotation = portraitRotation; 
        }
        else
        {
            distance = landscapeDistance;
            rotation = landscapeRotation;
        }
        cam.transform.localPosition = new Vector3(cam.transform.localPosition.x, distance, cam.transform.localPosition.z);
        cam.transform.rotation = Quaternion.Euler(rotation);
    }

    public void ZoomBackground() //Acerca el canvas con el background. Llamado por la pesta�a de resultados al aparecer
    {
        camBackground.planeDistance = 2; //Poniendo el canvas cerca se consigue que los resultados se lean bien, dejando a los dados de fondo
    }

    public void ZoomOutBackground()
    {
        camBackground.planeDistance = 100; //El background debe de estar detr�s de los dados/monedas para que su lanzamiento se vea bien
    }
}
