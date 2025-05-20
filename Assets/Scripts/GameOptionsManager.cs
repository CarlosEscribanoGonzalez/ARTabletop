using UnityEngine;

public class GameOptionsManager : MonoBehaviour
{
    [SerializeField] private GameInfo[] games;
    [SerializeField] private GameObject gameOptionPrefab;

    void Start()
    {
        foreach (var info in games) 
        {
            GameOption game = Instantiate(gameOptionPrefab, this.transform).GetComponent<GameOption>();
            game.Info = info;
        }
    }
}
