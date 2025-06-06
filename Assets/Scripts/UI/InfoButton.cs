using UnityEngine;
using TMPro;

public class InfoButton : MonoBehaviour
{
    [TextArea][SerializeField] private string info; //Informaci�n a desplegar 
    [SerializeField] private GameObject infoPanel;
    private string title; //T�tulo de la informaci�n
    private CanvasGroup infoPanelCanvasGroup; //Canvas group del info panel. S�lo hay un info panel y es transparente hasta que haga falta mostrarlo
    private TextMeshProUGUI[] infoPanelTexts; //Textos del info panel (0 -> t�tulo; 1 -> info; no tiene m�s)

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        title = GetComponentInChildren<TextMeshProUGUI>(true).text; //Lo que pone en el bot�n es el t�tulo de la info a mostrar
        infoPanelCanvasGroup = infoPanel.GetComponentInParent<CanvasGroup>();
        infoPanelTexts = infoPanel.GetComponentsInChildren<TextMeshProUGUI>();
    }

    public void DisplayInfo() //El infopanel se muestra y se vuelve interactuable
    {
        //Su informaci�n se configura para mostrar la info requerida
        infoPanelTexts[0].text = title;
        infoPanelTexts[1].text = info;
        infoPanelCanvasGroup.interactable = true;
        infoPanelCanvasGroup.blocksRaycasts = true;
        infoPanelCanvasGroup.alpha = 1;
    }
}
