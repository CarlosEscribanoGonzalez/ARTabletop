using UnityEngine;
using System.Collections.Generic;
using Serialization;
using System.Linq;
using System.IO;

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
        GameDeleter.DeleteGame(this);
    }

    public void Share()
    {
        GameSharer.Share(this);
    }

    public string ConvertToJson()
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
        string path = Path.Combine(Application.persistentDataPath, GetCustomID());
        if (!File.Exists(path)) File.WriteAllText(path, JsonUtility.ToJson(gameInfoSerializable, true));
        return path;
    }

    public string GetCustomID()
    {
        string name = gameName;
        string imageName = gameImage.texture.name;
        int dif = specialCardsInfo.Count * cardsInfo.Count + boards2D.Count; //Número diferenciador en caso de que tengan dos juegos el mismo nombre y la misma imagen
        return name + "_" + imageName[0].GetHashCode() + imageName[imageName.Length - 1].GetHashCode() + "_" + dif + ".artabletop";
    }

    public static GameInfo FromJsonToSO(string json)
    {
        GameInfo newGameInfo = ScriptableObject.CreateInstance<GameInfo>();

        GameInfoSerializable deserialized = JsonUtility.FromJson<GameInfoSerializable>(json);
        //General settings:
        newGameInfo.gameName = deserialized.gameName;
        newGameInfo.gameImage = AssignSprite(deserialized.gameImageFileName);
        //RNG section:
        newGameInfo.autoShuffle = deserialized.autoShuffle;
        newGameInfo.gameHasDice = deserialized.gameHasDice;
        newGameInfo.gameHasWheel = deserialized.gameHasWheel;
        newGameInfo.gameHasCoins = deserialized.gameHasCoins;
        //Cards:
        newGameInfo.cardsInfo = new List<CardInfo>();
        foreach (var card in deserialized.cardsInfo)
        {
            CardInfo cardInfo = new CardInfo();
            cardInfo.text = card.text;
            cardInfo.sprite = AssignSprite(card.spriteFileName);
            cardInfo.sizeMult = card.size;
            newGameInfo.cardsInfo.Add(cardInfo);
        }
        newGameInfo.defaultSprite = AssignSprite(deserialized.defaultSpriteFileName);
        //Boards:
        foreach (var board2d in deserialized.boardImagesNames)
        {
            newGameInfo.boards2D.Add(AssignSprite(board2d));
        }
        //Special cards:
        foreach (var scard in deserialized.specialCardsInfo)
        {
            SpecialCardInfo specialCardInfo = new SpecialCardInfo();
            specialCardInfo.name = scard.name;
            foreach (var card in scard.cardsInfo)
            {
                CardInfo cardInfo = new CardInfo();
                cardInfo.text = card.text;
                cardInfo.sprite = AssignSprite(card.spriteFileName);
                cardInfo.sizeMult = card.size;
                specialCardInfo.cardsInfo.Add(cardInfo);
            }
            specialCardInfo.defaultSpecialSprite = AssignSprite(scard.defaultSpriteFileName);
            newGameInfo.specialCardsInfo.Add(specialCardInfo);
        }
        return newGameInfo;
    }

    static string path;
    static byte[] imgData;
    static Texture2D texture;
    private static Sprite AssignSprite(string textureName)
    {
        if (textureName == string.Empty) return null;
        string[] supportedExtensions = { ".png", ".jpg", ".jpeg" };
        foreach (var ext in supportedExtensions)
        {
            path = Path.Combine(Application.persistentDataPath, textureName + ext);
            if (File.Exists(path))
            {
                imgData = File.ReadAllBytes(path);
                texture = new Texture2D(0, 0); //El tamaño se autoajusta más tarde
                texture.name = textureName;
                texture.LoadImage(imgData);
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }
        Debug.LogError($"La textura {textureName} no fue encontrada en {path}");
        return null;
    }
}