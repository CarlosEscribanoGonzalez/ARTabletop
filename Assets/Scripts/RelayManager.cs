using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;

public class RelayManager : MonoBehaviour
{
    [SerializeField] private Button hostButton; //Botón de crear partida
    [SerializeField] private Button clientButton; //Botón de unirse a partida
    [SerializeField] private TextMeshProUGUI joinInputText; //Input Field para introducir el código
    [SerializeField] private TextMeshProUGUI codeText; //Indicador del código de la sala en la parte superior izquierda de la pantalla
    [SerializeField] private GameObject lobby; //Objeto que engloba el menú del lobby

    private void Start()
    {
        //La opción por defecto es la online
        hostButton.onClick.AddListener(CreateRelay); 
        clientButton.onClick.AddListener(JoinRelay);
        SignOutFromPreviousSession();
    }

    private async void SignOutFromPreviousSession()
    {
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();

            if (NetworkManager.Singleton != null &&
                (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient))
            {
                Debug.Log("Cerrando sesión de red (Netcode)...");
                NetworkManager.Singleton.Shutdown();
            }

            if (AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("Cerrando sesión de usuario...");
                AuthenticationService.Instance.SignOut();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al cerrar sesión: " + e.Message);
        }
    }

    public void OnOnlineToggleChanged(bool online) //Llamado cuando el toggle de "partida online" cambia
    {
        //Se eliminan las funciones de los botones
        hostButton.onClick.RemoveAllListeners();
        clientButton.onClick.RemoveAllListeners();
        if (online) //Si se activa el online se suscriben las funciones pertinentes a los botones
        {
            hostButton.onClick.AddListener(CreateRelay);
            clientButton.onClick.AddListener(JoinRelay);
        }
        else //Si no, las que se suscriben son las offline
        {
            hostButton.onClick.AddListener(CreateOfflineCode);
            clientButton.onClick.AddListener(JoinOfflineCode);
        }
    }

    public async void CreateRelay()
    {
        try //Intenta crear una sesión con el relay de Unity
        {
            ToggleButtonInteraction(false); //Se desactivan todos los componentes interactuables para evitar errores
            if (Application.internetReachability == NetworkReachability.NotReachable) //Si no hay conexión a internet se lanza una excepción
                throw new System.Exception("No Internet connection.");
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(8);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId); //Se obtiene el joinCode de la sesión, proporcionado por el relay
            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(allocation, "wss");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost(); //Se inicia la sesión como host
            lobby.SetActive(false); //Se ocultan los botones del lobby
            codeText.text = "Room code: " + joinCode.ToUpper(); //Se activa el texto con el código para que se pueda unir gente
            GameSettings.Instance.IsOnline = true; //Se configura la partida como online
        }
        catch (System.Exception e) //Si no se puede se hace sign out y se imprime un mensaje de error
        {
            Debug.LogError("Session couldn't be created: " + e);
            AuthenticationService.Instance.SignOut();
            StopAllCoroutines(); //Si había una corrutina de enseñar error activa esta se para
            FeedbackManager.Instance.DisplayMessage("Game session couldn't be created. Please, check your Internet connection and try again.");
        }
        finally { ToggleButtonInteraction(true); } //En cualquier caso se reactiva la interacción con los elementos del lobby
    }

    public async void JoinRelay()
    {
        try //Intenta unirse a una sesión
        {
            string joinCode = joinInputText.text; //Se obtiene el valor del input field
            ToggleButtonInteraction(false); //Se desactivan todos los componentes interactuables para evitar errores
            if (Application.internetReachability == NetworkReachability.NotReachable) //Lo mismo que al crear la partida
                throw new System.Exception("No Internet connection.");
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            if (joinCode.Length > 6) joinCode = joinCode.Substring(0, 6); //A veces el input field da un char de más de forma errónea
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(joinAllocation, "wss");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient(); //Se une a la partida como cliente
            lobby.SetActive(false); //Se desactiva el lobby 
            codeText.text = "Room code: " + joinCode.ToUpper(); //Se activa el texto con el código para que se pueda unir gente
            GameSettings.Instance.IsOnline = true; //Se configura la partida como online
        }
        catch (RelayServiceException e)
        {
            AuthenticationService.Instance.SignOut();
            StopAllCoroutines();
            if (e.Message.ToLower().Contains("allocation is full"))
                FeedbackManager.Instance.DisplayMessage("This game session is full. Please try a different code.", Color.white);
            else FeedbackManager.Instance.DisplayMessage("A game session with the provided code couldn't be found. Please, check the join code and try again.");
        }
        catch (System.Exception e) //Si falla al unirse se hace sign out y se imprime un mensaje de error
        {
            Debug.LogError("Invalid code: " + e);
            AuthenticationService.Instance.SignOut();
            StopAllCoroutines();
            FeedbackManager.Instance.DisplayMessage("Game session couldn't be found. Please, check your Internet connection and try again.");
        }
        finally { ToggleButtonInteraction(true); } //En cualquier caso se reactiva la interacción con los elementos del lobby
    }

    public void CreateOfflineCode() //Se crea simplemente una semilla aleatoria para GameSettings
    {
        int randSeed = Random.Range(0, 100000);
        codeText.text = "Room code: " + randSeed; //La semilla se muestra por pantalla para que se pueda unir la gente
        GameSettings.Instance.SetSeed(randSeed);
        lobby.SetActive(false); //Se desactiva el lobby 
        GameSettings.Instance.IsOnline = false; //Se configura el juego como offline
    }

    public void JoinOfflineCode()
    {
        try //Intenta unirse con una semilla dada
        {
            string code = joinInputText.text;
            int randSeed = int.Parse(code.Substring(0, code.Length - 1)); //El Input Field le añade siempre un char extra
            codeText.text = "Room code: " + randSeed; //Muestra la semilla para que la gente se pueda unir
            GameSettings.Instance.SetSeed(randSeed); //Aporta la semilla al GameSettings
            lobby.SetActive(false); //Desactiva el lobby 
            GameSettings.Instance.IsOnline = false; //Se configura el juego como offline
        }
        catch (System.Exception e) //Si ha habido algún problema se muestra el error por pantalla
        {
            Debug.LogError("Invalid code: " + e);
            StopAllCoroutines();
            FeedbackManager.Instance.DisplayMessage("Invalid code. Please, use numbers only.");
        }
    }

    private void ToggleButtonInteraction(bool enable) //Hace toggle la interacción de los elementos del lobby  
    {
        foreach (var selectable in GetComponentsInChildren<Selectable>())
        {
            selectable.interactable = enable;
        }
    }
}
