using UnityEngine;
using UnityEngine.UI;

public class ToolManager : MonoBehaviour
{
    [SerializeField] private Button createGameButton; 
    private CardBuilder cardBuilder;
    private GeneralSettingsBuilder generalSettingsBuilder;
    private GameInfo gameInfo;

    private void Awake()
    {
        gameInfo = ScriptableObject.CreateInstance<GameInfo>();
        cardBuilder = GetComponentInChildren<CardBuilder>(true);
        generalSettingsBuilder = GetComponentInChildren<GeneralSettingsBuilder>(true);
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
    }

    public void SetGameName(string name)
    {
        gameInfo.gameName = name;
        createGameButton.interactable = gameInfo.gameImage != null;
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
        GameSharer.CreateGame(gameInfo);
    }
}
