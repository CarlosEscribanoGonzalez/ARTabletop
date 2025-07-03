using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameOption : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI textMesh;
    public GameInfo Info { get; set; } = null; //Información del juego asociado al botón
    private GameDetailedInfo[] detailedInfoPanels;
    
    void Start()
    {
        //Se configura la apariencia del botón:
        if(Info.gameImage != null) image.sprite = Info.gameImage;
        textMesh.text = Info.gameName;
        detailedInfoPanels = FindObjectsByType<GameDetailedInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    public void OnClick() //Cuando es pulsado se carga la escena del juego 
    {
        foreach (var detailedInfo in detailedInfoPanels)
        {
            StartCoroutine(DisplayGameInfo(detailedInfo));
        }
    }

    IEnumerator DisplayGameInfo(GameDetailedInfo detailedInfo)
    {
        detailedInfo.SetInfo(Info, this);
        yield return null;
        detailedInfo.GetComponent<Canvas>().enabled = true;
    }
}
