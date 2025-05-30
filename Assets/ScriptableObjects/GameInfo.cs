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
    public List<SpecialCardInfo> specialCardsInfo = new();

    public void Delete()
    {
        LoadingScreenManager.ToggleLoadingScreen(true);
        GameLoader gameLoader = FindFirstObjectByType<GameLoader>();
        GameOptionsManager gameOptionsManager = FindFirstObjectByType<GameOptionsManager>();
        gameLoader.DeleteGameInfo(ConvertGameInfoToJSON());
        gameOptionsManager.RemoveGame(this);
        foreach(string textureName in GetAllUsedTextures())
        {
            bool contained = false;
            foreach(GameInfo game in gameOptionsManager.Games)
            {
                List<string> otherGamesTextures = game.GetAllUsedTextures();
                if (otherGamesTextures.Contains(textureName))
                {
                    contained = true;
                    Debug.Log("Imagen " + textureName + " no ha podido ser eliminada porque es usada por " + game.gameName);
                    break;
                }
            }
            if (!contained) gameLoader.DeleteImage(textureName);
        }
        LoadingScreenManager.ToggleLoadingScreen(false);
    }

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

        LoadingScreenManager.ToggleLoadingScreen(false);
    }

    private List<string> GetAllUsedTextures()
    {
        List<string> textureNameList = new();
        AddTextureToList(textureNameList, gameImage);
        foreach (var c in cardsInfo) AddTextureToList(textureNameList, c.sprite);
        AddTextureToList(textureNameList, defaultSprite);
        foreach (var b in boards2D) AddTextureToList(textureNameList, b);
        foreach (var sc in specialCardsInfo)
        {
            AddTextureToList(textureNameList, sc.defaultSpecialSprite);
            foreach (var c in sc.cardsInfo) AddTextureToList(textureNameList, c.sprite);
        }
        return textureNameList;
    }

    private void AddTextureToList(List<string> textureList, Sprite sprite)
    {
        if (sprite == null || textureList.Contains(sprite.texture.name)) return;
        textureList.Add(sprite.texture.name);
    }

    private string ConvertGameInfoToJSON()
    {
        var gameInfoSerializable = new GameInfoSerializable
        {
            //General settings:
            gameName = this.gameName,
            gameImageFileName = this.gameImage != null ? this.gameImage.texture.name : null,
            //RNG section:
            autoShuffle = this.autoShuffle,
            gameHasDice = this.gameHasDice,
            gameHasWheel = this.gameHasWheel,
            gameHasCoins = this.gameHasCoins,
            //Cards:
            cardsInfo = this.cardsInfo.Select(card => new CardInfoSerializable
            {
                spriteFileName = card.sprite != null ? card.sprite.texture.name : null,
                text = card.text,
                size = card.sizeMult
            }).ToList(),
            defaultSpriteFileName = this.defaultSprite != null ? this.defaultSprite.texture.name : null,
            //Boards:
            boardImagesNames = this.boards2D.Select(board => board.texture.name).ToList(),
            //Special cards:
            specialCardsInfo = this.specialCardsInfo.Select(card => new SpecialCardInfoSerializable
            {
                name = card.name,
                cardsInfo = card.cardsInfo.Select(c => new CardInfoSerializable
                {
                    spriteFileName = c.sprite != null ? c.sprite.texture.name : null,
                    text = c.text,
                    size = c.sizeMult
                }).ToList(),
                defaultSpriteFileName = card.defaultSpecialSprite != null ? card.defaultSpecialSprite.texture.name : null
            }).ToList()
        };
        string path = Path.Combine(Application.persistentDataPath, GameLoader.GetCustomGameID(gameInfoSerializable));
        if(!File.Exists(path)) File.WriteAllText(path, JsonUtility.ToJson(gameInfoSerializable, true));
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
        FindFirstObjectByType<GameLoader>().zipPathToRemove = zipPath;
        return zipPath;
    }

    private List<string> GetImagePaths()
    {
        List<string> listToReturn = new();
        AddPathIfNotContained(listToReturn, GetPathFromSprite(gameImage));
        foreach(var cardInfo in cardsInfo)
        {
            AddPathIfNotContained(listToReturn, GetPathFromSprite(cardInfo.sprite));
        }
        AddPathIfNotContained(listToReturn, GetPathFromSprite(defaultSprite));
        foreach(var specialCard in specialCardsInfo)
        {
            AddPathIfNotContained(listToReturn, GetPathFromSprite(specialCard.defaultSpecialSprite));
            foreach (var cardInfo in specialCard.cardsInfo) AddPathIfNotContained(listToReturn, GetPathFromSprite(cardInfo.sprite));
        }
        foreach(var board2d in boards2D)
        {
            AddPathIfNotContained(listToReturn, GetPathFromSprite(board2d));
        }
        return listToReturn;
    }

    private void AddPathIfNotContained(List<string> list, string path)
    {
        if (path != string.Empty && !list.Contains(path)) list.Add(path);
        else Debug.Log("Foto ya añadida para compartir, no incluida");
    }

    private string GetPathFromSprite(Sprite sprite) //A MIRAR SI SIGUE FUNCIONANDO SIN CODIFICAR, SÓLO OBTENIENDO EL PATH
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