using UnityEngine;
using Serialization;
using System.Collections.Generic;
using System.IO;

public class GameOptionsManager : MonoBehaviour
{
    [SerializeField] private List<GameInfo> games = new();
    [SerializeField] private GameObject gameOptionPrefab;

    void Awake()
    {
        foreach (var info in games)
        {
            GameOption game = Instantiate(gameOptionPrefab, this.transform).GetComponent<GameOption>();
            game.Info = info;
        }
    }

    public void AddNewGame(string jsonInfo)
    {
        GameInfo newGameInfo = ScriptableObject.CreateInstance<GameInfo>();
        
        //Faltarían todos los archivos, de momento para hacer la prueba sólo se pasan ajustes generales y cartas
        GameInfoSerializable deserialized = JsonUtility.FromJson<GameInfoSerializable>(jsonInfo);
        newGameInfo.gameName = deserialized.gameName;
        newGameInfo.gameImage = AssignSprite(deserialized.gameImageFileName);
        newGameInfo.autoShuffle = deserialized.autoShuffle;
        newGameInfo.extendedTracking = deserialized.extendedTracking;
        newGameInfo.gameHasDice = deserialized.gameHasDice;
        newGameInfo.gameHasWheel = deserialized.gameHasWheel;
        newGameInfo.gameHasCoins = deserialized.gameHasCoins;
        newGameInfo.cardsInfo = new List<CardInfo>();
        foreach (var card in deserialized.cardsInfo)
        {
            CardInfo cardInfo = new CardInfo();
            cardInfo.text = card.text;
            cardInfo.sizeMult = card.size;
            newGameInfo.cardsInfo.Add(cardInfo);
        }

        GameOption game = Instantiate(gameOptionPrefab, this.transform).GetComponent<GameOption>();
        game.Info = newGameInfo;
    }

    string path;
    byte[] imgData;
    Texture2D texture;
    private Sprite AssignSprite(string textureName)
    {
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
