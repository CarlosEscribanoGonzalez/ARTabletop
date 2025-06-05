using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Settings : MonoBehaviour
{
    [SerializeField] private Toggle extendedTrackingToggle;
    [SerializeField] private TMP_InputField nameInputField;

    void Start()
    {
        extendedTrackingToggle.isOn = PlayerPrefs.GetInt("ExtendedTracking", 0) == 0 ? false : true;
        ExtendedTrackingManager.IsXTEnabled = extendedTrackingToggle.isOn;
        nameInputField.SetTextWithoutNotify(PlayerPrefs.GetString("PlayerName", $"Player{Random.Range(0, 10000)}"));
        PlayerPrefs.SetString("PlayerName", nameInputField.text);
    }

    public void SaveXTConfig(bool isOn)
    {
        PlayerPrefs.SetInt("ExtendedTracking", isOn ? 1 : 0);
        ExtendedTrackingManager.IsXTEnabled = isOn;
    }

    public void SetPlayerName(string name)
    {
        PlayerPrefs.SetString("PlayerName", name);
    }
}
