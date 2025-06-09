using UnityEngine;
using UnityEngine.UI;
using System.IO;

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
        gameInfo.cardsInfo = cardBuilder.Content;
        //Pieces:
        gameInfo.pieces = pieceBuilder.GetFinalPieces();
        gameInfo.numPieces = pieceBuilder.TotalPieces;
        gameInfo.defaultPiece = pieceBuilder.DefaultPiece;
    }

    public void SetGameName(string name)
    {
        gameInfo.gameName = name;
        createGameButton.interactable = (gameInfo.gameImage != null && !string.IsNullOrEmpty(name));
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
        if (File.Exists(Path.Combine(Application.persistentDataPath, IDCreator.GetCustomJsonID(gameInfo))))
        {
            Debug.Log("Archivo con el mismo nombre e imagen ya existe, no se crea nada");
            return;
        }
        ContentDownloader.DownloadSprite(gameInfo.gameImage);
        pieceBuilder.DownloadInfo();
        cardBuilder.DownloadInfo();
        gameInfo.ConvertToJson();
    }
}
