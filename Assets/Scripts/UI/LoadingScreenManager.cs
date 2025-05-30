using UnityEngine;

public class LoadingScreenManager : MonoBehaviour
{
    private static Canvas canvas;

    private void Awake()
    {
        canvas = GetComponentInChildren<Canvas>();
    }

    public static void ToggleLoadingScreen(bool enable)
    {
        canvas.enabled = enable;
    }
}
