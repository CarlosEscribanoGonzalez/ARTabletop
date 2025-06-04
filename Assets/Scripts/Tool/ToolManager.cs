using UnityEngine;
using UnityEngine.UI;

public class ToolManager : MonoBehaviour
{
    [SerializeField] private Button createGameButton; 
    private GeneralSettingsBuilder generalSettingsBuilder;
    private CardBuilder cardBuilder;
    private PieceBuilder pieceBuilder;
    private GameInfo gameInfo;

    private void Awake()
    {
        gameInfo = ScriptableObject.CreateInstance<GameInfo>();
        generalSettingsBuilder = GetComponentInChildren<GeneralSettingsBuilder>(true);
        cardBuilder = GetComponentInChildren<CardBuilder>(true);
        pieceBuilder = GetComponentInChildren<PieceBuilder>(true);
        createGameButton.interactable = false;
    }

    public void ConfigureGame()
    {
        //General settings:
        gameInfo.autoShuffle = generalSettingsBuilder.AutoShuffle;
        gameInfo.gameHasDice = generalSettingsBuilder.GameHasDice;
        gameInfo.gameHasWheel = generalSettingsBuilder.GameHasWheel;
        gameInfo.gameHasCoins = generalSettingsBuilder.GameHasCoin;
        //Cards:
        gameInfo.defaultSprite = cardBuilder.DefaultImage;
        gameInfo.cardsInfo = cardBuilder.Cards;
        //Pieces:
        gameInfo.pieces = pieceBuilder.GetFinalPieces();
        gameInfo.numPieces = pieceBuilder.TotalPieces;
        gameInfo.defaultPiece = pieceBuilder.DefaultPiece;
    }

    public void SetGameName(string name)
    {
        gameInfo.gameName = name;
        if(!string.IsNullOrEmpty(name)) createGameButton.interactable = gameInfo.gameImage != null;
    }

    public void SetGameImage(Image image)
    {
        Sprite gameImage = image.sprite;
        if (gameImage == null) return; //En caso de que haya habido algún fallo
        gameInfo.gameImage = gameImage;
        createGameButton.interactable = !string.IsNullOrEmpty(gameInfo.gameName);
    }

    public void CreateGame()
    {
        pieceBuilder.DownloadInfo();
        //cardBuilder.DownloadInfo();
        GameSharer.CreateGame(gameInfo);
    }
}
