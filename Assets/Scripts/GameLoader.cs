using Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;

public class GameLoader : MonoBehaviour
{
    private static GameLoader instance;

    private void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
        DontDestroyOnLoad(this);
        LoadGames();
        TryReadNewGame();
    }

    public void OnIntentReceived(string uri)
    {
        Debug.Log("Datos recividos con URI: " + uri);
        SaveGame(ReadFileFromUri(uri));
    }

    private void TryReadNewGame()
    {
        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject intent = activity.Call<AndroidJavaObject>("getIntent");

            string action = intent.Call<string>("getAction");
            byte[] zipBytes = null;
            if (action == "android.intent.action.VIEW")
            {
                AndroidJavaObject uri = intent.Call<AndroidJavaObject>("getData");
                string scheme = uri.Call<string>("getScheme");
                string jsonContent = "";
                if (scheme == "file")
                {
                    string path = uri.Call<string>("getPath");
                    zipBytes = File.ReadAllBytes(path);
                }
                else if (scheme == "content")
                {
                    AndroidJavaObject contentResolver = activity.Call<AndroidJavaObject>("getContentResolver");
                    AndroidJavaObject inputStream = contentResolver.Call<AndroidJavaObject>("openInputStream", uri);

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        const int bufferSize = 4096;
                        byte[] buffer = new byte[bufferSize];

                        AndroidJavaObject dataInputStream = new AndroidJavaObject("java.io.DataInputStream", inputStream);
                        while (true)
                        {
                            int available = dataInputStream.Call<int>("available");
                            if (available <= 0) break;

                            int readSize = Math.Min(bufferSize, available);
                            byte[] temp = dataInputStream.Call<byte[]>("readNBytes", readSize);
                            if (temp == null || temp.Length == 0) break;

                            memoryStream.Write(temp, 0, temp.Length);
                        }

                        zipBytes = memoryStream.ToArray();
                    }
                }

                if (zipBytes != null && zipBytes.Length > 0)
                {
                    using (MemoryStream zipStream = new MemoryStream(zipBytes))
                    using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
                    {
                        ZipArchiveEntry targetEntry = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".artabletop", StringComparison.OrdinalIgnoreCase));
                        if (targetEntry != null)
                        {
                            using (StreamReader reader = new StreamReader(targetEntry.Open()))
                            {
                                jsonContent = reader.ReadToEnd();
                            }
                        }
                        else
                        {
                            Debug.LogError("No se encontró ningún archivo .artabletop dentro del .zip.");
                        }
                    }
                }

                if (!string.IsNullOrEmpty(jsonContent))
                {
                    SaveGame(jsonContent);
                }
                else
                {
                    Debug.LogError("El archivo está vacío o no se pudo leer.");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Intento de añadir juego fallido: " + e.Message);
        }
    }

    private string ReadFileFromUri(string uriString)
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

    private void LoadGames()
    {
        string rootPath = Application.persistentDataPath;
        DirectoryInfo dirInfo = new DirectoryInfo(rootPath);
        FileInfo[] gameFiles = dirInfo.GetFiles("game_*.artabletop")
                                 .OrderBy(f => f.CreationTime)
                                 .ToArray();
        foreach (FileInfo file in gameFiles)
        {
            try
            {
                AddGame(File.ReadAllText(file.FullName));
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error cargando un juego: " + e);
            }
        }
    }

    private void SaveGame(string content)
    {
        string gameId = "game_" + JsonUtility.FromJson<GameInfoSerializable>(content).gameName + content.Length % 99;
        Debug.Log(gameId);
        string path = Path.Combine(Application.persistentDataPath, gameId + ".artabletop");
        if (File.Exists(path)) return;
        try
        {
            File.WriteAllText(path, content);
            Debug.Log("Juego guardado en: " + path);
            AddGame(content);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error guardando el juego: " + e);
        }
    }

    private void AddGame(string content)
    {
        GameOptionsManager gameOptionsManager = FindFirstObjectByType<GameOptionsManager>();
        if (gameOptionsManager != null) gameOptionsManager.AddNewGame(content);
    }
}
