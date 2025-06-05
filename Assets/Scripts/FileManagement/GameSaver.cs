using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class GameSaver : MonoBehaviour
{
    private static GameSaver Instance; //A diferencia del resto, este perdura entre escenas (se puede cargar un juego durante la escena de juego)
    private static List<string> addedTextures = new(); //En caso de que añadir un json falle se han de borrar las texturas que se han descargado anteriormente
    private static List<string> addedModels = new(); //Lo mismo pero con los modelos 3D

    private void Awake()
    {
        if (Instance == null) Instance = this; //Singleton
        else Destroy(this.gameObject);
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        TryReadNewGame(); //Comprueba si la app se abrió con un intent (es decir, leyendo el .zip)
    }

    public void OnIntentReceived(string uri) //Recibe un intent una vez la app ya está abierta
    {
        LoadingScreenManager.ToggleLoadingScreen(true);
        Invoke("TryReadNewGame", 0.5f); //Pequeño retraso para que dé tiempo a que salga la pantalla de carga antes de que Android bloquee la app
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
                        //Se busca el json:
                        ZipArchiveEntry targetEntry = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".artabletop", StringComparison.OrdinalIgnoreCase));
                        if (targetEntry != null)
                        {
                            using (StreamReader reader = new StreamReader(targetEntry.Open())) //Si hay un json se abre y se lee
                            {
                                jsonContent = reader.ReadToEnd();
                                if (!string.IsNullOrEmpty(jsonContent)) //Si el contenido es legible se guarda en memoria
                                {
                                    SaveGameInfo(jsonContent, archive);
                                }
                                else
                                {
                                    throw new Exception("El archivo está vacío o no se pudo leer.");
                                }
                            }
                        }
                        else throw new Exception("No se encontró ningún archivo .artabletop dentro del .zip.");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Intento de añadir juego fallido: " + e.Message);
            foreach (string texture in addedTextures) GameDeleter.TryDeleteSingleImage(texture);
            foreach (string model in addedModels) GameDeleter.TryDeleteSingleModel(model);
        }
        finally
        {
            LoadingScreenManager.ToggleLoadingScreen(false);
        }
    }

    private void SaveGameInfo(string content, ZipArchive archive) //Guarda el json y archivos en memoria
    {
        string gameId = GameInfo.GetCustomJsonID(content); //Obtiene su CustomID
        string path = Path.Combine(Application.persistentDataPath, gameId); //Obtiene su path
        if (File.Exists(path))
        {
            Debug.Log("Juego no guardado porque ya se encontraba en el dispositivo");
            return;
        }
        try //Si el json no existe se almacena
        {
            addedTextures.Clear(); //Se limpia la lista de imágenes añadidas anteriormente
            foreach (var entry in archive.Entries.Where((p) => p.FullName.EndsWith(".png") || p.FullName.EndsWith(".jpg")))
            SaveImage(entry); //Se guardan las imágenes en memoria local
            addedModels.Clear(); //Lo mismo con los modelos
            foreach (var entry in archive.Entries.Where((p) => p.FullName.EndsWith(".glb"))) SaveModel(entry);
            File.WriteAllText(path, content); //Se guarda su info
            Debug.Log("Juego guardado en: " + path);
            GameLoader.LoadGame(GameInfo.FromJsonToSO(content)); //Si se está en el menú principal se carga en la lista de juegos
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error guardando el juego: " + e);
            foreach (string texture in addedTextures) GameDeleter.TryDeleteSingleImage(texture);
            foreach (string model in addedModels) GameDeleter.TryDeleteSingleModel(model);
        }
    }

    private void SaveImage(ZipArchiveEntry entry) //Guarda las imágenes en local
    {
        try
        {
            string safeName = Path.GetFileName(entry.FullName);
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

            addedTextures.Add(Path.GetFileNameWithoutExtension(imagePath));
            Debug.Log($"Imagen guardada en: {imagePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error guardando imagen {entry.FullName}: {ex.Message}");
        }
    }

    private void SaveModel(ZipArchiveEntry entry)
    {
        try
        {
            string safeName = Path.GetFileName(entry.FullName);
            string modelPath = Path.Combine(Application.persistentDataPath, safeName); //Se obtiene el path donde se guardarán
            if (File.Exists(modelPath)) //Los modelos no se guardan si ya están guardados de antes
            {
                Debug.Log($"Modelo no guardado en: {modelPath} puesto que ya existe");
                return;
            }
            //Si no, se almacena su información en el path:
            using (Stream entryStream = entry.Open())
            using (FileStream fileStream = new FileStream(modelPath, FileMode.Create, FileAccess.Write))
            {
                entryStream.CopyTo(fileStream);
            }

            addedModels.Add(Path.GetFileNameWithoutExtension(modelPath));
            Debug.Log($"Modelo guardado en: {modelPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error guardando el modelo {entry.FullName}: {ex.Message}");
        }
    }
}
