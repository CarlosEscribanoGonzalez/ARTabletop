using UnityEngine;
using Serialization;
using System.Collections.Generic;
using System.IO;

public class GameOptionsManager : MonoBehaviour
{
    [SerializeField] private List<GameInfo> games = new();
    [SerializeField] private GameObject gameOptionPrefab;
    private LayoutManager layoutManager;

    void Awake()
    {
        layoutManager = GetComponent<LayoutManager>();
        foreach (var info in games)
        {
            GameOption game = Instantiate(gameOptionPrefab, layoutManager.GetCurrentLayoutTransform()).GetComponent<GameOption>();
            game.Info = info;
           layoutManager.AddContent(game.transform);
        }
    }

    public void AddNewGame(string jsonInfo)
    {
        GameInfo newGameInfo = ScriptableObject.CreateInstance<GameInfo>();

        GameInfoSerializable deserialized = JsonUtility.FromJson<GameInfoSerializable>(jsonInfo);
        //General settings:
        newGameInfo.gameName = deserialized.gameName;
        newGameInfo.gameImage = AssignSprite(deserialized.gameImageFileName);
        //RNG section:
        newGameInfo.autoShuffle = deserialized.autoShuffle;
        newGameInfo.extendedTracking = deserialized.extendedTracking;
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
        foreach(var board2d in deserialized.boardImagesNames)
        {
            newGameInfo.boards2D.Add(AssignSprite(board2d));
        }
        //Special cards:
        foreach(var scard in deserialized.specialCardsInfo)
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

        GameOption game = Instantiate(gameOptionPrefab, layoutManager.GetCurrentLayoutTransform()).GetComponent<GameOption>();
        game.Info = newGameInfo;
        GetComponent<LayoutManager>().AddContent(game.transform);
    }

    string path;
    byte[] imgData;
    Texture2D texture;
    private Sprite AssignSprite(string textureName)
    {
        if (textureName == string.Empty) return null;
        string[] supportedExtensions = { ".png", ".jpg", ".jpeg" };
        foreach(var ext in supportedExtensions)
        {
            path = Path.Combine(Application.persistentDataPath, textureName + ext);
            if (File.Exists(path))
            {
                imgData = File.ReadAllBytes(path);
                texture = new Texture2D(0, 0); //El tamaño se autoajusta más tarde
                texture.LoadImage(imgData);
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }
        Debug.LogError($"La textura {textureName} no fue encontrada en {path}");
        return null;

    }
}
