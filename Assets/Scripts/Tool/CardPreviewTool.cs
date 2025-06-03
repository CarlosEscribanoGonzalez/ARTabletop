using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.IO;

public class CardPreviewTool : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI cardText;
    [SerializeField] private TMP_InputField textInputField;
    [SerializeField] private TMP_InputField sizeInputField;
    [SerializeField] private bool isDefaultPreview = false;
    private RectTransform rectTransform;
    private CardBuilder cardBuilder;
    public UnityEvent onImageSet; 

    void Start()
    {
        this.rectTransform = GetComponent<RectTransform>();
        cardBuilder = GetComponentInParent<CardBuilder>();
        AdjustSize();
    }

    public void PickImage()
    {
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                Texture2D texture = NativeGallery.LoadImageAtPath(path, 1024);
                if (texture == null)
                {
                    Debug.LogError("No se pudo cargar la imagen.");
                    return;
                }
                texture.name = Path.GetFileNameWithoutExtension(path);
                SetImage(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)));
            }
        }, "Select an image");
    }

    public void SetImage(Sprite sprite)
    {
        image.sprite = sprite;
        AdjustSize();
        onImageSet?.Invoke();
        if (cardBuilder == null) return;
        if (!isDefaultPreview) cardBuilder.UpdateCurrentImage(sprite);
        else cardBuilder.DefaultImage = sprite;
    }

    public void SetText(string text)
    {
        cardText.text = text;
        cardBuilder.UpdateCurrentText(text);
    }

    public void SetSize(string size)
    {
        cardBuilder.UpdateCurrentSize(float.Parse(size));
    }

    public void UpdateValues(CardInfo info)
    {
        image.sprite = info.sprite ?? cardBuilder.DefaultImage;
        AdjustSize();
        cardText.text = info.text;
        textInputField.SetTextWithoutNotify(info.text);
        sizeInputField.SetTextWithoutNotify(info.sizeMult.ToString());
    }

    private void AdjustSize()
    {
        float ratio = (float)image.sprite.texture.width / image.sprite.texture.height; //Se obtiene el aspect ratio de la imagen
        //La imagen se escala teniendo en cuenta cuál de sus dos dimensiones, width o height, es más grande, manteniendo el ratio
        if (image.sprite.texture.width < image.sprite.texture.height) //Si es más alta se ajusta a lo ancho
            image.rectTransform.sizeDelta = new Vector2(rectTransform.rect.size.x, rectTransform.rect.size.x / ratio);
        else image.rectTransform.sizeDelta = new Vector2(rectTransform.rect.size.y * ratio, rectTransform.rect.size.y); //Si no, a lo alto
        //Si la imagen aun así se sale del mask se hace progresivamente más pequeña hasta que no lo haga
        while (image.rectTransform.sizeDelta.x > rectTransform.rect.size.x || image.rectTransform.sizeDelta.y > rectTransform.rect.size.y)
            image.rectTransform.sizeDelta *= 0.95f;
    }
}
