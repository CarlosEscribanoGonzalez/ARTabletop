using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOption : MonoBehaviour
{
    public GameInfo Info { get; set; } = null; //Información del juego asociado al botón
    private Button button; //El botón en sí
    private GameDetailedInfo detailedInfo;
    
    void Start()
    {
        button = GetComponent<Button>();
        //Se configura la apariencia del botón:
        if(Info.gameImage != null) button.image.sprite = Info.gameImage;
        button.GetComponentInChildren<TextMeshProUGUI>().text = Info.gameName;
        detailedInfo = FindFirstObjectByType<GameDetailedInfo>(FindObjectsInactive.Include);
    }

    public void OnClick() //Cuando es pulsado se carga la escena del juego 
    {
        detailedInfo.SetInfo(Info, this);
        detailedInfo.gameObject.SetActive(true);
    }
}
