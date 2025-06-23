using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Settings : MonoBehaviour
{
    [SerializeField] private Toggle extendedTrackingToggle;
    [SerializeField] private Toggle randomColorToggle;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TextMeshProUGUI rulesText;
    public bool IsRandomColorEnabled => randomColorToggle.isOn;
    public System.EventHandler<bool> OnColorSettingChanged;

    void Start()
    {
        extendedTrackingToggle.isOn = PlayerPrefs.GetInt("ExtendedTracking", 0) == 0 ? false : true;
        ExtendedTrackingManager.IsXTEnabled = extendedTrackingToggle.isOn;
        randomColorToggle.isOn = PlayerPrefs.GetInt("ColorSetting", 0) == 0 ? false : true;
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

    public void ToggleColorSetting(bool isOn)
    {
        PlayerPrefs.SetInt("ColorSetting", isOn ? 1 : 0);
        OnColorSettingChanged?.Invoke(this, isOn);
    }

    public void SetRules(string rules)
    {
        rulesText.text = rules != string.Empty ? rules : "Sorry! The author of this game didn't add the rules.";
    }
}
