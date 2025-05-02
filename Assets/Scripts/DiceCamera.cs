using UnityEngine;

public class DiceCamera : MonoBehaviour
{
    [SerializeField] private float portraitDistance = 13f;
    [SerializeField] private float landscapeDistance = 7.75f;
    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void OnEnable()
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
}
