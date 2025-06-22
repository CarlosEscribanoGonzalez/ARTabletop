using UnityEngine;
using System.IO;
using System.Collections;
using UnityEngine.Networking;

public class MarkerProvider : MonoBehaviour
{
    [SerializeField] private string documentName = "ARTabletop_OfficialMarkers";

    public void GetMarkers(string extension)
    {
        string fileName = documentName + "." + extension;
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        //if (File.Exists(filePath)) ShareDocument(filePath);
        //else StartCoroutine(GetMarkersPath(fileName));
        StartCoroutine(GetMarkersPath(fileName));
    }

    IEnumerator GetMarkersPath(string fileName)
    {
        string sourcePath = Path.Combine(Application.streamingAssetsPath, fileName);
        string destinationPath = Path.Combine(Application.persistentDataPath, fileName);
        UnityWebRequest request = UnityWebRequest.Get(sourcePath);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            File.WriteAllBytes(destinationPath, request.downloadHandler.data);
            Debug.Log($"Archivo copiado a {destinationPath}");
            ShareDocument(destinationPath);
        }
        else
        {
            Debug.LogError($"Error al copiar archivo a persistentDataPath: {request.error}");
        }
    }

    private void ShareDocument(string filePath) //Mismo código que en GameSharer
    {
        AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
        AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
        intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
        intentObject.Call<AndroidJavaObject>("setType", "application/zip");
        AndroidJavaObject unityActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
            .GetStatic<AndroidJavaObject>("currentActivity");
        string authority = unityActivity.Call<string>("getPackageName") + ".provider";
        AndroidJavaClass fileProviderClass = new AndroidJavaClass("androidx.core.content.FileProvider");
        AndroidJavaObject uriObject = fileProviderClass.CallStatic<AndroidJavaObject>(
            "getUriForFile", unityActivity, authority, new AndroidJavaObject("java.io.File", filePath));
        intentObject.Call<AndroidJavaObject>("putExtra", "android.intent.extra.STREAM", uriObject);
        intentObject.Call<AndroidJavaObject>("addFlags", 1 << 1);
        AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "Share markers");
        unityActivity.Call("startActivity", chooser);
    }
}
