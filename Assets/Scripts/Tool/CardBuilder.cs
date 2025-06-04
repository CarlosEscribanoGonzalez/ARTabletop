using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class CardBuilder : MonoBehaviour
{
    [SerializeField] private CardPreview preview;
    [SerializeField] private CardPreview defaultPreview;
    [SerializeField] private Sprite defaultImage;
    [SerializeField] private TMP_Dropdown totalCardsDropdown;
    [SerializeField] private TextMeshProUGUI indexText;
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private TMP_InputField textInputField;
    [SerializeField] private TMP_InputField sizeInputField;
    public List<CardInfo> Cards { get; set; } = new();
    public Sprite DefaultImage => defaultImage;
    private int index;

    private void Awake()
    {
        if(totalCardsDropdown != null) 
            for (int i = 0; i < totalCardsDropdown.value + 1; i++) Cards.Add(new CardInfo());
    }

    public void UpdateIndex(int dir)
    {
        index += dir;
        if (index >= Cards.Count) index = 0;
        else if (index < 0) index = Cards.Count - 1;
        preview.UpdateValues(Cards[index]);
        indexText.text = (index + 1).ToString();
        textInputField.SetTextWithoutNotify(Cards[index].text);
        sizeInputField.SetTextWithoutNotify(Cards[index].sizeMult.ToString());
    }

    public void PickImage(bool isDefaultImage)
    {
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                Texture2D texture = NativeGallery.LoadImageAtPath(path, 1024, false);
                if (texture == null)
                {
                    Debug.LogError("No se pudo cargar la imagen.");
                    return;
                }
                texture.name = Path.GetFileName(path);
                Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                if (isDefaultImage)
                {
                    defaultImage = newSprite;
                    defaultPreview.SetImage(defaultImage);
                }
                else Cards[index].sprite = newSprite;
                preview?.UpdateValues(Cards[index]);
            }
        }, "Select an image");
    }

    public void UpdateText(string text)
    {
        Cards[index].text = text;
        preview.UpdateValues(Cards[index]);
    }

    public void UpdateSize(string size)
    {
        Cards[index].sizeMult = float.Parse(size);
        preview.UpdateValues(Cards[index]);
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

    public void DownloadInfo()
    {
        ContentDownloader.DownloadSprite(defaultImage);
        foreach(var c in Cards)
        {
            ContentDownloader.DownloadSprite(c.sprite);
        }
    }
}
