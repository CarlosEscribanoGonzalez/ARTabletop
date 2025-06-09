using UnityEngine;
using TMPro;
using System.IO;
using System.Globalization;

public class CardBuilder : ABuilder<CardInfo>
{
    [SerializeField] private CardPreview defaultPreview;
    [SerializeField] private Sprite defaultImage;
    [SerializeField] private TMP_InputField textInputField;
    [SerializeField] private TMP_InputField sizeInputField;
    public Sprite DefaultImage => defaultImage;

    private void Awake()
    {
        if(contentDropdown != null) 
            for (int i = 0; i < contentDropdown.value + 1; i++) Content.Add(new CardInfo());
    }

    public override void UpdateIndex(int dir)
    {
        base.UpdateIndex(dir);
        textInputField.SetTextWithoutNotify(Content[index].text);
        sizeInputField.SetTextWithoutNotify(Content[index].sizeMult.ToString());
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
                else Content[index].sprite = newSprite;
                preview?.UpdateValues(Content[index]);
            }
        }, "Select an image");
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
            ContentDownloader.DownloadSprite(c.sprite);
        }
    }
}
