using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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
        GameConfigurator.GameInfo = Info; //Se pasa la info del juego al Game Configurator para que se configure
        SceneManager.LoadScene(1);
    }

    public void OnRemoveButtonPressed() //Llamado cuando el bot�n de borrado se pulsa
    {
        confirmationPanel.CurrentOption = this; //Configura el panel de confirmaci�n y lo activa
        confirmationPanel.gameObject.SetActive(true);
    }

    public void DeleteGame() //Llamado cuando en el panel de confirmaci�n se confirma el borrado del juego
    {
        GameDeleter.DeleteGameFiles(Info); //Se borra toda la informaci�n del juego de los archivos locales
        FindFirstObjectByType<GameOptionsManager>().RemoveGame(Info, this.transform); //Se borra la opci�n de la lista de juegos
        Destroy(this.gameObject); //Se destruye el bot�n
    }

    public void Share() //Llamado cuando se pulsa el bot�n de compartir
    {
        LoadingScreenManager.ToggleLoadingScreen(true);
        //Se a�ade un peque�o delay antes de que Android bloquee la app sin la loading screen abierta, lo cual queda raro
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
