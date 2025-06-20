using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOption : MonoBehaviour
{
    public GameInfo Info { get; set; } = null; //Información del juego asociado al botón
    private ConfirmationPanel confirmationPanel; //Panel de confirmación de borrado de juego
    private Button button; //El botón en sí
    
    void Start()
    {
        confirmationPanel = FindFirstObjectByType<ConfirmationPanel>(FindObjectsInactive.Include);
        button = GetComponent<Button>();
        //Se configura la apariencia del botón:
        if(Info.gameImage != null) button.image.sprite = Info.gameImage;
        button.GetComponentInChildren<TextMeshProUGUI>().text = Info.gameName;
    }

    public void OnClick() //Cuando es pulsado se carga la escena del juego 
    {
        LoadingScreenManager.ToggleLoadingScreen(true); //Pestaña de cargando para que no se raye el usuario
        GameConfigurator.EssentialInfo = Info; //Se pasa la info del juego al Game Configurator para que se configure
        SceneManager.LoadScene(1);
    }

    public void OnRemoveButtonPressed() //Llamado cuando el botón de borrado se pulsa
    {
        confirmationPanel.CurrentOption = this; //Configura el panel de confirmación y lo activa
        confirmationPanel.gameObject.SetActive(true);
    }

    public void DeleteGame() //Llamado cuando en el panel de confirmación se confirma el borrado del juego
    {
        StartCoroutine(DeleteCoroutine());
    }

    public void EditGame()
    {
        StartCoroutine(EditCoroutine());
    }

    public void Share() //Llamado cuando se pulsa el botón de compartir
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
        yield return null; //Se espera un frame para que la pestaña aparezca
        GameSharer.Share(GameInfo.GetFullInfo(Info));
    }

    IEnumerator DeleteCoroutine()
    {
        LoadingScreenManager.ToggleLoadingScreen(true);
        yield return null;
        FindFirstObjectByType<GameOptionsManager>().RemoveGame(Info, this.transform); //Se borra la opción de la lista de juegos
        GameDeleter.DeleteGameFiles(GameInfo.GetFullInfo(Info)); //Se borra toda la información del juego de los archivos locales
        Destroy(this.gameObject); //Se destruye el botón
    }

    IEnumerator EditCoroutine()
    {
        LoadingScreenManager.ToggleLoadingScreen(true);
        yield return null;
        ToolManager.GameToEdit = GameInfo.GetFullInfo(Info);
        SceneManager.LoadScene(2);
    }
}
