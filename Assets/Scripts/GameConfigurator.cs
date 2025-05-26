using UnityEngine;
using System.Collections.Generic;

public class GameConfigurator : MonoBehaviour
{
    [SerializeField] private GameSettings gameSettings;
    [SerializeField] private RNGSection rngSection;
    [SerializeField] private CardGameManager cardManager;
    [SerializeField] private PieceGameManager pieceManager;
    [SerializeField] private BoardGameManager boardManager;
    [SerializeField] private GameObject specialCardManagerPrefab;
    public static GameInfo gameInfo;

    void Awake()
    {
        //General settings:
        gameSettings.ExtendedTracking = gameInfo.extendedTracking;
        gameSettings.AutoShuffle = gameInfo.autoShuffle;
        //RNG section:
        rngSection.GameHasDice = gameInfo.gameHasDice;
        rngSection.GameHasWheel = gameInfo.gameHasWheel;
        rngSection.GameHasCoins = gameInfo.gameHasCoins;
        //Cards normales:
        cardManager.CardsInfo = gameInfo.cardsInfo.ToArray();
        cardManager.DefaultImage = gameInfo.defaultSprite;
        /*/Piezas:
        List<GameObject> pieces = new();
        for(int i = 0; i < gameInfo.numPieces; i++)
        {
            if (i < gameInfo.pieces.Count) pieces.Add(gameInfo.pieces[i]);
            else pieces.Add(gameInfo.defaultPiece);
        }
        pieceManager.Pieces = pieces.ToArray();
        //Tableros: 
        boardManager.BoardModels = gameInfo.boards3D.ToArray();
        boardManager.BoardSprites = gameInfo.boards2D.ToArray();
        */
        //SpecialCards:
        SpecialCardGameManager newManager;
        for(int i = 0; i < gameInfo.specialCardsInfo.Count; i++)
        {
            newManager = Instantiate(specialCardManagerPrefab).GetComponent<SpecialCardGameManager>();
            newManager.CardsInfo = gameInfo.specialCardsInfo[i].cardsInfo.ToArray();
            newManager.DefaultImage = gameInfo.specialCardsInfo[i].defaultSpecialSprite;
            newManager.CardTypeName = gameInfo.specialCardsInfo[i].name;
        }
    }
}
