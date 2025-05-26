using UnityEngine;
using System.Collections.Generic;
using System;
using Serialization;
using System.Linq;
using System.IO;
using System.IO.Compression;

[CreateAssetMenu(menuName = "ScriptableObjects/GameInfo")]
public class GameInfo : ScriptableObject
{
    [Header("Game info: ")]
    public string gameName;
    public Sprite gameImage;

    [Header("General settings: ")]
    public bool autoShuffle = true;
    public bool extendedTracking = false;
    public bool gameHasDice = true;
    public bool gameHasWheel = true;
    public bool gameHasCoins = true;

    [Header("Cards: ")]
    public List<CardInfo> cardsInfo = new();
    public Sprite defaultSprite;

    [Header("Pieces: ")]
    public int numPieces;
    public GameObject defaultPiece;
    public List<GameObject> pieces = new();

    [Header("Boards: ")]
    public List<GameObject> boards3D = new();
    public List<Sprite> boards2D = new();

    [Header("SpecialCards")]
    public List<SpecialCardInfo> specialCardsInfo;

    public void Share()
    {
        List<string> files = new();
        files.Add(ConvertGameInfoToJSON());
        files.AddRange(GetImagePaths());

        string zipPath = CreateZip(gameName, files);

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
    }

    private string ConvertGameInfoToJSON()
    {
        var gameInfoSerializable = new GameInfoSerializable
        {
            gameName = this.gameName,
            gameImageFileName = this.gameImage != null ? this.gameImage.texture.name : null,

            autoShuffle = this.autoShuffle,
            extendedTracking = this.extendedTracking,
            gameHasDice = this.gameHasDice,
            gameHasWheel = this.gameHasWheel,
            gameHasCoins = this.gameHasCoins,

            cardsInfo = this.cardsInfo.Select(card => new CardInfoSerializable
            {
                spriteFileName = card.sprite != null ? card.sprite.name + ".png" : null,
                text = card.text,
                size = card.sizeMult
            }).ToList(),

            defaultSpriteFileName = this.defaultSprite != null ? this.defaultSprite.name + ".png" : null
        };
        string path = Application.persistentDataPath + $"/{gameName}_info.artabletop";
        File.WriteAllText(path, JsonUtility.ToJson(gameInfoSerializable, true));
        return path;
    }

    private string CreateZip(string zipName, List<string> files)
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

        return zipPath;
    }

    private List<string> GetImagePaths()
    {
        List<string> listToReturn = new();
        listToReturn.Add(GetPathFromSprite(gameImage));
        return listToReturn;
    }

    private string GetPathFromSprite(Sprite sprite)
    {
        string textureName = sprite.texture.name;
        string path;
        byte[] bytes;
        try
        {
            bytes = sprite.texture.EncodeToPNG();
            path = Application.persistentDataPath + $"/{textureName}.png";
        } 
        catch
        {
            bytes = sprite.texture.EncodeToJPG();
            path = Application.persistentDataPath + $"/{textureName}.jpg";
        }
        File.WriteAllBytes(path, bytes);
        return path;
    }
}

namespace Serialization
{
    [Serializable]
    public class SpecialCardInfo
    {
        public string name;
        public List<CardInfo> cardsInfo = new();
        public Sprite defaultSpecialSprite;
    }

    [Serializable]
    public class CardInfoSerializable
    {
        public string spriteFileName;
        public string text;
        public float size;
    }

    public class GameInfoSerializable
    {
        public string gameName;
        public string gameImageFileName;
        public bool autoShuffle;
        public bool extendedTracking;
        public bool gameHasDice;
        public bool gameHasWheel;
        public bool gameHasCoins;
        public List<CardInfoSerializable> cardsInfo;
        public string defaultSpriteFileName;
    }
}