using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GeneralSettingsBuilder : MonoBehaviour
{
    [SerializeField] private Toggle autoShuffleToggle;
    [SerializeField] private Toggle diceToggle;
    [SerializeField] private Toggle wheelToggle;
    [SerializeField] private Toggle coinToggle;

    [Header("Auto shuffle explanation: ")]
    [SerializeField] private float explanationFadeSpeed = 1;
    [SerializeField] private CanvasGroup explanationCanvasGroup;
    [SerializeField] private Transform deployExplanationButton;
    private bool shown = false;

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

    public void OnExplanationButtonClicked()
    {
        StopAllCoroutines();
        deployExplanationButton.localScale = new Vector3(1, -deployExplanationButton.localScale.y, 1);
        if (!shown) StartCoroutine(FadeInExplanation());
        else StartCoroutine(FadeOutExplanation());
    }

    IEnumerator FadeInExplanation()
    {
        shown = true;
        while(explanationCanvasGroup.alpha < 1)
        {
            explanationCanvasGroup.alpha += explanationFadeSpeed * Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator FadeOutExplanation()
    {
        shown = false;
        while (explanationCanvasGroup.alpha > 0)
        {
            explanationCanvasGroup.alpha -= explanationFadeSpeed * Time.deltaTime;
            yield return null;
        }
    }
}
