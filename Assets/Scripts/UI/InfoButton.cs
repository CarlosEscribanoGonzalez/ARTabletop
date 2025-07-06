using UnityEngine;
using TMPro;

public class InfoButton : MonoBehaviour
{
    [TextArea][SerializeField] private string[] pages; //Información a desplegar 
    private string title; //Título de la información
    private InfoPanel infoPanel;

    private void Awake()
    {
        infoPanel = FindFirstObjectByType<InfoPanel>();
        title = GetComponentInChildren<TextMeshProUGUI>(true).text; //Lo que pone en el botón es el título de la info a mostrar
    }

    public void DisplayInfo() //El infopanel se muestra y se vuelve interactuable
    {
        if(infoPanel == null)
        {
            infoPanel = FindFirstObjectByType<InfoPanel>();
            title = GetComponentInChildren<TextMeshProUGUI>(true).text;
        }
        //Su información se configura para mostrar la info requerida
        infoPanel.DisplayInfo(title, pages);
    }
}
