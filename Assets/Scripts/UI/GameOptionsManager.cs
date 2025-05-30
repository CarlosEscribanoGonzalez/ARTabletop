using UnityEngine;
using System.Collections.Generic;

public class GameOptionsManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOptionPrefab; //Prefab de opciones de los juegos
    public List<GameInfo> CustomGames { get; private set; } = new(); //Lista de juegos añadidos (es decir, los juegos extra, no los base)
    private LayoutManager layoutManager; //Layout Manager para que se vea bien en portrait y landscape

    void Awake()
    {
        layoutManager = GetComponent<LayoutManager>();
    }

    public void AddGame(GameInfo newGameInfo, bool isDefaultGame = false) //Añade un juego a la lista
    {
        //Instancia el botón y le pasa la info correspondiente:
        GameOption game = Instantiate(gameOptionPrefab, layoutManager.GetCurrentLayoutTransform()).GetComponent<GameOption>();
        game.Info = newGameInfo;
        //Añade el contenido al Layout Manager para que sea escalable
        layoutManager.AddContent(game.transform);
        //Configuraciones finales dependiendo de si es un juego base o no (los base se configuran como tal, los que no se añaden a la lista)
        if (isDefaultGame) game.ConfigureAsDefaultGame();
        else CustomGames.Add(newGameInfo);
    }

    public void RemoveGame(GameInfo game, Transform buttonTransform) //Borra un juego de la lista
    {
        CustomGames.Remove(game);
        layoutManager.RemoveContent(buttonTransform); //Tiene que eliminarlo del layoutManager para que no salga error al girar el dispositivo
    }
}
