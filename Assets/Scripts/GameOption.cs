using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOption : MonoBehaviour
{
    public GameInfo Info { get; set; } = null;
    private ConfirmationPanel confirmationPanel;
    private Button button;
    
    void Start()
    {
        confirmationPanel = FindFirstObjectByType<ConfirmationPanel>(FindObjectsInactive.Include);
        button = GetComponent<Button>();
        if(Info.gameImage != null) button.image.sprite = Info.gameImage;
        button.GetComponentInChildren<TextMeshProUGUI>().text = Info.gameName;
    }

    public void OnClick()
    {
        GameConfigurator.gameInfo = Info;
        SceneManager.LoadScene(1);
    }

    public void OnRemoveButtonPressed()
    {
        confirmationPanel.CurrentOption = this;
        confirmationPanel.gameObject.SetActive(true);
    }

    public void RemoveGame()
    {
        Info.Delete();
        Destroy(this.gameObject);
    }

    public void Share()
    {
        Info.Share();
    }
}
