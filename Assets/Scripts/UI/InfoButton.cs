using UnityEngine;
using TMPro;

public class InfoButton : MonoBehaviour
{
    [TextArea][SerializeField] private string info; //Informaci�n a desplegar 
    private string title; //T�tulo de la informaci�n
    private CanvasGroup infoPanelCanvasGroup; //Canvas group del info panel. S�lo hay un info panel y es transparente hasta que haga falta mostrarlo
    private TextMeshProUGUI[] infoPanelTexts; //Textos del info panel (0 -> t�tulo; 1 -> info; no tiene m�s)

    private void Awake()
    {
        title = GetComponentInChildren<TextMeshProUGUI>().text; //Lo que pone en el bot�n es el t�tulo de la info a mostrar
        infoPanelCanvasGroup = GameObject.FindWithTag("InfoPanel").GetComponentInParent<CanvasGroup>();
        infoPanelTexts = GameObject.FindWithTag("InfoPanel").GetComponentsInChildren<TextMeshProUGUI>();
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
