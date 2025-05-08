using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualKeyboardFixer : MonoBehaviour
{
    private ScrollRect scrollRect;
    private TMP_InputField inputField;

    void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        scrollRect = GetComponentInParent<ScrollRect>();
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
        ExecuteEvents.Execute(inputField.gameObject, data, ExecuteEvents.pointerDownHandler);
    }
}
