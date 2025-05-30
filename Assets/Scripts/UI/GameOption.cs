using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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
        GameConfigurator.GameInfo = Info; //Se pasa la info del juego al Game Configurator para que se configure
        SceneManager.LoadScene(1);
    }

    public void OnRemoveButtonPressed() //Llamado cuando el botón de borrado se pulsa
    {
        confirmationPanel.CurrentOption = this; //Configura el panel de confirmación y lo activa
        confirmationPanel.gameObject.SetActive(true);
    }

    public void DeleteGame() //Llamado cuando en el panel de confirmación se confirma el borrado del juego
    {
        GameDeleter.DeleteGameFiles(Info); //Se borra toda la información del juego de los archivos locales
        FindFirstObjectByType<GameOptionsManager>().RemoveGame(Info, this.transform); //Se borra la opción de la lista de juegos
        Destroy(this.gameObject); //Se destruye el botón
    }

    public void Share() //Llamado cuando se pulsa el botón de compartir
    {
        LoadingScreenManager.ToggleLoadingScreen(true);
        //Se añade un pequeño delay antes de que Android bloquee la app sin la loading screen abierta, lo cual queda raro
        Invoke("ShareAfterCooldown", 0.2f); 
    }

    public void ConfigureAsDefaultGame() //Configura el juego como juego base de la app: sin los botones de eliminar y compartir
    {
        foreach(var button in GetComponentsInChildren<Button>())
        {
            if (button != GetComponent<Button>()) button.gameObject.SetActive(false); //Se borran todos los botones menos el de abrir el juego
        }
    }

    private void ShareAfterCooldown() //Comparte el juego 
    {
        GameSharer.Share(Info);
    }
}
