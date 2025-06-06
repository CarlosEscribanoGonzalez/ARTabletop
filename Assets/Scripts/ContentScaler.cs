using UnityEngine;

public static class ContentScaler
{
    private static MeshFilter[] meshFilters;
    public static float ScaleModel(GameObject model, float targetSize)
    {
        meshFilters = model.GetComponentsInChildren<MeshFilter>();
        if (meshFilters.Length == 0) return 1;

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
        return scaleFactor;
    }

    private static Bounds TransformBounds(Matrix4x4 matrix, Bounds bounds)
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
