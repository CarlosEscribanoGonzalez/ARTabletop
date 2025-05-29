using UnityEngine;
using TMPro;

public class InfoButton : MonoBehaviour
{
    [TextArea][SerializeField] private string info;
    private string title;
    private CanvasGroup infoPanelCanvasGroup;
    private TextMeshProUGUI[] infoPanelTexts;

    private void Awake()
    {
        title = GetComponentInChildren<TextMeshProUGUI>().text; //Lo que pone en el botón
        infoPanelCanvasGroup = GameObject.FindWithTag("InfoPanel").GetComponent<CanvasGroup>();
        infoPanelTexts = infoPanelCanvasGroup.GetComponentsInChildren<TextMeshProUGUI>();
    }

    public void DisplayInfo()
    {
        infoPanelTexts[0].text = title;
        infoPanelTexts[1].text = info;
        infoPanelCanvasGroup.interactable = true;
        infoPanelCanvasGroup.blocksRaycasts = true;
        infoPanelCanvasGroup.alpha = 1;
    }
}
