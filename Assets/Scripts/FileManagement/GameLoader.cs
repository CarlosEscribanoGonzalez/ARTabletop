using System.IO;
using System.Linq;
using UnityEngine;

public class GameLoader : MonoBehaviour
{
    [SerializeField] private GameInfo[] baseGames; //Juegos base de la app

    public void OnEnable()
    {
        GameOptionsManager gameOptionsManager = FindFirstObjectByType<GameOptionsManager>();
        foreach (GameInfo game in baseGames) gameOptionsManager.AddGame(game, true); //Añade los juegos base al menú principal
        LoadSavedGames(); //Carga los juegos guardados en memoria
    }

    public static void LoadGame(GameInfo game) //Carga un juego en el menú principal a partir de su SO
    {
        GameOptionsManager gameOptionsManager = FindFirstObjectByType<GameOptionsManager>();
        if (gameOptionsManager != null) gameOptionsManager.AddGame(game);
    }

    private void LoadSavedGames() //Busca todos los json con extensión .artabletop y los carga para añadirlos al menú principal
    {
        string rootPath = Application.persistentDataPath;
        DirectoryInfo dirInfo = new DirectoryInfo(rootPath);
        FileInfo[] gameFiles = dirInfo.GetFiles("*.artabletop")
                                 .OrderBy(f => f.CreationTime)
                                 .ToArray(); //Busca todos los json y los ordena por orden de instalación en el dispositivo
        GameInfo loadedGame;
        foreach (FileInfo file in gameFiles)
        {
            Debug.Log("Cargando un juego..." + file);
            try
            {
                loadedGame = GameInfo.FromJsonToSO(File.ReadAllText(file.FullName), true); //Deserializa la info y la convierte en SO
                LoadGame(loadedGame); //Carga el juego a partir del SO
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error cargando un juego: " + e);
                FeedbackManager.Instance.DisplayMessage("Unexpected error: some games couldn't be loaded.");
            }
        }
    }
}
