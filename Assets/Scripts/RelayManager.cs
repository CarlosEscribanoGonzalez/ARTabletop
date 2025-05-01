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
    [SerializeField] private GameObject errorText;

    async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        hostButton.onClick.AddListener(CreateRelay);
        clientButton.onClick.AddListener(() => JoinRelay(joinInputText.text));
    }

    async void CreateRelay()
    {
        try
        {
            ToggleButtonInteraction(false);
            errorText.SetActive(false);
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(8);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            codeText.text = "Room code: " + joinCode;
            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(allocation, "wss");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();
            errorText.transform.parent.gameObject.SetActive(false);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("No se pudo crear la partida: " + e);
        }
        finally { ToggleButtonInteraction(true); }
    }

    async void JoinRelay(string joinCode)
    {
        try
        {
            ToggleButtonInteraction(false);
            errorText.SetActive(false);
            if (joinCode.Length > 6) joinCode = joinCode.Substring(0, 6);
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(joinAllocation, "wss");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
            errorText.transform.parent.gameObject.SetActive(false);
            codeText.text = "Room code: " + joinCode;
        }
        catch(RelayServiceException e)
        {
            Debug.LogError("Código no válido: " + e);
            StopAllCoroutines();
            StartCoroutine(DisplayErrorText());
        }
        finally { ToggleButtonInteraction(true); }
    }

    private void ToggleButtonInteraction(bool enable)
    {
        hostButton.interactable = enable;
        clientButton.interactable = enable;
        joinInputText.GetComponentInParent<TMP_InputField>(true).interactable = enable;
    }

    IEnumerator DisplayErrorText()
    {
        errorText.SetActive(true);
        yield return new WaitForSeconds(5);
        errorText.SetActive(false);
    }
}
