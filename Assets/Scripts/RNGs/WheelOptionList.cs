using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class WheelOptionList : MonoBehaviour
{
    [SerializeField] private GameObject optionPrefab;
    [SerializeField] private Transform contentTransform;
    private Dictionary<WheelOption, GameObject> optionsDictionary = new();
    private WheelManager wheelManager;

    private void Awake()
    {
        wheelManager = FindFirstObjectByType<WheelManager>();
    }

    public void AddOrUpdateOption(WheelOption option)
    {
        if (!optionsDictionary.ContainsKey(option))
        {
            GameObject element = Instantiate(optionPrefab, contentTransform);
            optionsDictionary.Add(option, element);
            element.GetComponentInChildren<TMP_InputField>().onValueChanged.AddListener((text) => option.Text = text);
            element.GetComponentInChildren<Button>().onClick.AddListener(() => RemoveOption(option));
        }
        optionsDictionary[option].GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(option.Text);
    }
    
    public void RemoveOption(WheelOption option)
    {
        wheelManager.RemoveOption(option);
        Destroy(optionsDictionary[option]);
    }
}
