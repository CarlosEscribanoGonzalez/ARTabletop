using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;

public class GameSaver : MonoBehaviour
{
    private static GameSaver Instance;

    private void Start()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
        DontDestroyOnLoad(this);
        TryReadNewGame();
    }

    public void OnIntentReceived(string uri)
    {
        LoadingScreenManager.ToggleLoadingScreen(true);
        Invoke("TryReadNewGame", 0.2f);
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
                    string zipPath = uri.Call<string>("getPath");
                    zipBytes = File.ReadAllBytes(zipPath);
                }
                else if (scheme == "content")
                {
                    AndroidJavaObject contentResolver = activity.Call<AndroidJavaObject>("getContentResolver");
                    AndroidJavaObject inputStream = contentResolver.Call<AndroidJavaObject>("openInputStream", uri);

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        const int bufferSize = 4096;

                        AndroidJavaObject dataInputStream = new AndroidJavaObject("java.io.DataInputStream", inputStream);
                        while (true)
                        {
                            int available = dataInputStream.Call<int>("available");
                            if (available <= 0) break;

                            int readSize = Math.Min(bufferSize, available);
                            sbyte[] tempSBytes = dataInputStream.Call<sbyte[]>("readNBytes", readSize);

                            if (tempSBytes == null || tempSBytes.Length == 0) break;

                            byte[] tempBytes = new byte[tempSBytes.Length];
                            Buffer.BlockCopy(tempSBytes, 0, tempBytes, 0, tempSBytes.Length);

                            memoryStream.Write(tempBytes, 0, tempBytes.Length);
                        }

                        zipBytes = memoryStream.ToArray();
                    }
                }
                
                if (zipBytes != null && zipBytes.Length > 0)
                {
                    using (MemoryStream zipStream = new MemoryStream(zipBytes))
                    using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
                    {
                        foreach (var entry in archive.Entries) SaveImage(entry);
                        ZipArchiveEntry targetEntry = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".artabletop", StringComparison.OrdinalIgnoreCase));
                        if (targetEntry != null)
                        {
                            using (StreamReader reader = new StreamReader(targetEntry.Open()))
                            {
                                jsonContent = reader.ReadToEnd();
                                if (!string.IsNullOrEmpty(jsonContent))
                                {
                                    if (!SaveGameInfo(jsonContent)) return;
                                }
                                else
                                {
                                    Debug.LogError("El archivo está vacío o no se pudo leer.");
                                }
                            }
                        }
                        else Debug.LogError("No se encontró ningún archivo .artabletop dentro del .zip.");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Intento de añadir juego fallido: " + e.Message);
        }
        finally
        {
            LoadingScreenManager.ToggleLoadingScreen(false);
        }
    }

    private bool SaveGameInfo(string content)
    {
        GameInfo newGameInfo = GameInfo.FromJsonToSO(content);
        string gameId = newGameInfo.GetCustomID();
        string path = Path.Combine(Application.persistentDataPath, gameId);
        if (File.Exists(path))
        {
            Debug.Log("Juego no guardado porque ya se encontraba en el dispositivo");
            return false;
        }
        try
        {
            File.WriteAllText(path, content);
            Debug.Log("Juego guardado en: " + path);
            GameLoader.LoadGame(newGameInfo);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error guardando el juego: " + e);
            return false;
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
                if (File.Exists(imagePath))
                {
                    Debug.Log($"Imagen no guardada en: {imagePath} puesto que ya existe");
                    return;
                }
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
}
