using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Collections;

public class CardBuilder : ABuilder<CardInfo>
{
    [SerializeField] private CardPreview defaultPreview;
    [SerializeField] private Sprite defaultImage;
    [SerializeField] private TMP_InputField textInputField;
    [SerializeField] private TMP_InputField sizeInputField;
    public Sprite DefaultImage => defaultImage;

    private void OnEnable()
    {
        if (confirmationPanel == null) CreateConfirmationPanel(); //Si es el builder de un SCard no tiene confirmation panel para él
        if (Content.Count > 0) return;
        if (contentDropdown != null)
        {            
            for (int i = 0; i < initLength; i++)
            {
                Content.Add(new CardInfo());
            }
            contentDropdown.SetValueWithoutNotify(initLength - 1);
        }
    }

    public override void SetInitInfo(GameInfo gameInfo)
    {
        ConfigureBuilder(gameInfo.cardsInfo, gameInfo.defaultSprite); //Para no escribir dos veces el mismo código
        CheckContentButtons();
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
    }

    public void LoadImage(bool isDefaultImage)
    {
        LoadingScreenManager.ToggleLoadingScreen(true, false, "Importing image...");
        ContentLoader.Instance.PickImage((path) =>
        {
            try
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
                }
                LoadingScreenManager.ToggleLoadingScreen(false);
            }
            catch
            {
                LoadingScreenManager.ToggleLoadingScreen(false);
            }
        });
    }

    public void ResetCard()
    {
        Content[index].sprite = null;
        Content[index].text = string.Empty;
        Content[index].sizeMult = 1;
        textInputField.SetTextWithoutNotify(string.Empty);
        sizeInputField.SetTextWithoutNotify("1");
        preview.UpdateValues(Content[index]);
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

    public void TextEditingCooldown()
    {
        StartCoroutine(TextCooldownCoroutine());
    }

    private void CreateConfirmationPanel()
    {
        confirmationPanel = Instantiate(GameObject.FindWithTag("CardConfirmationPanel")).GetComponent<Canvas>();
        Button confirmButton = confirmationPanel.transform.Find("Background/YesButton").GetComponent<Button>();
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() => confirmationPanel.enabled = false);
        confirmButton.onClick.AddListener(() => ConfirmChange());
    }

    IEnumerator TextCooldownCoroutine()
    {
        textInputField.interactable = false;
        yield return new WaitForSeconds(0.3f);
        textInputField.interactable = true;
    }
}
