using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class CardPreview : APreview<CardInfo>
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI cardText;
    [SerializeField] private bool adjustSize = true;
    [SerializeField] private RectTransform rectTransform;
    private CardBuilder cardBuilder;
    public UnityEvent OnImageSet; 

    void Awake()
    {
        cardBuilder = GetComponentInParent<CardBuilder>();
        if(adjustSize) AdjustSize();
    }

    public void SetImage(Sprite sprite)
    {
        image.sprite = sprite;
        if(adjustSize) AdjustSize();
        OnImageSet?.Invoke();
    }

    public override void UpdateValues(CardInfo info)
    {
        if (cardBuilder == null) cardBuilder = GetComponentInParent<CardBuilder>(true);
        SetImage(info.sprite ?? cardBuilder.DefaultImage);
        cardText.text = info.text;
    }

    protected override void AdjustSize()
    {
        image.rectTransform.sizeDelta = ContentScaler.ScaleImage(image.sprite.texture, rectTransform.rect);
    }
}
