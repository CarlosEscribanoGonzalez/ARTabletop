using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class CardPreview : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI cardText;
    [SerializeField] private bool adjustSize = true;
    private RectTransform rectTransform;
    private CardBuilder cardBuilder;
    public UnityEvent OnImageSet; 

    void Start()
    {
        this.rectTransform = GetComponent<RectTransform>();
        cardBuilder = GetComponentInParent<CardBuilder>();
        if(adjustSize) AdjustSize();
    }

    public void SetImage(Sprite sprite)
    {
        image.sprite = sprite;
        if(adjustSize) AdjustSize();
        OnImageSet?.Invoke();
    }

    public void UpdateValues(CardInfo info)
    {
        SetImage(info.sprite ?? cardBuilder.DefaultImage);
        cardText.text = info.text;
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
