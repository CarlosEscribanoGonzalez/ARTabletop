using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;

public class CubeTest : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private float rotationSpeed = 10;
    private ARAnchorManager anchorManager;
    private ARPlaneManager planeManager;
    private ARAnchor anchor;
    private Vector3 initScale;

    private void Start()
    {
        anchorManager = FindFirstObjectByType<ARAnchorManager>();
        planeManager = FindFirstObjectByType<ARPlaneManager>();
        Debug.Log("INICIALIZADO");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Se intentar� crear un anchor...");
        Debug.Log($"Parent: {transform.parent}; scale: {transform.localScale}");
        if (planeManager.trackables.count == 0 || anchor != default) return;
        CreateAnchor();
    }

    private void CreateAnchor()
    {
        ARPlane nearestPlane = null;
        float minDistance = float.MaxValue;

        foreach (var plane in planeManager.trackables)
        {
            float dist = Vector3.Distance(plane.transform.position, transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestPlane = plane;
            }
        }

        if (nearestPlane == null)
        {
            Debug.LogError("ERROR: NO SE HA ENCONTRADO UN PLANO V�LIDO");
            return;
        }

        Vector3 planeCenter = nearestPlane.transform.position;
        Vector3 objectPos = transform.position;
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

        anchor = anchorManager.AttachAnchor(nearestPlane, newPose);

        if (anchor != null)
        {
            float initDist = Vector3.Distance(objectPos, Camera.main.transform.position);
            Debug.Log("ANCHOR CORRECTAMENTE ATTACHED A PLANE");
            Debug.Log(transform.localScale);
            transform.SetParent(anchor.transform);
            transform.localPosition = Vector3.zero;
            Debug.Log(transform.localScale);
            float finalDist = Vector3.Distance(transform.position, Camera.main.transform.position);
            float scaleFactor = finalDist / initDist;
            if(scaleFactor > 0) transform.localScale *= scaleFactor;
            Debug.Log(transform.localScale);
        }
        else
        {
            Debug.LogError("ERROR: ANCHOR ES NULL");
        }
        Debug.Log($"Posici�n final del objeto con anchor: {transform.position}");
    }

    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }

    private bool detected = false;
    public void DebugPlaneDetected()
    {
        if (detected) return;
        detected = true;
        Debug.Log("PLANES DETECTED: " + planeManager.trackables.count);
    }
}
