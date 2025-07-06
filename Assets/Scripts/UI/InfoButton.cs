using UnityEngine;
using TMPro;

public class InfoButton : MonoBehaviour
{
    [TextArea][SerializeField] private string[] pages; //Informaci�n a desplegar 
    private string title; //T�tulo de la informaci�n
    private InfoPanel infoPanel;

    private void Awake()
    {
        infoPanel = FindFirstObjectByType<InfoPanel>();
        title = GetComponentInChildren<TextMeshProUGUI>(true).text; //Lo que pone en el bot�n es el t�tulo de la info a mostrar
    }

    public void DisplayInfo() //El infopanel se muestra y se vuelve interactuable
    {
        if(infoPanel == null)
        {
            infoPanel = FindFirstObjectByType<InfoPanel>();
            title = GetComponentInChildren<TextMeshProUGUI>(true).text;
        }
        //Su informaci�n se configura para mostrar la info requerida
        infoPanel.DisplayInfo(title, pages);
    }
}
