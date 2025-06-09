using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SpecialCardBuilder : ABuilder<CardBuilder>
{
    [SerializeField] private int maxSCards = 5;
    [SerializeField] private Transform cardBuildersParent;
    [SerializeField] private GameObject cardBuilderPrefab;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button addButton;
    [SerializeField] private Button removeButton;
    public List<string> Names { get; } = new();

    private void Awake()
    {
        AddSpecialCard();
    }

    public void AddSpecialCard()
    {
        GameObject builder = Instantiate(cardBuilderPrefab, cardBuildersParent);
        Content.Add(builder.GetComponent<CardBuilder>());
        Names.Add("SCards " + Content.Count);
        for(int i = 0; i < Names.Count; i++) if (Names[i].StartsWith("SCards ")) Names[i] = "SCards 0" + (i + 1);
        Content[index].gameObject.SetActive(false);
        index = Content.Count - 1;
        UpdateIndex(0);
        removeButton.interactable = Content.Count != 1;
        addButton.interactable = Content.Count < maxSCards;
        contentDropdown.SetValueWithoutNotify(Content.Count - 1);
    }

    public void RemoveSpecialCard()
    {
        GameObject objToRemove = Content[index].gameObject;
        Content.RemoveAt(index);
        Names.RemoveAt(index);
        if (index >= Content.Count) index = Content.Count - 1;
        UpdateIndex(0);
        Destroy(objToRemove);
        Content[index].UpdateIndex(0);
        removeButton.interactable = Content.Count != 1;
        addButton.interactable = Content.Count < maxSCards;
        contentDropdown.SetValueWithoutNotify(Content.Count - 1);
    }

    public override void UpdateIndex(int dir)
    {
        Content[index].gameObject.SetActive(false);
        base.UpdateIndex(dir);
        Content[index].gameObject.SetActive(true);
        Content[index].Index = 0;
        Content[index].UpdateIndex(0);
        nameInputField.SetTextWithoutNotify(Names[index]);
    }

    public void SetName(string name)
    {
        while (name.EndsWith(" ")) name.Remove(name.Length - 2);
        Names[index] = name;
    }

    public override void DownloadInfo()
    {
        foreach (var cardBuilder in Content) cardBuilder.DownloadInfo();
    }

    public override CardBuilder GetDefaultContent()
    {
        return new CardBuilder();
    }
}
