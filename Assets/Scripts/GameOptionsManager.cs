using UnityEngine;
using System.Collections.Generic;

public class GameOptionsManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOptionPrefab;
    public List<GameInfo> Games { get; private set; } = new();
    private LayoutManager layoutManager;

    void Awake()
    {
        layoutManager = GetComponent<LayoutManager>();
    }

    public void AddGame(GameInfo newGameInfo, bool isDefaultGame = false)
    {
        GameOption game = Instantiate(gameOptionPrefab, layoutManager.GetCurrentLayoutTransform()).GetComponent<GameOption>();
        game.Info = newGameInfo;
        layoutManager.AddContent(game.transform);
        if (isDefaultGame) game.ConfigureAsDefaultGame();
        else Games.Add(newGameInfo);
    }

    public void RemoveGame(GameInfo game)
    {
        Games.Remove(game);
    }
}
