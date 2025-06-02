using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.IO.Compression;
using System.Linq;

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
        List<string> files = new(); //Lista de paths
        files.Add(gameToShare.ConvertToJson()); //Añade el json
        files.AddRange(GetImagePaths(gameToShare)); //Añade las imágenes
        files.AddRange(GetModelPaths(gameToShare)); //Añade los modelos

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
        AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "Compartir Juego");
        unityActivity.Call("startActivity", chooser);

        LoadingScreenManager.ToggleLoadingScreen(false);
    }

    private static string CreateZip(string zipName, List<string> files) //Crea un zip con el nombre del juego
    {
        string zipPath = Application.persistentDataPath + $"/{zipName}.zip";

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

    private static List<string> GetImagePaths(GameInfo game) //Obtiene el path de todas las imágenes
    {
        List<string> listToReturn = new();
        AddPathIfNotContained(listToReturn, GetPathFromSprite(game.gameImage));
        foreach (var cardInfo in game.cardsInfo)
        {
            AddPathIfNotContained(listToReturn, GetPathFromSprite(cardInfo.sprite));
        }
        AddPathIfNotContained(listToReturn, GetPathFromSprite(game.defaultSprite));
        foreach (var specialCard in game.specialCardsInfo)
        {
            AddPathIfNotContained(listToReturn, GetPathFromSprite(specialCard.defaultSpecialSprite));
            foreach (var cardInfo in specialCard.cardsInfo) AddPathIfNotContained(listToReturn, GetPathFromSprite(cardInfo.sprite));
        }
        foreach (var board2d in game.boards2D)
        {
            AddPathIfNotContained(listToReturn, GetPathFromSprite(board2d));
        }
        return listToReturn;
    }

    private static List<string> GetModelPaths(GameInfo game)
    {
        List<string> listToReturn = new();
        AddPathIfNotContained(listToReturn, GetPathFromModel(game.defaultPiece));
        foreach (var piece in game.pieces) AddPathIfNotContained(listToReturn, GetPathFromModel(piece));
        return listToReturn;
    }

    private static void AddPathIfNotContained(List<string> list, string path) //Añade un path a una lista siempre que no esté ya añadido
    {
        if (path != string.Empty && !list.Contains(path)) list.Add(path);
        else if(path != string.Empty) Debug.Log($"Contenido en {path} ya añadido para compartir, no incluido");
    }

    private static string GetPathFromSprite(Sprite sprite) //Obtiene el path de un sprite a partir del nombre de su textura
    {
        if (sprite is null) return string.Empty;
        string textureName = sprite.texture.name;
        string path;
        byte[] bytes = new byte[0];
        try //Primero intenta buscar con extensión .png
        {
            path = Application.persistentDataPath + $"/{textureName}.png";
            if (!File.Exists(path)) bytes = sprite.texture.EncodeToPNG(); //Si el path no existe carga su contenido
        }
        catch //Si no, con jpg
        {
            path = Application.persistentDataPath + $"/{textureName}.jpg";
            if (!File.Exists(path)) bytes = sprite.texture.EncodeToJPG(); //Si el path no existe carga su contenido
        }
        if (!File.Exists(path)) File.WriteAllBytes(path, bytes); //Si el path no existe lo crea con el contenido cargado
        return path;
    }

    private static string GetPathFromModel(GameObject model)
    {
        string path = Path.Combine(Application.persistentDataPath, model.name + ".glb");
        return path;
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
