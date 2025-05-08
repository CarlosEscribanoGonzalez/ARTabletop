using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualKeyboardFixer : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    private ScrollRect scrollRect;
    private CanvasGroup canvasGroup;

    void Start()
    {
        scrollRect = GetComponentInParent<ScrollRect>();
        canvasGroup = GetComponent<CanvasGroup>();
        inputField.onDeselect.AddListener((_) => OnFieldDeselected());
    }

    public void BeginScroll(BaseEventData data)
    {
        PointerEventData pointerData = (PointerEventData)data;
        scrollRect.OnBeginDrag(pointerData);
    }

    public void Scroll(BaseEventData data)
    {
        PointerEventData pointerData = (PointerEventData)data;
        scrollRect.OnDrag(pointerData);
    }

    public void EndScroll(BaseEventData data)
    {
        PointerEventData pointerData = (PointerEventData)data;
        scrollRect.OnEndDrag(pointerData);
    }

    public void OpenKeyBoard(BaseEventData data)
    {
        inputField.shouldHideMobileInput = (Screen.orientation == ScreenOrientation.Portrait
                                                || Screen.orientation == ScreenOrientation.PortraitUpsideDown);
        ToggleScreenRotation(false);
        ExecuteEvents.Execute(inputField.gameObject, data, ExecuteEvents.pointerDownHandler);
        canvasGroup.blocksRaycasts = false;
    }

    private void OnFieldDeselected()
    {
        canvasGroup.blocksRaycasts = true;
        ToggleScreenRotation(true);
    }

    private void ToggleScreenRotation(bool allow)
    {
        Screen.autorotateToLandscapeLeft = allow;
        Screen.autorotateToLandscapeRight = allow;
        Screen.autorotateToPortrait = allow;
        Screen.autorotateToPortraitUpsideDown = allow;
    }
}
