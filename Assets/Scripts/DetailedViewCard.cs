using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class DetailedViewCard : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private float interpolationSpeed;
    [SerializeField] private float swipThreshold = 4;
    [SerializeField] private Vector2 maskVerticalPadding = new Vector2(0.2f, 0.066f);
    [SerializeField] private Sprite defaultSprite;
    private RectTransform rectTransform;
    private ScreenOrientation prevOrientation;
    private ScrollRect scrollRect;
    private DetailedViewCardManager manager;
    private bool isDragging = false;
    public bool IsDetailedCard => scrollRect == null;
    public CardInfo Info { get; private set; } = null;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        scrollRect = GetComponentInParent<ScrollRect>();
        manager = FindFirstObjectByType<DetailedViewCardManager>();
        AdjustSize();
    }

    private void Update()
    {
        if (!IsDetailedCard) return;
        if (Screen.orientation != prevOrientation)
        {
            AdjustSize();
            prevOrientation = Screen.orientation;
        }
        float scale = Mathf.Clamp01(1 - Mathf.Abs((rectTransform.anchoredPosition.y - image.rectTransform.anchoredPosition.y) / Screen.height*2) + 0.1f);
        image.rectTransform.localScale = new Vector3(scale, scale, scale);
    }

    public void SetInfo(CardInfo info)
    {
        image.sprite = info.sprite ?? defaultSprite;
        textMesh.text = info.text;
        Info = info;
    }

    public void OnBeginDrag(BaseEventData data)
    {
        if (IsDetailedCard) image.transform.parent = rectTransform.parent;
        else scrollRect.OnBeginDrag((PointerEventData) data);
        isDragging = true;
    }

    public void OnDrag(BaseEventData data)
    {
        PointerEventData pointerData = (PointerEventData)data;
        if(IsDetailedCard) image.rectTransform.anchoredPosition += new Vector2(0, pointerData.delta.y);
        else scrollRect.OnDrag(pointerData);
    }

    public void OnEndDrag(BaseEventData data)
    {
        if (IsDetailedCard)
        {
            float heightTarget;
            MovementType action = MovementType.None;
            float offset = image.rectTransform.anchoredPosition.y - rectTransform.anchoredPosition.y;
            if (offset > Screen.height / 2 - Screen.height / swipThreshold)
            {
                heightTarget = Screen.height * 0.75f;
                action = MovementType.Remove;
            }
            else if (offset < -(Screen.height / 2 - Screen.height / swipThreshold))
            {
                heightTarget = -Screen.height * 0.5f;
                action = MovementType.Add;
            }
            else heightTarget = (int)rectTransform.anchoredPosition.y;
            StartCoroutine(MoveToHeight(heightTarget, action));
        }
        else scrollRect.OnEndDrag((PointerEventData)data);
        isDragging = false;
    }

    public void OnClick(BaseEventData _)
    {
        if(!IsDetailedCard && !isDragging) manager.SetDetailedInfo(Info);
    }

    private void AdjustSize()
    {
        if (IsDetailedCard)
        {
            rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, Screen.height * maskVerticalPadding[0]);
            rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, -Screen.height * maskVerticalPadding[1]);
        }
        float ratio = (float)image.sprite.texture.width / image.sprite.texture.height;
        if (image.sprite.texture.width < image.sprite.texture.height)
            image.rectTransform.sizeDelta = new Vector2(rectTransform.rect.size.x, rectTransform.rect.size.x / ratio);
        else image.rectTransform.sizeDelta = new Vector2(rectTransform.rect.size.y * ratio, rectTransform.rect.size.y);
        while (image.rectTransform.sizeDelta.x > rectTransform.rect.size.x || image.rectTransform.sizeDelta.y > rectTransform.rect.size.y)
            image.rectTransform.sizeDelta *= 0.95f;
    }

    IEnumerator MoveToHeight(float desiredHeight, MovementType action)
    {
        float currentY;
        float speed = action == MovementType.None ? interpolationSpeed : interpolationSpeed * 2;
        while (Mathf.Abs(image.rectTransform.anchoredPosition.y - desiredHeight) > 5)
        {
            currentY = Mathf.MoveTowards(image.rectTransform.anchoredPosition.y, desiredHeight, speed * Time.deltaTime);
            image.rectTransform.anchoredPosition = new Vector2(0, currentY);
            yield return null;
        }
        image.transform.parent = rectTransform;
        image.rectTransform.anchoredPosition = Vector2.zero;
        if (action != MovementType.None) manager.ToggleView(false);
        if (action == MovementType.Add) manager.KeepCard(Info);
        else if (action == MovementType.Remove) manager.RemoveCard(Info);
    }
}

public enum MovementType
{
    None,
    Add,
    Remove
}
