using System.IO;
using UnityEngine;

public class ContentDownloader : MonoBehaviour
{
    public static void DownloadSprite(Sprite sprite)
    {
        if (sprite == null || sprite.texture.name.StartsWith("DefaultImage")) return;
        string textureName = sprite.texture.name;
        string path = Path.Combine(Application.persistentDataPath, textureName);
        byte[] bytes;
        if (textureName.EndsWith(".png")) bytes = sprite.texture.EncodeToPNG();
        else bytes = sprite.texture.EncodeToJPG();
        File.WriteAllBytes(path, bytes);
        Debug.Log($"Imagen {textureName} almacenada/actualizada localmente");
    }

    public static void DownloadModel(string originPath)
    {
        if (originPath.Contains("DefaultModel")) return;
        string fileName = Path.GetFileName(originPath);
        string destinationFile = Path.Combine(Application.persistentDataPath, fileName);
        File.Copy(originPath, destinationFile, true);
        Debug.Log("Modelo " + fileName + " almacenado/actualizado localmente");
    }
}
