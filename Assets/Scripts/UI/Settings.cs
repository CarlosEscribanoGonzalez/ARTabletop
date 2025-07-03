using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
    [SerializeField] private Toggle extendedTrackingToggle;
    [SerializeField] private Toggle randomColorToggle;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TextMeshProUGUI[] rulesTexts;
    public bool IsRandomColorEnabled => randomColorToggle.isOn;
    public System.EventHandler<bool> OnColorSettingChanged;

    void Start()
    {
        extendedTrackingToggle.isOn = PlayerPrefs.GetInt("ExtendedTracking", 0) == 0 ? false : true;
        ExtendedTrackingManager.IsXTEnabled = extendedTrackingToggle.isOn;
        randomColorToggle.isOn = PlayerPrefs.GetInt("ColorSetting", 0) == 0 ? false : true;
        //En el menú principal se deja en blanco el inputfield para que el jugador sepa que tiene que meter su nombre
        if (string.IsNullOrEmpty(PlayerPrefs.GetString("PlayerName", ""))) SetRandomName();
        if (SceneManager.GetActiveScene().buildIndex == 0 && PlayerPrefs.GetString("PlayerName").StartsWith("Player_"))
            return;
        nameInputField.SetTextWithoutNotify(PlayerPrefs.GetString("PlayerName", ""));
    }

    public void SaveXTConfig(bool isOn)
    {
        PlayerPrefs.SetInt("ExtendedTracking", isOn ? 1 : 0);
        ExtendedTrackingManager.IsXTEnabled = isOn;
    }

    public void SetXTToggle(bool state)
    {
        extendedTrackingToggle.isOn = state;
    }

    public void SetPlayerName(string name)
    {
        PlayerPrefs.SetString("PlayerName", name.Trim());
        nameInputField.SetTextWithoutNotify(name.Trim());
        if (name.Trim().Length == 0) SetRandomName();
    }

    public void ToggleColorSetting(bool isOn)
    {
        PlayerPrefs.SetInt("ColorSetting", isOn ? 1 : 0);
        OnColorSettingChanged?.Invoke(this, isOn);
    }

    public void SetRules(string rules)
    {
        foreach(var t in rulesTexts)
            t.text = rules != string.Empty ? rules : "Sorry! The author of this game didn't add the rules.";
    }

    public void CloseApp()
    {
        Application.Quit();
    }

    private void SetRandomName()
    {
        PlayerPrefs.SetString("PlayerName", $"Player_{Random.Range(0, 10000)}");
    }
}
