using System.IO;
using UnityEngine;

public class ContentDownloader : MonoBehaviour
{
    public static void DownloadSprite(Sprite sprite)
    {
        if (sprite == null || sprite.texture.name.Contains("DefaultImage")) return;
        string textureName = sprite.texture.name;
        string path = Path.Combine(Application.persistentDataPath, textureName);
        if (File.Exists(path))
        {
            Debug.Log("Archivo " + textureName + " no añadido al ya estar en el dispositivo.");
            return;
        }
        byte[] bytes;
        if (textureName.EndsWith(".png")) bytes = sprite.texture.EncodeToPNG();
        else bytes = sprite.texture.EncodeToJPG();
        File.WriteAllBytes(path, bytes);
        Debug.Log($"Imagen {textureName} almacenada localmente");
    }

    public static void DownloadModel(string originPath, string modelName)
    {
        string destinationFile = Path.Combine(Application.persistentDataPath, modelName);
        if (File.Exists(destinationFile))
        {
            Debug.Log("Archivo " + originPath + " no añadido al ya estar en el dispositivo.");
            return;
        }
        File.Copy(originPath, destinationFile, true);
        Debug.Log("Modelo " + modelName + " almacenado localmente");
    }
}
