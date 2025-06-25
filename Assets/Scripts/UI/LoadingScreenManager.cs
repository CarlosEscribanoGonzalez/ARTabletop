using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreenManager : MonoBehaviour
{
    private static Canvas canvas;
    public static bool Blocked { get; set; } = false;
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
        SceneManager.sceneLoaded += ((_, _) => ToggleLoadingScreen(false)); //La escena inicial empieza con el loading hasta que los juegos cambian
    }

    public static void ToggleLoadingScreen(bool enable, bool block = false)
    {
        if (Blocked) return;
        if(canvas != null) canvas.enabled = enable;
        Blocked = block;
    }
}
