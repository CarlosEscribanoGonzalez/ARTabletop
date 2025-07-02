using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class DropdownFixer : MonoBehaviour
{
    private Transform list;
    private TMP_Dropdown dropdown;
    private EventTrigger trigger;

    private void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        if(dropdown != null)
        {
            trigger = dropdown.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => { OnDeployed((PointerEventData)data); });
            trigger.triggers.Add(entry);
        }
    }

    private void OnDeployed(PointerEventData _)
    {
        list = transform.Find("Dropdown List");
        if (list.transform.localPosition.y <= 0) return;
        RectTransform rt = list.GetComponent<RectTransform>();
        float prevY = rt.anchoredPosition.y;
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.anchoredPosition = new Vector2(0, -prevY);
    }
}
