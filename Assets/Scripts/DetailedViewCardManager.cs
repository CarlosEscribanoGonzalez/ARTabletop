using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using System.Collections;

public class DetailedViewCardManager : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private float interpolationSpeed;
    [SerializeField] private float swipThreshold = 4;
    [SerializeField] private Vector2 maskVerticalPadding = new Vector2(0.2f, 0.066f);
    private RectTransform mask;
    private Volume dof;
    private ScreenOrientation prevOrientation;
    private GameObject rngSection;
    public bool IsInDetailedView => mask.gameObject.activeSelf;

    private void Awake()
    {
        mask = GetComponentInChildren<RectMask2D>(true).rectTransform;
        dof = GetComponentInChildren<Volume>(true);
        rngSection = FindFirstObjectByType<RNGSection>().gameObject;
    }

    private void Update()
    {
        if (!IsInDetailedView) return;
#if UNITY_EDITOR
        AdjustSize();
#endif
        if (Screen.orientation != prevOrientation)
        {
            AdjustSize();
            prevOrientation = Screen.orientation;
        }
        float scale = Mathf.Clamp01(1 - Mathf.Abs((mask.anchoredPosition.y - image.rectTransform.anchoredPosition.y)/Screen.height) + 0.1f);
        image.rectTransform.localScale = new Vector3(scale, scale, scale);
    }

    public void SetDetailedInfo(Sprite sprite, string text)
    {
        ToggleView(true);
        image.sprite = sprite;
        textMesh.text = text;
        AdjustSize();
    }

    public void OnBeginDrag(BaseEventData _)
    {
        image.transform.parent = mask.parent;
    }

    public void OnDrag(BaseEventData data)
    {
        PointerEventData pointerData = (PointerEventData)data;
        image.rectTransform.anchoredPosition += new Vector2(0, pointerData.delta.y);
    }

    public void OnEndDrag(BaseEventData _)
    {
        int heightTarget;
        float offset = image.rectTransform.anchoredPosition.y - mask.anchoredPosition.y;
        if (offset > Screen.height / 2 - Screen.height / swipThreshold) heightTarget = Screen.height;
        else if (offset < -(Screen.height / 2 - Screen.height / swipThreshold)) heightTarget = -Screen.height;
        else heightTarget = (int)mask.anchoredPosition.y;
        StartCoroutine(MoveToHeight(heightTarget));
    }

    private void ToggleView(bool activate)
    {
        mask.gameObject.SetActive(activate);
        dof.enabled = activate;
        rngSection.SetActive(!activate);
    }

    private void AdjustSize()
    {
        mask.offsetMin = new Vector2(mask.offsetMin.x, Screen.height * maskVerticalPadding[0]);
        mask.offsetMax = new Vector2(mask.offsetMax.x, -Screen.height * maskVerticalPadding[1]);
        float ratio = (float) image.sprite.texture.width / image.sprite.texture.height;
        if (image.sprite.texture.width < image.sprite.texture.height)
            image.rectTransform.sizeDelta = new Vector2(mask.rect.size.x, mask.rect.size.x / ratio);
        else image.rectTransform.sizeDelta = new Vector2(mask.rect.size.y * ratio, mask.rect.size.y);
        while (image.rectTransform.sizeDelta.x > mask.rect.size.x || image.rectTransform.sizeDelta.y > mask.rect.size.y)
        {
            image.rectTransform.sizeDelta /= 1.05f;
        }
    }

    IEnumerator MoveToHeight(int desiredHeight)
    {
        float currentY;
        while(Mathf.Abs(image.rectTransform.anchoredPosition.y - desiredHeight) > 5)
        {
            currentY = Mathf.Lerp(image.rectTransform.anchoredPosition.y, desiredHeight, interpolationSpeed * Time.deltaTime);
            image.rectTransform.anchoredPosition = new Vector2(0, currentY);
            yield return null;
        }
        if (desiredHeight != mask.anchoredPosition.y) ToggleView(false);
        image.transform.parent = mask;
        image.rectTransform.anchoredPosition = Vector2.zero;
    }
}
