using UnityEngine;
using TMPro;

public class InfoButton : MonoBehaviour
{
    [TextArea][SerializeField] private string info; //Información a desplegar 
    [SerializeField] private GameObject infoPanel;
    private string title; //Título de la información
    private CanvasGroup infoPanelCanvasGroup; //Canvas group del info panel. Sólo hay un info panel y es transparente hasta que haga falta mostrarlo
    private TextMeshProUGUI[] infoPanelTexts; //Textos del info panel (0 -> título; 1 -> info; no tiene más)

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        title = GetComponentInChildren<TextMeshProUGUI>(true).text; //Lo que pone en el botón es el título de la info a mostrar
        infoPanelCanvasGroup = infoPanel.GetComponentInParent<CanvasGroup>();
        infoPanelTexts = infoPanel.GetComponentsInChildren<TextMeshProUGUI>();
    }

    public void DisplayInfo() //El infopanel se muestra y se vuelve interactuable
    {
        //Su información se configura para mostrar la info requerida
        infoPanelTexts[0].text = title;
        infoPanelTexts[1].text = info;
        infoPanelCanvasGroup.interactable = true;
        infoPanelCanvasGroup.blocksRaycasts = true;
        infoPanelCanvasGroup.alpha = 1;
    }
}
