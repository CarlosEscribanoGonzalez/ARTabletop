using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LoadingScreenManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI message;
    private Canvas canvas;
    public static bool Blocked { get; set; } = false;
    public static bool UnlockBySceneChange { get; set; } = false;
    public static LoadingScreenManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
        canvas = GetComponentInChildren<Canvas>();
        SceneManager.sceneLoaded += ((_, _) =>
        {
            if (UnlockBySceneChange) Blocked = false;
            UnlockBySceneChange = false;
            ToggleLoadingScreen(false);
        }); //La escena inicial empieza con el loading hasta que los juegos cambian
    }

    public static void ToggleLoadingScreen(bool enable, bool block = false, string msg = "")
    {
        if (Blocked) return;
        Instance.ShowScreen(enable, msg);
        Blocked = block;
    }

    private void ShowScreen(bool enable, string msg)
    {
        if (canvas != null) canvas.enabled = enable;
        message.text = msg;
    }
}
