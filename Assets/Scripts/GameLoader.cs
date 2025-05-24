using Newtonsoft.Json;
using UnityEngine;

public class GameLoader : MonoBehaviour
{
    private static GameLoader instance;

    private void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
        TryReadNewGame();
    }

    public void OnIntentReceived(string uri)
    {
        Debug.Log("Datos recividos con URI: " + uri);
        AddGame(ReadFileFromUri(uri));
    }

    private void TryReadNewGame()
    {
        Debug.Log("Intentando leer nuevo juego...");
        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject intent = activity.Call<AndroidJavaObject>("getIntent");

            string action = intent.Call<string>("getAction");

            if (action == "android.intent.action.VIEW")
            {
                AndroidJavaObject uri = intent.Call<AndroidJavaObject>("getData");
                string scheme = uri.Call<string>("getScheme");
                string jsonContent = "";

                if (scheme == "file")
                {
                    string path = uri.Call<string>("getPath");
                    jsonContent = System.IO.File.ReadAllText(path);
                }
                else if (scheme == "content")
                {
                    AndroidJavaObject contentResolver = activity.Call<AndroidJavaObject>("getContentResolver");
                    AndroidJavaObject inputStream = contentResolver.Call<AndroidJavaObject>("openInputStream", uri);

                    AndroidJavaObject inputStreamReader = new AndroidJavaObject("java.io.InputStreamReader", inputStream);
                    AndroidJavaObject bufferedReader = new AndroidJavaObject("java.io.BufferedReader", inputStreamReader);

                    string line;
                    while ((line = bufferedReader.Call<string>("readLine")) != null)
                    {
                        jsonContent += line;
                    }

                    bufferedReader.Call("close");
                }

                if (!string.IsNullOrEmpty(jsonContent))
                {
                    AddGame(jsonContent);
                }
                else
                {
                    Debug.LogError("El archivo está vacío o no se pudo leer.");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("Intento de añadir juego fallido: " + e.Message);
        }
    }

    string ReadFileFromUri(string uriString)
    {
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject contentResolver = activity.Call<AndroidJavaObject>("getContentResolver");
            AndroidJavaObject uri = new AndroidJavaClass("android.net.Uri").CallStatic<AndroidJavaObject>("parse", uriString);

            AndroidJavaObject inputStream = contentResolver.Call<AndroidJavaObject>("openInputStream", uri);
            AndroidJavaObject inputStreamReader = new AndroidJavaObject("java.io.InputStreamReader", inputStream);
            AndroidJavaObject bufferedReader = new AndroidJavaObject("java.io.BufferedReader", inputStreamReader);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string line;
            while ((line = bufferedReader.Call<string>("readLine")) != null)
            {
                sb.Append(line);
            }

            bufferedReader.Call("close");
            inputStreamReader.Call("close");
            inputStream.Call("close");

            return sb.ToString();
        }
    }

    private void AddGame(string content)
    {
        GameOptionsManager gameOptionsManager = FindFirstObjectByType<GameOptionsManager>();
        if (gameOptionsManager != null) gameOptionsManager.AddNewGame(content);
    }
}
