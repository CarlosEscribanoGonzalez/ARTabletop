using UnityEngine;

public class DiceCamera : MonoBehaviour
{
    [SerializeField] private float portraitDistance = 13f;
    [SerializeField] private float landscapeDistance = 7.75f;
    private Camera cam;
    private Canvas camBackground;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        camBackground = GetComponentInChildren<Canvas>();
    }

    private void OnEnable()
    {
        camBackground.planeDistance = 100;
    }

    private void Update()
    {
        float distance;
        if(Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
        {
            distance = portraitDistance;
            cam.transform.rotation = Quaternion.Euler(90, 90, 0); 
        }
        else
        {
            distance = landscapeDistance;
            cam.transform.rotation = Quaternion.Euler(90, 0, 0);
        }
        cam.transform.localPosition = new Vector3(cam.transform.localPosition.x, distance, cam.transform.localPosition.z);
    }

    public void ZoomBackground()
    {
        camBackground.planeDistance = 5;
    }
}
