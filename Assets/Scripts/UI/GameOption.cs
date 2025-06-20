using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOption : MonoBehaviour
{
    public GameInfo Info { get; set; } = null; //Informaci�n del juego asociado al bot�n
    private ConfirmationPanel confirmationPanel; //Panel de confirmaci�n de borrado de juego
    private Button button; //El bot�n en s�
    
    void Start()
    {
        confirmationPanel = FindFirstObjectByType<ConfirmationPanel>(FindObjectsInactive.Include);
        button = GetComponent<Button>();
        //Se configura la apariencia del bot�n:
        if(Info.gameImage != null) button.image.sprite = Info.gameImage;
        button.GetComponentInChildren<TextMeshProUGUI>().text = Info.gameName;
    }

    public void OnClick() //Cuando es pulsado se carga la escena del juego 
    {
        LoadingScreenManager.ToggleLoadingScreen(true); //Pesta�a de cargando para que no se raye el usuario
        GameConfigurator.EssentialInfo = Info; //Se pasa la info del juego al Game Configurator para que se configure
        SceneManager.LoadScene(1);
    }

    public void OnRemoveButtonPressed() //Llamado cuando el bot�n de borrado se pulsa
    {
        confirmationPanel.CurrentOption = this; //Configura el panel de confirmaci�n y lo activa
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

    public void ConfigureAsDefaultGame() //Configura el juego como juego base de la app: sin los botones de eliminar y compartir
    {
        Info.isDefault = true;
        foreach(var button in GetComponentsInChildren<Button>())
        {
            if (button != GetComponent<Button>()) button.gameObject.SetActive(false); //Se borran todos los botones menos el de abrir el juego
        }
    }

    IEnumerator ShareCoroutine()
    {
        LoadingScreenManager.ToggleLoadingScreen(true);
        yield return null; //Se espera un frame para que la pesta�a aparezca
        GameSharer.Share(GameInfo.GetFullInfo(Info));
    }

    IEnumerator DeleteCoroutine()
    {
        LoadingScreenManager.ToggleLoadingScreen(true);
        yield return null;
        FindFirstObjectByType<GameOptionsManager>().RemoveGame(Info, this.transform); //Se borra la opci�n de la lista de juegos
        GameDeleter.DeleteGameFiles(GameInfo.GetFullInfo(Info)); //Se borra toda la informaci�n del juego de los archivos locales
        Destroy(this.gameObject); //Se destruye el bot�n
    }

    IEnumerator EditCoroutine()
    {
        LoadingScreenManager.ToggleLoadingScreen(true);
        yield return null;
        ToolManager.GameToEdit = GameInfo.GetFullInfo(Info);
        SceneManager.LoadScene(2);
    }
}
