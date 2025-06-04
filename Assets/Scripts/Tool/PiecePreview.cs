using UnityEngine;

public class PiecePreview : MonoBehaviour
{
    [SerializeField] private Transform previewTransform;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float verticalSpeed;
    [SerializeField] private float verticalOffset;
    [SerializeField] private float targetSize = 6.75f;
    private GameObject currentPiece;
    private float targetY;

    void Start()
    {
        targetY = previewTransform.localPosition.y + verticalOffset;
    }

    void FixedUpdate()
    {
        previewTransform.Rotate(0, rotationSpeed * Time.fixedDeltaTime, 0);
        float newY = Mathf.MoveTowards(previewTransform.localPosition.y, targetY, verticalSpeed * Time.fixedDeltaTime);
        previewTransform.localPosition = new Vector3(previewTransform.localPosition.x, newY, previewTransform.localPosition.z);
        if (Mathf.Abs(previewTransform.localPosition.y - targetY) < 0.05f) targetY *= -1;
    }

    public void SetPiece(GameObject piecePrefab)
    {
        Destroy(currentPiece);
        previewTransform.localScale = Vector3.one;
        currentPiece = GameObject.Instantiate(piecePrefab, previewTransform);
        ScalePiece(piecePrefab);
    }

    private MeshFilter[] meshFilters;
    public void ScalePiece(GameObject prefabReference) //El objeto está en movimiento y genera bugs, así que se utiliza el prefab en sí
    {
        meshFilters = prefabReference.GetComponentsInChildren<MeshFilter>();
        if (meshFilters.Length == 0) return;

        Bounds combinedBounds = meshFilters[0].sharedMesh.bounds;
        Matrix4x4 matrix = meshFilters[0].transform.localToWorldMatrix;
        combinedBounds = TransformBounds(matrix, combinedBounds);

        for (int i = 1; i < meshFilters.Length; i++)
        {
            MeshFilter mf = meshFilters[i];
            Bounds worldBounds = TransformBounds(mf.transform.localToWorldMatrix, mf.sharedMesh.bounds);
            combinedBounds.Encapsulate(worldBounds);
        }

        float sizeMagnitude = combinedBounds.size.magnitude;
        float scaleFactor = targetSize / sizeMagnitude;
        previewTransform.localScale = Vector3.one * scaleFactor;
    }

    private Bounds TransformBounds(Matrix4x4 matrix, Bounds bounds)
    {
        Vector3 center = matrix.MultiplyPoint3x4(bounds.center);
        Vector3 extents = bounds.extents;
        Vector3 axisX = matrix.MultiplyVector(new Vector3(extents.x, 0, 0));
        Vector3 axisY = matrix.MultiplyVector(new Vector3(0, extents.y, 0));
        Vector3 axisZ = matrix.MultiplyVector(new Vector3(0, 0, extents.z));
        extents = new Vector3(
            Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x),
            Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y),
            Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z)
        );
        return new Bounds(center, extents * 2);
    }
}
