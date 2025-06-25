using UnityEngine;

public static class ContentScaler
{
    private static MeshFilter[] meshFilters;
    private static float spriteScaleMult;
    private static Vector2 imageScaleMult;

    public static float ScaleSprite(Texture2D texture, Vector2 desiredTextureSize)
    {
        if (texture.width > texture.height)
            spriteScaleMult = desiredTextureSize.x / texture.width;
        else spriteScaleMult = desiredTextureSize.y / texture.height;
        while (texture.width * spriteScaleMult > desiredTextureSize.x || texture.height * spriteScaleMult > desiredTextureSize.y)
            spriteScaleMult *= 0.95f;
        return spriteScaleMult;
    }

    public static Vector2 ScaleImage(Texture texture, Rect desiredRect)
    {
        float ratio = (float)texture.width / texture.height; //Se obtiene el aspect ratio de la imagen
        //La imagen se escala teniendo en cuenta cuál de sus dos dimensiones, width o height, es más grande, manteniendo el ratio
        if (texture.width < texture.height) //Si es más alta se ajusta a lo ancho
            imageScaleMult = new Vector2(desiredRect.size.x, desiredRect.size.x / ratio);
        else imageScaleMult = new Vector2(desiredRect.size.y * ratio, desiredRect.size.y); //Si no, a lo alto
        //Si la imagen aun así se sale del mask se hace progresivamente más pequeña hasta que no lo haga
        while (imageScaleMult.x > desiredRect.size.x || imageScaleMult.y > desiredRect.size.y)
            imageScaleMult *= 0.95f;
        return imageScaleMult;
    }

    public static float ScaleModel(GameObject model, float targetSize)
    {
        //Para la forma en la que se calcula el factor importa mucho la orientación del modelo, 
        //así que se trabaja con una instancia en su lugar en vez de con el modelo original
        GameObject instance = GameObject.Instantiate(model, model.transform.parent); 
        foreach (var rend in instance.GetComponentsInChildren<Renderer>()) rend.enabled = false;
        instance.transform.SetParent(null);
        instance.transform.position = Vector3.zero;
        instance.transform.rotation = Quaternion.identity;
        meshFilters = instance.GetComponentsInChildren<MeshFilter>();
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
        GameObject.Destroy(instance);
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
