using UnityEngine;
using UnityEngine.EventSystems;
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

        // Direcci�n desde c�mara hacia objeto
        Vector3 camToObjDir = (objectPos - cameraPos).normalized;

        // Vector desde c�mara hasta el centro del plano
        Vector3 camToPlane = planeCenter - cameraPos;

        // Proyectamos camToPlane sobre la direcci�n c�mara->objeto
        float projLength = Vector3.Dot(camToPlane, camToObjDir);

        // Definimos distancia m�nima al plano (offset)
        float minDistanceToPlane = 0.1f;

        // Aseguramos que la proyecci�n est� al menos a minDistanceToPlane
        projLength = Mathf.Max(projLength, minDistanceToPlane);

        // Calculamos la nueva posici�n a lo largo de la l�nea c�mara->objeto
        Vector3 newPos = cameraPos + camToObjDir * projLength;

        Pose newPose = new Pose(newPos, Quaternion.identity);

        ARAnchor anchor = anchorManager.AttachAnchor(nearestPlane, newPose);
        Debug.Log($"Posici�n del anchor: {anchor.transform.position}");
        return anchor;
    }
}
