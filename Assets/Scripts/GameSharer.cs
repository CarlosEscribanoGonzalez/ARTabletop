using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.IO.Compression;

public class GameSharer : MonoBehaviour
{
    private static string zipPathToRemove;

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && !string.IsNullOrEmpty(zipPathToRemove) && File.Exists(zipPathToRemove))
        {
            File.Delete(zipPathToRemove);
            Debug.Log("Zip borrado en " + zipPathToRemove);
        }
    }

    public static void Share(GameInfo gameToShare)
    {
        List<string> files = new();
        files.Add(gameToShare.ConvertToJson());
        files.AddRange(GetImagePaths(gameToShare));

        string zipPath = CreateZip(gameToShare.gameName, files);

        AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
        AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
        intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
        intentObject.Call<AndroidJavaObject>("setType", "application/zip");

        AndroidJavaObject unityActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
            .GetStatic<AndroidJavaObject>("currentActivity");
        string authority = unityActivity.Call<string>("getPackageName") + ".provider";

        AndroidJavaClass fileProviderClass = new AndroidJavaClass("androidx.core.content.FileProvider");
        AndroidJavaObject uriObject = fileProviderClass.CallStatic<AndroidJavaObject>(
            "getUriForFile", unityActivity, authority, new AndroidJavaObject("java.io.File", zipPath));

        intentObject.Call<AndroidJavaObject>("putExtra", "android.intent.extra.STREAM", uriObject);
        intentObject.Call<AndroidJavaObject>("addFlags", 1 << 1);

        AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "Compartir Juego");
        unityActivity.Call("startActivity", chooser);

        LoadingScreenManager.ToggleLoadingScreen(false);
    }

    private static string CreateZip(string zipName, List<string> files)
    {
        string zipPath = Application.persistentDataPath + $"/{zipName}.zip";

        if (File.Exists(zipPath))
            File.Delete(zipPath);

        using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            foreach (string file in files)
            {
                archive.CreateEntryFromFile(file, Path.GetFileName(file));
            }
        }
        zipPathToRemove = zipPath;
        return zipPath;
    }

    private static List<string> GetImagePaths(GameInfo game)
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

    private static void AddPathIfNotContained(List<string> list, string path)
    {
        if (path != string.Empty && !list.Contains(path)) list.Add(path);
        else Debug.Log("Foto ya añadida para compartir, no incluida");
    }

    private static string GetPathFromSprite(Sprite sprite) //A MIRAR SI SIGUE FUNCIONANDO SIN CODIFICAR, SÓLO OBTENIENDO EL PATH
    {
        if (sprite is null) return string.Empty;
        string textureName = sprite.texture.name;
        string path;
        byte[] bytes = new byte[0];
        try
        {
            path = Application.persistentDataPath + $"/{textureName}.png";
            if (!File.Exists(path)) bytes = sprite.texture.EncodeToPNG();
        }
        catch
        {
            path = Application.persistentDataPath + $"/{textureName}.jpg";
            if (!File.Exists(path)) bytes = sprite.texture.EncodeToJPG();
        }
        if (!File.Exists(path)) File.WriteAllBytes(path, bytes);
        return path;
    }
}
