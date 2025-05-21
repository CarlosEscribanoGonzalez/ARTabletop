using UnityEngine;
using Serialization;
using System.Collections.Generic;

public class GameOptionsManager : MonoBehaviour
{
    [SerializeField] private GameInfo[] games;
    [SerializeField] private GameObject gameOptionPrefab;

    void Start()
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
}
