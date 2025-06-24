using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class GameOptionsManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOptionPrefab; //Prefab de opciones de los juegos
    public static List<GameInfo> CustomGames { get; private set; } = new(); //Lista de juegos a�adidos (es decir, los juegos extra, no los base)
    private LayoutManager layoutManager; //Layout Manager para que se vea bien en portrait y landscape

    void Awake()
    {
        layoutManager = GetComponent<LayoutManager>();
        layoutManager.OnLayoutUpdated += (() => StartCoroutine(ResetScrollViews()));
    }

    public void AddGame(GameInfo newGameInfo, bool isDefaultGame = false) //A�ade un juego a la lista
    {
        //Instancia el bot�n y le pasa la info correspondiente:
        GameOption game = Instantiate(gameOptionPrefab, layoutManager.GetCurrentLayoutTransform()).GetComponent<GameOption>();
        game.Info = newGameInfo;
        //A�ade el contenido al Layout Manager para que sea escalable
        layoutManager.AddContent(game.transform);
        //Configuraciones finales dependiendo de si es un juego base o no (los base se configuran como tal, los que no se a�aden a la lista)
        if (!isDefaultGame) CustomGames.Add(newGameInfo);
        StartCoroutine(ResetScrollViews());
    }

    public void RemoveGame(GameInfo game, Transform buttonTransform) //Borra un juego de la lista
    {
        CustomGames.Remove(game);
        layoutManager.RemoveContent(buttonTransform); //Tiene que eliminarlo del layoutManager para que no salga error al girar el dispositivo
    }

    IEnumerator ResetScrollViews()
    {
        yield return null;
        foreach (var scroll in GetComponentsInChildren<ScrollRect>(true))
        {
            if (scroll.horizontal) yield return null;
            float prevElasticity = scroll.elasticity;
            scroll.elasticity = 0;
            scroll.verticalNormalizedPosition = 1;
            scroll.horizontalNormalizedPosition = 0;
            yield return null;
            scroll.elasticity = prevElasticity;
        }
    }
}
