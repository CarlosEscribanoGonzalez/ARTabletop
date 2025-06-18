using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using System.Linq;

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
        pieceManager.DefaultPiece = completeInfo.defaultPiece;
        //Tableros: 
        boardManager.BoardModels = completeInfo.boards3D.ToArray();
        boardManager.BoardSprites = completeInfo.boards2D.ToArray();
        //SpecialCards:
        SpecialCardGameManager[] managers = FindObjectsByType<SpecialCardGameManager>(FindObjectsSortMode.InstanceID);
        if (!completeInfo.isDefault) completeInfo.specialCardsInfo.Reverse();
        for(int i = 0; i < managers.Length; i++) 
        {
            if (i < completeInfo.specialCardsInfo.Count)
            {
                managers[i].CardsInfo = completeInfo.specialCardsInfo[i].cardsInfo.ToArray();
                managers[i].DefaultImage = completeInfo.specialCardsInfo[i].defaultSpecialSprite;
                managers[i].CardTypeName = completeInfo.specialCardsInfo[i].name;
            }
            else managers[i].gameObject.SetActive(false);
        }
    }
}
