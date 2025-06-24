using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOption : MonoBehaviour
{
    public GameInfo Info { get; set; } = null; //Informaci�n del juego asociado al bot�n
    private Button button; //El bot�n en s�
    private GameDetailedInfo detailedInfo;
    
    void Start()
    {
        button = GetComponent<Button>();
        //Se configura la apariencia del bot�n:
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
