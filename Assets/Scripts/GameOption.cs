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
        LoadingScreenManager.ToggleLoadingScreen(true);
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
        FindFirstObjectByType<LayoutManager>().RemoveContent(this.transform);
        Destroy(this.gameObject);
    }

    public void Share()
    {
        LoadingScreenManager.ToggleLoadingScreen(true);
        Invoke("ShareAfterCooldown", 0.1f);
    }

    public void ConfigureAsDefaultGame()
    {
        foreach(var button in GetComponentsInChildren<Button>())
        {
            if (button != GetComponent<Button>()) button.gameObject.SetActive(false);
        }
    }

    private void ShareAfterCooldown()
    {
        Info.Share();
    }
}
