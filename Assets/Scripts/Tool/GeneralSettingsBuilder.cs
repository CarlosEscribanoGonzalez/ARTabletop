using UnityEngine;
using UnityEngine.UI;

public class GeneralSettingsBuilder : MonoBehaviour
{
    [SerializeField] private Toggle autoShuffleToggle;
    [SerializeField] private Toggle diceToggle;
    [SerializeField] private Toggle wheelToggle;
    [SerializeField] private Toggle coinToggle;

    public bool AutoShuffle => autoShuffleToggle.isOn;
    public bool GameHasDice => diceToggle.isOn;
    public bool GameHasWheel => wheelToggle.isOn;
    public bool GameHasCoin => coinToggle.isOn;

    public void SetInitInfo(GameInfo gameInfo)
    {
        autoShuffleToggle.isOn = gameInfo.autoShuffle;
        diceToggle.isOn = gameInfo.gameHasDice;
        wheelToggle.isOn = gameInfo.gameHasWheel;
        coinToggle.isOn = gameInfo.gameHasCoins;
    }
}
