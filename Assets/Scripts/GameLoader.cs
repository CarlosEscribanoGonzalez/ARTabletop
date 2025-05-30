using System.IO;
using System.Linq;
using UnityEngine;

public class GameLoader : MonoBehaviour
{
    [SerializeField] private GameInfo[] baseGames;

    public void OnEnable()
    {
        GameOptionsManager gameOptionsManager = FindFirstObjectByType<GameOptionsManager>();
        foreach (GameInfo game in baseGames) gameOptionsManager.AddGame(game, true); 
        LoadSavedGames();
    }

    public static void LoadGame(GameInfo game)
    {
        GameOptionsManager gameOptionsManager = FindFirstObjectByType<GameOptionsManager>();
        if (gameOptionsManager != null) gameOptionsManager.AddGame(game);
    }

    private void LoadSavedGames()
    {
        string rootPath = Application.persistentDataPath;
        DirectoryInfo dirInfo = new DirectoryInfo(rootPath);
        FileInfo[] gameFiles = dirInfo.GetFiles("*.artabletop")
                                 .OrderBy(f => f.CreationTime)
                                 .ToArray();
        GameInfo loadedGame;
        
        foreach (FileInfo file in gameFiles)
        {
            Debug.Log("Cargando un juego..." + file);
            try
            {
                loadedGame = GameInfo.FromJsonToSO(File.ReadAllText(file.FullName));
                LoadGame(loadedGame);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error cargando un juego: " + e);
            }
        }
    }
}
