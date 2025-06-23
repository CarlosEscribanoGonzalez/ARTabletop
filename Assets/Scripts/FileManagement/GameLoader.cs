using System.IO;
using System.Linq;
using UnityEngine;

public class GameLoader : MonoBehaviour
{
    [SerializeField] private GameInfo[] baseGames; //Juegos base de la app

    public void OnEnable()
    {
        GameOptionsManager gameOptionsManager = FindFirstObjectByType<GameOptionsManager>();
        foreach (GameInfo game in baseGames) gameOptionsManager.AddGame(game, true); //A�ade los juegos base al men� principal
        LoadSavedGames(); //Carga los juegos guardados en memoria
    }

    public static void LoadGame(GameInfo game) //Carga un juego en el men� principal a partir de su SO
    {
        GameOptionsManager gameOptionsManager = FindFirstObjectByType<GameOptionsManager>();
        if (gameOptionsManager != null) gameOptionsManager.AddGame(game);
    }

    private void LoadSavedGames() //Busca todos los json con extensi�n .artabletop y los carga para a�adirlos al men� principal
    {
        string rootPath = Application.persistentDataPath;
        DirectoryInfo dirInfo = new DirectoryInfo(rootPath);
        FileInfo[] gameFiles = dirInfo.GetFiles("*.artabletop")
                                 .OrderBy(f => f.CreationTime)
                                 .ToArray(); //Busca todos los json y los ordena por orden de instalaci�n en el dispositivo
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
