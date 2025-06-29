using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameDetailedInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI author;
    [SerializeField] private TextMeshProUGUI lastEditor;
    [SerializeField] private TextMeshProUGUI rules;
    [SerializeField] private Button gameImage;
    [SerializeField] private GameObject extraButtons;
    private ConfirmationPanel confirmationPanel; //Panel de confirmaci�n de borrado de juego
    private GameInfo info;
    private GameOption gameOption;

    private void Awake()
    {
        confirmationPanel = FindFirstObjectByType<ConfirmationPanel>(FindObjectsInactive.Include);
    }

    public void SetInfo(GameInfo info, GameOption gameOption)
    {
        this.info = info;
        title.text = info.gameName;
        author.text = $"By: {info.author}";
        if(!info.author.Equals(info.lastEditor))
            lastEditor.text = info.lastEditor.Length > 0 ? $"Last edited by: {info.lastEditor}" : "";
        rules.text = info.rules != string.Empty ? info.rules : "Sorry! The author of this game didn't add the rules.";
        gameImage.image.sprite = info.gameImage;
        this.gameOption = gameOption;
        extraButtons.SetActive(!info.isDefault);
    }

    public void Play()
    {
        LoadingScreenManager.ToggleLoadingScreen(true); //Pesta�a de cargando para que no se raye el usuario
        GameConfigurator.EssentialInfo = info; //Se pasa la info del juego al Game Configurator para que se configure
        SceneManager.LoadScene(1);
    }

    public void OnRemoveButtonPressed() //Llamado cuando el bot�n de borrado se pulsa
    {
        confirmationPanel.Info = info; //Configura el panel de confirmaci�n y lo activa
        confirmationPanel.gameObject.SetActive(true);
    }

    public void DeleteGame() //Llamado cuando en el panel de confirmaci�n se confirma el borrado del juego
    {
        StartCoroutine(DeleteCoroutine());
    }

    public void EditGame()
    {
        StartCoroutine(EditCoroutine());
    }

    public void Share() //Llamado cuando se pulsa el bot�n de compartir
    {
        StartCoroutine(ShareCoroutine());
    }

    IEnumerator ShareCoroutine()
    {
        LoadingScreenManager.ToggleLoadingScreen(true);
        yield return null; //Se espera un frame para que la pesta�a aparezca
        GameSharer.Share(info);
    }

    IEnumerator DeleteCoroutine()
    {
        LoadingScreenManager.ToggleLoadingScreen(true);
        yield return null;
        FindFirstObjectByType<GameOptionsManager>().RemoveGame(info, gameOption.transform); //Se borra la opci�n de la lista de juegos
        //En este caso, como el borrado s�lo utiliza la informaci�n serializada no hace falta hacer GetFullInfo
        GameDeleter.DeleteGameFiles(info); //Se borra toda la informaci�n del juego de los archivos locales
        Destroy(gameOption.gameObject); //Se destruye el bot�n
        this.gameObject.SetActive(false);
    }

    IEnumerator EditCoroutine()
    {
        LoadingScreenManager.ToggleLoadingScreen(true);
        yield return null;
        ToolManager.GameToEdit = info;
        SceneManager.LoadScene(2);
    }
}
