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
using System.Collections;

public class RelayManager : MonoBehaviour
{
    [SerializeField] private Button hostButton; 
    [SerializeField] private Button clientButton;
    [SerializeField] private TextMeshProUGUI joinInputText;
    [SerializeField] private TextMeshProUGUI codeText;
    [SerializeField] private TextMeshProUGUI errorText;
    private GameObject lobby;

    private void Start()
    {
        lobby = errorText.transform.parent.gameObject;
        if (GameSettings.Instance.RequiresOnline)
        {
            hostButton.onClick.AddListener(CreateRelay);
            clientButton.onClick.AddListener(() => JoinRelay(joinInputText.text));
        }
        else
        {
            hostButton.onClick.AddListener(CreateOfflineCode);
            clientButton.onClick.AddListener(() => JoinOfflineCode(joinInputText.text));
        }
    }

    async void CreateRelay()
    {
        try
        {
            ToggleButtonInteraction(false);
            errorText.gameObject.SetActive(false);
            if (Application.internetReachability == NetworkReachability.NotReachable)
                throw new System.Exception("No Internet connection.");
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(8);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            codeText.text = "Room code: " + joinCode.ToUpper();
            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(allocation, "wss");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();
            lobby.SetActive(false);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Session couldn't be created: " + e);
            AuthenticationService.Instance.SignOut();
            StopAllCoroutines();
            StartCoroutine(DisplayErrorText("We couldn't create the game session. Please, check your Internet connection and try again."));
        }
        finally { ToggleButtonInteraction(true); }
    }

    async void JoinRelay(string joinCode)
    {
        try
        {
            ToggleButtonInteraction(false);
            errorText.gameObject.SetActive(false);
            if (Application.internetReachability == NetworkReachability.NotReachable)
                throw new System.Exception("No Internet connection.");
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            if (joinCode.Length > 6) joinCode = joinCode.Substring(0, 6);
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(joinAllocation, "wss");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
            lobby.SetActive(false);
            codeText.text = "Room code: " + joinCode.ToUpper();
        }
        catch(System.Exception e)
        {
            Debug.LogError("Invalid code: " + e);
            AuthenticationService.Instance.SignOut();
            StopAllCoroutines();
            StartCoroutine(DisplayErrorText("We couldn't find a game session with the provided code. Please, check the code and your Internet connection."));
        }
        finally { ToggleButtonInteraction(true); }
    }

    private void CreateOfflineCode()
    {
        int randSeed = Random.Range(0, 100000);
        codeText.text = "Room code: " + randSeed;
        GameSettings.Instance.SetSeed(randSeed);
        lobby.SetActive(false);
    }

    private void JoinOfflineCode(string code)
    {
        try
        {
            int randSeed = int.Parse(code.Substring(0, code.Length - 1)); //El Input Field le añade siempre un char extra
            codeText.text = "Room code: " + randSeed;
            GameSettings.Instance.SetSeed(randSeed);
            lobby.SetActive(false);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Invalid code: " + e);
            StopAllCoroutines();
            StartCoroutine(DisplayErrorText("Invalid code. Please, try using numbers only."));
        }
    }

    private void ToggleButtonInteraction(bool enable)
    {
        hostButton.interactable = enable;
        clientButton.interactable = enable;
        joinInputText.GetComponentInParent<TMP_InputField>(true).interactable = enable;
    }

    IEnumerator DisplayErrorText(string text)
    {
        errorText.text = text;
        errorText.gameObject.SetActive(true);
        yield return new WaitForSeconds(5);
        errorText.gameObject.SetActive(false);
    }
}
