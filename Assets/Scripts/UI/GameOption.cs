using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameOption : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI textMesh;
    public GameInfo Info { get; set; } = null; //Informaci�n del juego asociado al bot�n
    private GameDetailedMenu[] detailedInfoPanels;
    
    void Start()
    {
        //Se configura la apariencia del bot�n:
        if(Info.gameImage != null) image.sprite = Info.gameImage;
        textMesh.text = Info.gameName;
        detailedInfoPanels = FindObjectsByType<GameDetailedMenu>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    public void OnClick() //Cuando es pulsado se carga la escena del juego 
    {
        foreach (var detailedInfo in detailedInfoPanels)
        {
            StartCoroutine(DisplayGameInfo(detailedInfo));
        }
    }

    IEnumerator DisplayGameInfo(GameDetailedMenu detailedInfo)
    {
        detailedInfo.SetInfo(Info, this);
        yield return null;
        detailedInfo.GetComponent<Canvas>().enabled = true;
    }
}
