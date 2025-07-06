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

    protected override void Awake()
    {
        base.Awake();
        if (ToolManager.GameToEdit != null && ToolManager.GameToEdit.specialCardsInfo.Count > 0) return;
        GenerateSpecialCard();
    }

    public override void SetInitInfo(GameInfo gameInfo)
    {
        if (gameInfo.specialCardsInfo.Count == 0) return;
        for(int i = gameInfo.specialCardsInfo.Count - 1; i >= 0; i--) //Si no los da al revés
        {
            var scardInfo = gameInfo.specialCardsInfo[i];
            CardBuilder newCardBuilder = GenerateSpecialCard(scardInfo.name);
            newCardBuilder.ConfigureBuilder(scardInfo.cardsInfo, scardInfo.defaultImage);
            newCardBuilder.gameObject.SetActive(false);
        }
        index = 0;
        UpdateIndex(0);
        UpdateUI();
    }

    public void AddSpecialCard() //Llamado desde el botón
    {
        GenerateSpecialCard(); //Devuelve un CardBuilder, así que se tiene que dividir en dos funciones para que se llame bien desde el botón
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
        UpdateUI();
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
        if (name == string.Empty)
        {
            Names[index] = "DCards " + Content.Count;
            for (int i = 0; i < Names.Count; i++)
            {
                if (Names[i].StartsWith("DCards "))
                {
                    if (i + 1 < 10) Names[i] = "DCards 0" + (i + 1);
                    else Names[i] = "DCards " + (i + 1);
                }
            }
            nameInputField.SetTextWithoutNotify(Names[index]);
        }
        else Names[index] = name;
    }

    public override void DownloadInfo()
    {
        foreach (var cardBuilder in Content) cardBuilder.DownloadInfo();
    }

    public override CardBuilder GetDefaultContent()
    {
        return new CardBuilder();
    }

    private void UpdateUI()
    {
        removeButton.interactable = Content.Count != 1;
        addButton.interactable = Content.Count < maxSCards;
        contentDropdown.SetValueWithoutNotify(Content.Count - 1);
        CheckArrowsVisibility();
        nameInputField.SetTextWithoutNotify(Names[index]);
    }

    private CardBuilder GenerateSpecialCard(string name = "")
    {
        GameObject builder = Instantiate(cardBuilderPrefab, cardBuildersParent);
        Content.Add(builder.GetComponent<CardBuilder>());
        Names.Add(name != string.Empty ? name : "DCards " + Content.Count);
        for (int i = 0; i < Names.Count; i++)
        {
            if (Names[i].StartsWith("DCards "))
            {
                if (i + 1 < 10) Names[i] = "DCards 0" + (i + 1);
                else Names[i] = "DCards " + (i + 1);
            }
        }
        Content[index].gameObject.SetActive(false);
        index = Content.Count - 1;
        UpdateIndex(0);
        UpdateUI();
        return builder.GetComponent<CardBuilder>();
    }
}
