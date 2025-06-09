using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using Serialization;

public class ToolManager : MonoBehaviour
{
    [SerializeField] private Button createGameButton; 
    private GeneralSettingsBuilder generalSettingsBuilder;
    private CardBuilder cardBuilder;
    private PieceBuilder pieceBuilder;
    private BoardBuilder boardBuilder;
    private SpecialCardBuilder scardBuilder;
    private GameInfo gameInfo;

    private void Awake()
    {
        gameInfo = ScriptableObject.CreateInstance<GameInfo>();
        generalSettingsBuilder = GetComponentInChildren<GeneralSettingsBuilder>(true);
        cardBuilder = GetComponentInChildren<CardBuilder>(true);
        pieceBuilder = GetComponentInChildren<PieceBuilder>(true);
        boardBuilder = GetComponentInChildren<BoardBuilder>(true);
        scardBuilder = GetComponentInChildren<SpecialCardBuilder>(true);
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
        //Boards:
        foreach(var image in boardBuilder.Content.Where(go => go.GetComponentInChildren<SpriteRenderer>() != null))
        {
            gameInfo.boards2D.Add(image.GetComponentInChildren<SpriteRenderer>().sprite);
        }
        foreach (var model in boardBuilder.Content.Where(go => go.GetComponentInChildren<SpriteRenderer>() == null))
        {
            gameInfo.boards3D.Add(model);
        }
        //Special cards:
        foreach(var scard in scardBuilder.Content)
        {
            SpecialCardInfo newInfo = new();
            newInfo.defaultSpecialSprite = scard.DefaultImage;
            newInfo.cardsInfo = scard.Content;
            newInfo.name = scardBuilder.Names[scardBuilder.Content.IndexOf(scard)];
            gameInfo.specialCardsInfo.Add(newInfo);
        }
        gameInfo.specialCardsInfo.Reverse(); //Por algún motivo no salen en el orden bueno, salen al revés
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
        scardBuilder.DownloadInfo();
        boardBuilder.DownloadInfo();
        pieceBuilder.DownloadInfo();
        cardBuilder.DownloadInfo();
        gameInfo.ConvertToJson();
    }
}
