using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonSceneLoader : MonoBehaviour
{
    public void ChangeScene(int newSceneIndex)
    {
        string msg;
        if (newSceneIndex == 0) msg = "Configuring main menu...";
        else if (newSceneIndex == 1) msg = "Configuring game session...";
        else msg = "Configuring game creator...";
        LoadingScreenManager.ToggleLoadingScreen(true, false, msg);
        SceneManager.LoadScene(newSceneIndex);
    }
}
