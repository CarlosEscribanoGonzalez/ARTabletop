using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class WheelManager : MonoBehaviour
{
    [SerializeField] private bool gameHasWheel = true;
    [SerializeField] private Transform wheelTransform;
    [SerializeField] private GameObject optionPrefab;
    [SerializeField] private Button addButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button spinButton;
    [SerializeField] private GameObject resultPanel;
    private WheelOptionList optionList;
    private Toggle removeWinnerToggle;
    [Header("Spin settings:")]
    [SerializeField] private float spinDuration = 5;
    [SerializeField] private Vector2 spinDegreesMinMax = new Vector2(1080, 3240);
    private List<WheelOption> options = new();
    private float endRotation = 0;

    private void Start()
    {
        if (!gameHasWheel) Destroy(this.gameObject);
        optionList = FindFirstObjectByType<WheelOptionList>(FindObjectsInactive.Include);
        removeWinnerToggle = GetComponentInChildren<Toggle>(true);
        GameSettings.Instance.OnSeedSet += () => GetComponentInChildren<Canvas>(true).enabled = true; //Cuando se entra en partida se activa la UI
        for(int i = 0; i < 3; i++) AddOption(); //Tres opciones iniciales
    }

    public void AddOption()
    {
        WheelOption option = Instantiate(optionPrefab, wheelTransform).GetComponent<WheelOption>();
        option.Initialize();
        options.Add(option);
        UpdateWheel();
    }

    public void RemoveOption(WheelOption option)
    {
        options.Remove(option);
        Destroy(option.gameObject);
        UpdateWheel();
    }

    public void SpinWheel()
    {
        addButton.interactable = false;
        foreach (Button button in optionList.GetComponentsInChildren<Button>()) button.interactable = false;
        StartCoroutine(Spin(Random.Range(spinDegreesMinMax[0], spinDegreesMinMax[1]), spinDuration));
        wheelTransform.SetSiblingIndex(1);
    }

    public void SkipAnimation()
    {
        StopAllCoroutines();
        wheelTransform.rotation = Quaternion.Euler(0f, 0f, endRotation);
        ShowResults();
    }

    private void UpdateWheel()
    {
        int numDefaultOptions = 1;
        for (int i = 0; i < options.Count; i++)
        {
            options[i].SetPosition(options.Count, i);
            if (options[i].Text.Contains("Option")) options[i].Text = "Option " + numDefaultOptions++;
            optionList.AddOrUpdateOption(options[i]);
        }
        addButton.interactable = options.Count < 12;
        spinButton.interactable = options.Count > 0;
        wheelTransform.SetSiblingIndex(1);
        resultPanel.SetActive(false);
    }

    private void ShowResults()
    {
        addButton.interactable = true;
        skipButton.gameObject.SetActive(false);
        foreach (Button button in optionList.GetComponentsInChildren<Button>()) button.interactable = true;
        float maxResult = -1;
        float currentResult;
        WheelOption bestOption = null;
        foreach(WheelOption option in options)
        {
            currentResult = Vector3.Dot(option.GetResultDirection(), Vector3.up);
            if (currentResult > maxResult)
            {
                bestOption = option;
                maxResult = currentResult;
            }
        }
        if (removeWinnerToggle.isOn && options.Count > 1) optionList.RemoveOption(bestOption);
        resultPanel.GetComponentInChildren<TextMeshProUGUI>().text = bestOption.Text;
        resultPanel.SetActive(true);
        wheelTransform.SetSiblingIndex(0);
    }

    private IEnumerator Spin(float totalDegrees, float time)
    {
        float elapsed = 0f;
        float startRotation = wheelTransform.eulerAngles.z;
        endRotation = startRotation + totalDegrees;

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / time;
            float easedT = 1f - Mathf.Pow(1f - t, 3);
            float currentZ = Mathf.Lerp(startRotation, endRotation, easedT);
            wheelTransform.rotation = Quaternion.Euler(0f, 0f, currentZ);
            yield return null;
        }
        ShowResults();
    }
}
