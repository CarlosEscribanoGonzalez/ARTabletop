using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;

public class GameSaver : MonoBehaviour
{
    private static GameSaver Instance; //A diferencia del resto, este perdura entre escenas (se puede cargar un juego durante la escena de juego)

    private void Start()
    {
        if (Instance == null) Instance = this; //Singleton
        else Destroy(this.gameObject);
        DontDestroyOnLoad(this);
        TryReadNewGame(); //Comprueba si la app se abrió con un intent (es decir, leyendo el .zip)
    }

    public void OnIntentReceived(string uri) //Recibe un intent una vez la app ya está abierta
    {
        LoadingScreenManager.ToggleLoadingScreen(true);
        Invoke("TryReadNewGame", 0.2f); //Pequeño retraso para que dé tiempo a que salga la pantalla de carga antes de que Android bloquee la app
    }

    private void TryReadNewGame()
    {
        try //Intenta leer el intent:
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject intent = activity.Call<AndroidJavaObject>("getIntent");
            string action = intent.Call<string>("getAction");
            byte[] zipBytes = null;
            if (action == "android.intent.action.VIEW") //Si el intent es de tipo view se obtienen los datos
            {
                AndroidJavaObject uri = intent.Call<AndroidJavaObject>("getData");
                string scheme = uri.Call<string>("getScheme");
                string jsonContent = "";
                //Dependiendo del esquema del uri es obtienen los bytes del zip de distintas formas
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
                
                if (zipBytes != null && zipBytes.Length > 0) //Si se han logrado obtener los bytes del zip:
                {
                    using (MemoryStream zipStream = new MemoryStream(zipBytes))
                    using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read)) //Se lee el zip
                    {
                        foreach (var entry in archive.Entries) SaveImage(entry); //Se guardan las imágenes en memoria local
                        //Se busca el json:
                        ZipArchiveEntry targetEntry = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".artabletop", StringComparison.OrdinalIgnoreCase));
                        if (targetEntry != null)
                        {
                            using (StreamReader reader = new StreamReader(targetEntry.Open())) //Si hay un json se abre y se lee
                            {
                                jsonContent = reader.ReadToEnd();
                                if (!string.IsNullOrEmpty(jsonContent)) //Si el contenido es legible se guarda en memoria
                                {
                                    SaveGameInfo(jsonContent);
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

    private void SaveGameInfo(string content) //Guarda el json en memoria
    {
        GameInfo newGameInfo = GameInfo.FromJsonToSO(content); //Crea el SO a partir del json
        string gameId = newGameInfo.GetCustomID(); //Obtiene su CustomID
        string path = Path.Combine(Application.persistentDataPath, gameId); //Obtiene su path
        if (File.Exists(path)) Debug.Log("Juego no guardado porque ya se encontraba en el dispositivo");
        try //Si el json no existe se almacena
        {
            File.WriteAllText(path, content); //Se guarda su info
            Debug.Log("Juego guardado en: " + path);
            GameLoader.LoadGame(newGameInfo); //Si se está en el menú principal se carga en la lista de juegos
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error guardando el juego: " + e);
        }
    }

    private void SaveImage(ZipArchiveEntry entry) //Guarda las imágenes en local
    {
        string entryName = entry.FullName; //Primero se comprueba que sea un formato compatible
        if (entryName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
            entryName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                string safeName = Path.GetFileName(entryName);
                string imagePath = Path.Combine(Application.persistentDataPath, safeName); //Se obtiene el path donde se guardarán
                if (File.Exists(imagePath)) //Las imágenes no se guardan si ya están guardadas de antes
                {
                    Debug.Log($"Imagen no guardada en: {imagePath} puesto que ya existe");
                    return;
                }
                //Si no, se almacena su información en el path:
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
