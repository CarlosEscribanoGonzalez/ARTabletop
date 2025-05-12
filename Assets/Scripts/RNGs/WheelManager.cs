using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class WheelManager : MonoBehaviour
{
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
    [SerializeField] private int maxOptions = 12;
    private List<WheelOption> options = new();
    private float endRotation = 0;

    private void Start()
    {
        optionList = FindFirstObjectByType<WheelOptionList>(FindObjectsInactive.Include);
        removeWinnerToggle = GetComponentInChildren<Toggle>(true);
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
        UpdateWheel(false);
    }

    public void SpinWheel()
    {
        addButton.interactable = false;
        foreach (var listOption in optionList.Options)
                listOption.GetComponentInChildren<Button>().interactable = false;
        StartCoroutine(Spin(Random.Range(spinDegreesMinMax[0], spinDegreesMinMax[1]), spinDuration));
        wheelTransform.GetComponent<CanvasGroup>().alpha = 1;
    }

    public void SkipAnimation()
    {
        StopAllCoroutines();
        wheelTransform.rotation = Quaternion.Euler(0f, 0f, endRotation);
        ShowResults();
    }

    public void SetMenuState(bool inResults)
    {
        addButton.interactable = options.Count < maxOptions;
        spinButton.interactable = options.Count > 0;
        wheelTransform.GetComponent<CanvasGroup>().alpha = (inResults ? 0.5f : 1);
        resultPanel.SetActive(inResults);
        skipButton.gameObject.SetActive(false);
        foreach (var listOption in optionList.Options)
            if (options.Count > 1) listOption.GetComponentInChildren<Button>(true).interactable = true;
    }

    private void UpdateWheel(bool updateDefaultNames = true)
    {
        int numDefaultOptions = 1;
        for (int i = 0; i < options.Count; i++)
        {
            options[i].SetPosition(options.Count, i);
            if (options[i].Text.Contains("Option") && updateDefaultNames) options[i].Text = "Option " + numDefaultOptions++;
            optionList.AddOrUpdateOption(options[i]);
        }
        SetMenuState(false);
    }

    private void ShowResults()
    {
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
        SetMenuState(true);
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
