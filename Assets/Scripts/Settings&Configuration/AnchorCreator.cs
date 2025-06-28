using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class AnchorCreator : MonoBehaviour
{
    public static AnchorCreator Instance { get; private set; }
    private ARAnchorManager anchorManager;
    private ARPlaneManager planeManager;

    private void Start()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
        anchorManager = FindFirstObjectByType<ARAnchorManager>();
        planeManager = FindFirstObjectByType<ARPlaneManager>();
    }

    public ARAnchor CreateAnchor(GameObject gameUnit)
    {
        ARPlane nearestPlane = null;
        float minDistance = float.MaxValue;
        foreach (var plane in planeManager.trackables)
        {
            Debug.Log(plane);
            float dist = Vector3.Distance(plane.transform.position, gameUnit.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestPlane = plane;
            }
        }

        if (nearestPlane == null)
        {
            Debug.LogError("Error al crear un anchor: no hay planos");
            return null;
        }

        Vector3 planeCenter = nearestPlane.transform.position;
        Vector3 objectPos = gameUnit.transform.position;
        Vector3 cameraPos = Camera.main.transform.position;
        Vector3 camToObjDir = (objectPos - cameraPos).normalized;
        Vector3 camToPlane = planeCenter - cameraPos;
        float projLength = Vector3.Dot(camToPlane, camToObjDir);
        float minDistanceToPlane = 0.1f;
        projLength = Mathf.Max(projLength, minDistanceToPlane);
        Vector3 newPos = cameraPos + camToObjDir * projLength;
        Pose newPose = new Pose(newPos, Quaternion.identity);

        ARAnchor anchor = anchorManager.AttachAnchor(nearestPlane, newPose);
        Debug.Log($"Posición del anchor: {anchor.transform.position}");
        return anchor;
    }
}
