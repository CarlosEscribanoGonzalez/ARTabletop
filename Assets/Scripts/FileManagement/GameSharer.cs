using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Serialization;

public class GameSharer : MonoBehaviour
{
    private static string zipPathToRemove; //Cuando se termina de enviar el zip ha de eliminarse, así que se guarda una referencia a él

    private void Awake()
    {
        //En el caso de que el usuario cerrara la app nada más realizar el intent sin volver a ella el zip se queda guardado, así que hay que borrarlo
        DeleteAllZips();
    }

    private void OnApplicationFocus(bool hasFocus) //Si cuando se vuelve a la app hay un zip que borrar este se borra
    {
        if (hasFocus && !string.IsNullOrEmpty(zipPathToRemove) && File.Exists(zipPathToRemove))
        {
            File.Delete(zipPathToRemove);
            Debug.Log("Zip borrado en " + zipPathToRemove);
            zipPathToRemove = null;
        }
    }

    public static void Share(GameInfo gameToShare) //Hace un zip con los archivos necesarios y lo comparte mediante un intent
    {
        try
        {
            List<string> files = new(); //Lista de paths
            string jsonPath = gameToShare.ConvertToJson();
            files.Add(jsonPath); //Añade el json
            GameInfoSerializable serializedGameInfo = JsonUtility.FromJson<GameInfoSerializable>(File.ReadAllText(jsonPath));
            files.AddRange(GetImagePaths(serializedGameInfo)); //Añade las imágenes
            files.AddRange(GetModelPaths(serializedGameInfo)); //Añade los modelos

            string zipPath = CreateZip(gameToShare.gameName, files); //Crea el zip a partir de los paths
                                                                     //Se configura el intent de enviar .zip:
            AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
            AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
            intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
            intentObject.Call<AndroidJavaObject>("setType", "application/zip");
            //Se obtiene la authority del FileProvider:
            AndroidJavaObject unityActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                .GetStatic<AndroidJavaObject>("currentActivity");
            string authority = unityActivity.Call<string>("getPackageName") + ".provider";
            //Se obtiene la dependencia:
            AndroidJavaClass fileProviderClass = new AndroidJavaClass("androidx.core.content.FileProvider");
            AndroidJavaObject uriObject = fileProviderClass.CallStatic<AndroidJavaObject>(
                "getUriForFile", unityActivity, authority, new AndroidJavaObject("java.io.File", zipPath));
            //Se comparte el juego
            intentObject.Call<AndroidJavaObject>("putExtra", "android.intent.extra.STREAM", uriObject);
            intentObject.Call<AndroidJavaObject>("addFlags", 1 << 1);
            AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "Share game");
            unityActivity.Call("startActivity", chooser);
            LoadingScreenManager.ToggleLoadingScreen(false);
        }
        catch (System.Exception _)
        {
            LoadingScreenManager.ToggleLoadingScreen(false);
            FeedbackManager.Instance.DisplayMessage("Unexpected error: game couldn't be shared. Please, try again.");
        }
    }

    private static string CreateZip(string zipName, List<string> files) //Crea un zip con el nombre del juego
    {
        string zipPath = Application.persistentDataPath + $"/{zipName.Replace(" ", "")}.zip";

        if (File.Exists(zipPath)) File.Delete(zipPath);//Si ya existe el zip lo borra antes de formarlo de nuevo (por si acaso)

        using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Create)) //Se crea el zip
        {
            foreach (string file in files)
            {
                archive.CreateEntryFromFile(file, Path.GetFileName(file)); //Se crea un entry por cada path
            }
        }
        zipPathToRemove = zipPath; //El zip se configura para ser borrado una vez enviado
        return zipPath;
    }

    private static List<string> GetImagePaths(GameInfoSerializable game) //Obtiene el path de todas las imágenes
    {
        List<string> listToReturn = new();
        AddPathIfNotContained(listToReturn, GetPathFromName(game.gameImageFileName));
        foreach (var cardInfo in game.cardsInfo)
        {
            AddPathIfNotContained(listToReturn, GetPathFromName(cardInfo.spriteFileName));
        }
        AddPathIfNotContained(listToReturn, GetPathFromName(game.defaultSpriteFileName));
        foreach (var specialCard in game.specialCardsInfo)
        {
            AddPathIfNotContained(listToReturn, GetPathFromName(specialCard.defaultSpriteFileName));
            foreach (var cardInfo in specialCard.cardsInfo) AddPathIfNotContained(listToReturn, GetPathFromName(cardInfo.spriteFileName));
        }
        foreach (var board2d in game.boardImagesNames)
        {
            AddPathIfNotContained(listToReturn, GetPathFromName(board2d));
        }
        return listToReturn;
    }

    private static List<string> GetModelPaths(GameInfoSerializable game)
    {
        List<string> listToReturn = new();
        if (game.defaultPieceName != null) AddPathIfNotContained(listToReturn, GetPathFromName(game.defaultPieceName));
        foreach (var piece in game.piecesNames) AddPathIfNotContained(listToReturn, GetPathFromName(piece));
        foreach (var board in game.boardModelsNames) AddPathIfNotContained(listToReturn, GetPathFromName(board));
        return listToReturn;
    }

    private static string GetPathFromName(string name)
    {
        string path = Path.Combine(Application.persistentDataPath, name);
        if (File.Exists(path)) return path;
        return "";
    }

    private static void AddPathIfNotContained(List<string> list, string path) //Añade un path a una lista siempre que no esté ya añadido
    {
        if (path != string.Empty && !list.Contains(path)) list.Add(path);
        else if (path != string.Empty) Debug.Log($"Contenido en {path} ya añadido para compartir, no incluido");
    }

    private void DeleteAllZips()
    {
        string rootPath = Application.persistentDataPath;
        DirectoryInfo dirInfo = new DirectoryInfo(rootPath);
        FileInfo[] gameFiles = dirInfo.GetFiles("*.zip").ToArray();
        foreach (var file in gameFiles)
        {
            File.Delete(file.FullName);
            Debug.Log($"Zip {file.FullName} borrado.");
        }
    }
}
