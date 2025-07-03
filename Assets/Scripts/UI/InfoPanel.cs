using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleTextMesh;
    [SerializeField] private TextMeshProUGUI infoTextMesh;
    [SerializeField] private Button[] nextButtons;
    [SerializeField] private Button[] prevButtons;
    private string[] info;
    private CanvasGroup canvasGroup;
    private int index = 0;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void DisplayInfo(string title, string[] pages)
    {
        titleTextMesh.text = title;
        info = pages;
        index = 0;
        TurnPage(0);
        foreach(var prev in prevButtons) prev.interactable = false;
        foreach(var next in nextButtons) next.interactable = true;
        ToggleVisibility(true);
    }

    public void TurnPage(int dir)
    {
        index += dir;
        infoTextMesh.text = info[index];
        foreach (var prev in prevButtons) prev.interactable = index > 0;
        foreach (var next in nextButtons) next.interactable = index < info.Length - 1;
    }

    public void Close()
    {
        ToggleVisibility(false);
    }

    private void ToggleVisibility(bool visible)
    {
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
        canvasGroup.alpha = visible ? 1 : 0;
        foreach (var next in nextButtons) next.gameObject.SetActive(info.Length > 1);
        foreach (var prev in prevButtons) prev.gameObject.SetActive(info.Length > 1);
    }
}
