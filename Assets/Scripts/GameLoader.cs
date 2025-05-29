using Serialization;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;

public class GameLoader : MonoBehaviour
{
    private static GameLoader instance;

    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep; //La pantalla se configura para no apagarse sola
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
        DontDestroyOnLoad(this);
        LoadSavedGames();
        TryReadNewGame();
    }

    public void OnIntentReceived(string uri)
    {
        LoadingScreenManager.ToggleLoadingScreen(true);
        Invoke("TryReadNewGame", 0.2f);
    }

    public void DeleteGameInfo(string jsonPath)
    {
        string gameId = GetCustomGameID(JsonUtility.FromJson<GameInfoSerializable>(File.ReadAllText(jsonPath)));
        string path = Path.Combine(Application.persistentDataPath, gameId + ".artabletop");
        if (File.Exists(path))
        {
            Debug.Log($"Juego eliminado de {path}");
            File.Delete(path);
        }
        else Debug.LogError($"No se encontró el juego a borrar en {path}");
    }

    public void DeleteImage(string name)
    {
        string path = Path.Combine(Application.persistentDataPath, name + ".png");
        if (!File.Exists(path)) path = Path.Combine(Application.persistentDataPath, name + ".jpg");
        if (!File.Exists(path)) Debug.LogError($"Imagen a borrar en {path} no encontrada.");
        else
        {
            Debug.Log("Imagen en " + path + " eliminada");
            File.Delete(path);
        }
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
        finally
        {
            LoadingScreenManager.ToggleLoadingScreen(false);
        }
    }

    private void LoadSavedGames()
    {
        string rootPath = Application.persistentDataPath;
        DirectoryInfo dirInfo = new DirectoryInfo(rootPath);
        FileInfo[] gameFiles = dirInfo.GetFiles("*.artabletop")
                                 .OrderBy(f => f.CreationTime)
                                 .ToArray();
        foreach (FileInfo file in gameFiles)
        {
            Debug.Log("CARGANDO UN JUEGO..." + file);
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

    public static string GetCustomGameID(GameInfoSerializable jsonContent)
    {
        string name = jsonContent.gameName;
        int dif = jsonContent.specialCardsInfo.Count * jsonContent.cardsInfo.Count + jsonContent.boardImagesNames.Count; //Número diferenciador en caso de que tengan dos juegos el mismo nombre
        //Mirar si es una tontería el hash:
        return name + "_" + name[0].GetHashCode() + name[name.Length-1].GetHashCode() + "_" + dif;
    }

    private void AddGame(string content)
    {
        GameOptionsManager gameOptionsManager = FindFirstObjectByType<GameOptionsManager>();
        if (gameOptionsManager != null) gameOptionsManager.AddNewGame(content);
    }
}
