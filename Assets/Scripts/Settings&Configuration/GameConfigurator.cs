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
    public static GameInfo GameInfo { get; set; } = null;

    void Awake()
    {
        //General settings:
        gameSettings.AutoShuffle = GameInfo.autoShuffle;
        //RNG section:
        rngSection.GameHasDice = GameInfo.gameHasDice;
        rngSection.GameHasWheel = GameInfo.gameHasWheel;
        rngSection.GameHasCoins = GameInfo.gameHasCoins;
        //Cards normales:
        cardManager.CardsInfo = GameInfo.cardsInfo.ToArray();
        cardManager.DefaultImage = GameInfo.defaultSprite;
        //Piezas:
        List<GameObject> pieces = new();
        for(int i = 0; i < GameInfo.numPieces; i++)
        {
            if (i < GameInfo.pieces.Count) pieces.Add(GameInfo.pieces[i]);
            else pieces.Add(GameInfo.defaultPiece);
        }
        pieceManager.Pieces = pieces.ToArray();
        //Tableros: 
        boardManager.BoardModels = GameInfo.boards3D.ToArray();
        boardManager.BoardSprites = GameInfo.boards2D.ToArray();
        //SpecialCards:
        SpecialCardGameManager newManager;
        for(int i = 0; i < GameInfo.specialCardsInfo.Count; i++)
        {
            newManager = Instantiate(specialCardManagerPrefab).GetComponent<SpecialCardGameManager>();
            newManager.CardsInfo = GameInfo.specialCardsInfo[i].cardsInfo.ToArray();
            newManager.DefaultImage = GameInfo.specialCardsInfo[i].defaultSpecialSprite;
            newManager.CardTypeName = GameInfo.specialCardsInfo[i].name;
        }
    }
}
