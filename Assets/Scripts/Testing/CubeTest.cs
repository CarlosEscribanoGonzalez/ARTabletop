using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using System.Linq;

public class CubeTest : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private float rotationSpeed = 10;
    private ARAnchorManager anchorManager;
    private ARPlaneManager planeManager;
    private ARAnchor anchor;

    private void Start()
    {
        anchorManager = FindFirstObjectByType<ARAnchorManager>();
        planeManager = FindFirstObjectByType<ARPlaneManager>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Se intentará crear un anchor...");
        if (planeManager.trackables.count == 0 || anchor != default) return;
        CreateAnchor();
    }

    private void CreateAnchor()
    {
        ARPlane firstPlane = null;
        foreach (var plane in planeManager.trackables)
        {
            firstPlane = plane;
            break;
        }
        Pose pose = new Pose(firstPlane.transform.position, Quaternion.identity);
        anchor = anchorManager.AttachAnchor(firstPlane, pose);

        Debug.Log(anchor);
        if (anchor != null)
        {
            transform.SetParent(anchor.transform, false);
            transform.localPosition = Vector3.zero;
            transform.localScale /= 10;
        }
        else Debug.LogError("ERROR: ANCHOR ES NULO");
    }

    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
        if(anchor != null) Debug.Log(anchor.transform.position);
    }

    public void DebugPlaneDetected()
    {
        Debug.Log("PLANES DETECTED: " + planeManager.trackables.count);
    }
}
