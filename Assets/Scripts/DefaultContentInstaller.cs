using System.IO;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class DefaultContentInstaller : MonoBehaviour
{
    [SerializeField] private GameObject defaultPiece;
    [SerializeField] private string[] contentNames;
    [SerializeField] private bool forceDownload;
    public GameObject DefaultPiece => defaultPiece;
    public static DefaultContentInstaller Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        foreach (var content in contentNames) StartCoroutine(CopyFileToPersistentDataPath(content));
    }

    IEnumerator CopyFileToPersistentDataPath(string filename)
    {
        string sourcePath = Path.Combine(Application.streamingAssetsPath, filename);
        string destinationPath = Path.Combine(Application.persistentDataPath, filename);

        if (forceDownload && File.Exists(destinationPath)) File.Delete(destinationPath);

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
