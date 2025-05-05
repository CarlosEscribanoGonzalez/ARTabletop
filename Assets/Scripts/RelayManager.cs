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

    private void Start()
    {
        hostButton.onClick.AddListener(CreateRelay);
        clientButton.onClick.AddListener(() => JoinRelay(joinInputText.text));
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
            codeText.text = "Room code: " + joinCode;
            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(allocation, "wss");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();
            errorText.transform.parent.gameObject.SetActive(false);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Session couldn't be created: " + e);
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
            errorText.transform.parent.gameObject.SetActive(false);
            codeText.text = "Room code: " + joinCode;
        }
        catch(System.Exception e)
        {
            Debug.LogError("Invalid code: " + e);
            StopAllCoroutines();
            StartCoroutine(DisplayErrorText("We couldn't find a game session with the provided code. Please, check the code and your Internet connection."));
        }
        finally { ToggleButtonInteraction(true); }
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
