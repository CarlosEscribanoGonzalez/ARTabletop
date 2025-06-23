using System.IO.Compression;
using UnityEngine;

public class ReplaceConfirmation : MonoBehaviour
{
    public static ReplaceConfirmation Instance { get; private set; }
    private GameInfo gameToReplace;
    private string content;
    private ZipArchive archive;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
        DontDestroyOnLoad(this.gameObject);
    }

    public void RequestConfirmation(string content, ZipArchive archive)
    {
        GetComponent<Canvas>().enabled = true;
        this.content = content;
        this.archive = archive;
        gameToReplace = GameInfo.FromJsonToSO(content, true);
    }

    public void RequestConfirmation(GameInfo gameInfo)
    {
        GetComponent<Canvas>().enabled = true;
        this.content = null;
        this.archive = null;
        gameToReplace = gameInfo;
    }

    public void ReplaceGame()
    {
        string gameID = IDCreator.GetCustomJsonID(gameToReplace);
        GameInfo originalGame = null;
        foreach(GameInfo game in GameOptionsManager.CustomGames)
        {
            if (IDCreator.GetCustomJsonID(game).Equals(gameID))
            {
                originalGame = game;
                break;
            }
        }
        GameOptionsManager.CustomGames.Remove(originalGame);
        GameOptionsManager.CustomGames.Add(gameToReplace); //Temporal, para que no se borren los archivos en común
        GameDeleter.DeleteGameFiles(originalGame);
        GameOptionsManager.CustomGames.Remove(gameToReplace);
        if (content != null) GameSaver.Instance.SaveGameInfo(content, archive);
        else FindFirstObjectByType<ToolManager>().CreateGame();
    }
}
