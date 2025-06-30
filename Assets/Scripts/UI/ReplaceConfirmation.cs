using System.IO.Compression;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReplaceConfirmation : MonoBehaviour
{
    public static ReplaceConfirmation Instance { get; private set; }
    private GameInfo gameToReplace;
    private string content = null;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
        DontDestroyOnLoad(this.gameObject);
    }

    public void RequestConfirmation(string content)
    {
        GetComponent<Canvas>().enabled = true;
        this.content = content;
        gameToReplace = GameInfo.FromJsonToSO(content, true);
    }

    public void RequestConfirmation(GameInfo gameInfo)
    {
        GetComponent<Canvas>().enabled = true;
        this.content = null;
        gameToReplace = gameInfo;
    }

    public void ReplaceGame()
    {
        LoadingScreenManager.ToggleLoadingScreen(true, true, "Replacing game...");
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
        if(content == null) GameOptionsManager.CustomGames.Add(gameToReplace); //Temporal, para que no se borren los archivos en común, lo que es un problema para los modelos del tool
        GameDeleter.DeleteGameFiles(originalGame);
        if(content == null) GameOptionsManager.CustomGames.Remove(gameToReplace);
        if (content != null)
        {
            GameSaver.Instance.OnIntentReceived("Replace");
            if (SceneManager.GetActiveScene().buildIndex == 0) SceneManager.LoadScene(0); //Se recarga la escena para que los botones aparezcan bien
        }
        else
        {
            LoadingScreenManager.UnlockBySceneChange = true;
            FindFirstObjectByType<ToolManager>().CreateGame();
        }
    }
}
