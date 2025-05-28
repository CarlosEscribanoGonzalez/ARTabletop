using Newtonsoft.Json;
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
        LoadSavedGames();
        TryReadNewGame();
    }

    public void OnIntentReceived(string uri)
    {
        TryReadNewGame();
    }

    public void DeleteGameInfo(string jsonPath)
    {
        string gameId = GetCustomGameID(JsonUtility.FromJson<GameInfoSerializable>(File.ReadAllText(jsonPath)));
        string path = Path.Combine(Application.persistentDataPath, gameId + ".artabletop");
        if (File.Exists(path)) File.Delete(path);
        else Debug.LogError("No se encontró el juego a borrar");
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
                        foreach (var entry in archive.Entries) SaveImage(entry);
                    }
                }

                if (!string.IsNullOrEmpty(jsonContent))
                {
                    SaveGameInfo(jsonContent);
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

    private void LoadSavedGames()
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

    private void SaveGameInfo(string content)
    {
        string gameId = GetCustomGameID(JsonUtility.FromJson<GameInfoSerializable>(content));
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

    private void SaveImage(ZipArchiveEntry entry)
    {
        string entryName = entry.FullName;
        if (entryName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
            entryName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
            entryName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                string safeName = Path.GetFileName(entryName);
                string imagePath = Path.Combine(Application.persistentDataPath, safeName);
                if (File.Exists(imagePath)) return;
                using (Stream entryStream = entry.Open())
                using (FileStream fileStream = new FileStream(imagePath, FileMode.Create, FileAccess.Write))
                {
                    entryStream.CopyTo(fileStream);
                }

                Debug.Log($"Imagen guardada en: {imagePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error guardando imagen {entryName}: {ex.Message}");
            }
        }
    }

    private string GetCustomGameID(GameInfoSerializable jsonContent)
    {
        string name = jsonContent.gameName;
        int dif = jsonContent.specialCardsInfo.Count * jsonContent.cardsInfo.Count; //Número diferenciador en caso de que tengan dos juegos el mismo nombre
        return name + "_" + name[0].GetHashCode() + name[name.Length-1].GetHashCode() + "_" + dif;
    }

    private void AddGame(string content)
    {
        GameOptionsManager gameOptionsManager = FindFirstObjectByType<GameOptionsManager>();
        if (gameOptionsManager != null) gameOptionsManager.AddNewGame(content);
    }
}
