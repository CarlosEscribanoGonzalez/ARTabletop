using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Globalization;
using System.Collections.Generic;

public class CardBuilder : ABuilder<CardInfo>
{
    [SerializeField] private CardPreview defaultPreview;
    [SerializeField] private Sprite defaultImage;
    [SerializeField] private TMP_InputField textInputField;
    [SerializeField] private TMP_InputField sizeInputField;
    [SerializeField] private Button setAsDefaultButton;
    public Sprite DefaultImage => defaultImage;

    private void OnEnable()
    {
        if (Content.Count > 0) return;
        if (contentDropdown != null)
        {            
            for (int i = 0; i < maxLength; i++)
            {
                if(i < initLength) Content.Add(new CardInfo());
            }
            contentDropdown.SetValueWithoutNotify(initLength - 1);
            setAsDefaultButton.interactable = false;
        }
    }

    public override void SetInitInfo(GameInfo gameInfo)
    {
        ConfigureBuilder(gameInfo.cardsInfo, gameInfo.defaultSprite); //Para no escribir dos veces el mismo código
    }

    public void ConfigureBuilder(List<CardInfo> cardsInfo, Sprite defaultImage) //Hace falta esta función para que scards builder pueda configurar sus cartas
    {
        if (cardsInfo.Count > 0)
        {
            Content = cardsInfo;
            contentDropdown.SetValueWithoutNotify(Content.Count - 1);
        }
        this.defaultImage = defaultImage;
        defaultPreview.SetImage(defaultImage);
        UpdateIndex(0);
    }

    public override void UpdateIndex(int dir)
    {
        if (Content.Count == 0) return;
        base.UpdateIndex(dir);
        textInputField.SetTextWithoutNotify(Content[index].text);
        sizeInputField.SetTextWithoutNotify(Content[index].sizeMult.ToString());
        setAsDefaultButton.interactable = Content[index].sprite != null && 
                            !Content[index].sprite.texture.name.Equals(defaultImage.texture.name);
    }

    public void PickImage(bool isDefaultImage)
    {
        LoadingScreenManager.ToggleLoadingScreen(true);
        ContentLoader.Instance.PickImage((path) =>
        {
            if (path != null)
            {
                Texture2D texture = NativeGallery.LoadImageAtPath(path, 1024, false);
                if (texture == null)
                {
                    FeedbackManager.Instance.DisplayMessage("Unexpected error: image couldn't be loaded. Please, try again.");
                    Debug.LogError("No se pudo cargar la imagen.");
                    return;
                }
                if (Path.GetFileName(path).EndsWith(".png") || Path.GetFileName(path).EndsWith(".jpg"))
                    texture.name = Path.GetFileName(path);
                else texture.name = Path.GetFileNameWithoutExtension(path) + ".jpg";
                Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                if (isDefaultImage)
                {
                    defaultImage = newSprite;
                    defaultPreview.SetImage(defaultImage);
                }
                else Content[index].sprite = newSprite;
                preview?.UpdateValues(Content[index]);
                if(Content.Count > 0) setAsDefaultButton.interactable = Content[index].sprite != null && 
                                        !Content[index].sprite.texture.name.Equals(defaultImage.texture.name);
            }
            LoadingScreenManager.ToggleLoadingScreen(false);
        });
    }

    public void SetAsDefault()
    {
        Content[index].sprite = null;
        preview.UpdateValues(Content[index]);
        setAsDefaultButton.interactable = false;
    }

    public void UpdateText(string text)
    {
        Content[index].text = text;
        preview.UpdateValues(Content[index]);
    }

    public void UpdateSize()
    {
        if (!string.IsNullOrEmpty(sizeInputField.text))
            Content[index].sizeMult = float.Parse(sizeInputField.text, CultureInfo.InvariantCulture);
        else Content[index].sizeMult = 1;
    }

    public override CardInfo GetDefaultContent()
    {
        CardInfo defaultContent = new();
        defaultContent.sprite = defaultImage;
        return defaultContent;
    }

    public override void DownloadInfo()
    {
        ContentDownloader.DownloadSprite(defaultImage);
        foreach(var c in Content)
        {
            if(c.sprite != null && !c.sprite.texture.name.Equals(defaultImage.texture.name)) 
                ContentDownloader.DownloadSprite(c.sprite);
        }
    }
}
