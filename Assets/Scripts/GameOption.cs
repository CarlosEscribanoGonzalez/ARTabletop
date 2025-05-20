using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOption : MonoBehaviour
{
    public GameInfo Info { get; set; } = null;
    private Button button;
    

    void Start()
    {
        button = GetComponent<Button>();
        if(Info.gameImage != null) button.image.sprite = Info.gameImage;
        button.GetComponentInChildren<TextMeshProUGUI>().text = Info.gameName;
    }

    public void OnClick()
    {
        GameConfigurator.gameInfo = Info;
        SceneManager.LoadScene(1);
    }
}
