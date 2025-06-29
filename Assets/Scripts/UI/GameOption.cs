using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOption : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI textMesh;
    public GameInfo Info { get; set; } = null; //Información del juego asociado al botón
    private GameDetailedInfo detailedInfo;
    
    void Start()
    {
        //Se configura la apariencia del botón:
        if(Info.gameImage != null) image.sprite = Info.gameImage;
        textMesh.text = Info.gameName;
        detailedInfo = FindFirstObjectByType<GameDetailedInfo>(FindObjectsInactive.Include);
    }

    public void OnClick() //Cuando es pulsado se carga la escena del juego 
    {
        detailedInfo.SetInfo(Info, this);
        detailedInfo.GetComponent<Canvas>().enabled = true;
    }
}
