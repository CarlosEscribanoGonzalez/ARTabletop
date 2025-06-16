using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class GameConfigurator : MonoBehaviour
{
    [SerializeField] private GameSettings gameSettings;
    [SerializeField] private RNGSection rngSection;
    [SerializeField] private CardGameManager cardManager;
    [SerializeField] private PieceGameManager pieceManager;
    [SerializeField] private BoardGameManager boardManager;
    [SerializeField] private GameObject specialCardManagerPrefab;
    public static GameInfo EssentialInfo { get; set; } = null; //Información esencial: únicamente nombre y foto
    private GameInfo completeInfo; //Información completa

    void Awake()
    {
        if (EssentialInfo.isDefault) completeInfo = EssentialInfo;
        else completeInfo = GameInfo.GetFullInfo(EssentialInfo);
        //General settings:
        gameSettings.AutoShuffle = completeInfo.autoShuffle;
        //RNG section:
        rngSection.GameHasDice = completeInfo.gameHasDice;
        rngSection.GameHasWheel = completeInfo.gameHasWheel;
        rngSection.GameHasCoins = completeInfo.gameHasCoins;
        //Cards normales:
        cardManager.CardsInfo = completeInfo.cardsInfo.ToArray();
        cardManager.DefaultImage = completeInfo.defaultSprite;
        //Piezas:
        List<GameObject> pieces = new();
        for(int i = 0; i < completeInfo.numPieces; i++)
        {
            if (i < completeInfo.pieces.Count) pieces.Add(completeInfo.pieces[i]);
            else pieces.Add(completeInfo.defaultPiece);
        }
        pieceManager.Pieces = pieces.ToArray();
        //Tableros: 
        boardManager.BoardModels = completeInfo.boards3D.ToArray();
        boardManager.BoardSprites = completeInfo.boards2D.ToArray();
        //SpecialCards:
        SpecialCardGameManager newManager;
        for(int i = 0; i < completeInfo.specialCardsInfo.Count; i++)
        {
            newManager = Instantiate(specialCardManagerPrefab).GetComponent<SpecialCardGameManager>();
            newManager.CardsInfo = completeInfo.specialCardsInfo[i].cardsInfo.ToArray();
            newManager.DefaultImage = completeInfo.specialCardsInfo[i].defaultSpecialSprite;
            newManager.CardTypeName = completeInfo.specialCardsInfo[i].name;
        }
    }
}
