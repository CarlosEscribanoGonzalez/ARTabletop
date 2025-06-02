using System.IO;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TestGLB : MonoBehaviour
{
    [SerializeField] private GameObject[] models;

    private void Awake()
    {
        foreach(var model in models) StartCoroutine(CopyFileToPersistentDataPath(model.name + ".glb"));
    }

    IEnumerator CopyFileToPersistentDataPath(string filename)
    {
        string sourcePath = Path.Combine(Application.streamingAssetsPath, filename);
        string destinationPath = Path.Combine(Application.persistentDataPath, filename);

        if (!File.Exists(destinationPath))
        {
            UnityWebRequest request = UnityWebRequest.Get(sourcePath);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                File.WriteAllBytes(destinationPath, request.downloadHandler.data);
                Debug.Log($"Archivo copiado a {destinationPath}");
            }
            else
            {
                Debug.LogError($"Error al copiar archivo: {request.error}");
            }
        }
        else
        {
            Debug.Log($"Archivo ya existe en {destinationPath}");
        }
    }
}
