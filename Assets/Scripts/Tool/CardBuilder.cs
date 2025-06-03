using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CardBuilder : MonoBehaviour
{
    [SerializeField] private CardPreviewTool preview;
    [SerializeField] private Sprite defaultImage;
    [SerializeField] private TMP_Dropdown totalCardsDropdown;
    [SerializeField] private TextMeshProUGUI indexText;
    [SerializeField] private GameObject confirmationPanel;
    public List<CardInfo> Cards { get; set; } = new();
    public Sprite DefaultImage { get { return defaultImage; } set { defaultImage = value; preview.UpdateValues(Cards[index]); } }
    private int index;

    private void Awake()
    {
        for (int i = 0; i < totalCardsDropdown.value + 1; i++) Cards.Add(new CardInfo());
    }

    public void UpdateIndex(int dir)
    {
        index += dir;
        if (index >= Cards.Count) index = 0;
        else if (index < 0) index = Cards.Count - 1;
        preview.UpdateValues(Cards[index]);
        indexText.text = (index + 1).ToString();
    }

    public void UpdateCurrentImage(Sprite sprite)
    {
        Cards[index].sprite = sprite;
    }

    public void UpdateCurrentText(string text)
    {
        Cards[index].text = text;
    }

    public void UpdateCurrentSize(float size)
    {
        Cards[index].sizeMult = size;
    }

    private int newLength;
    public void UpdateLength(int value)
    {
        newLength = value + 1;
        if (Cards.Count > newLength)
        {
            totalCardsDropdown.SetValueWithoutNotify(Cards.Count - 1); //Se "cancela" el cambio a la espera de la confirmación
            confirmationPanel.SetActive(true);
        }
        else if (Cards.Count < newLength) while (Cards.Count < newLength) Cards.Add(new CardInfo());
    }

    public void ConfirmChange()
    {
        Cards.RemoveRange(newLength, Cards.Count - newLength);
        totalCardsDropdown.SetValueWithoutNotify(newLength - 1);
        if (index >= Cards.Count)
        {
            index = Cards.Count - 1;
            preview.UpdateValues(Cards[index]);
        }
        indexText.text = (index + 1).ToString();
    }
}
