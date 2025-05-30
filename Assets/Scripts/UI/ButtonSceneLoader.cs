using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonSceneLoader : MonoBehaviour
{
    public void ChangeScene(int newSceneIndex)
    {
        LoadingScreenManager.ToggleLoadingScreen(true);
        SceneManager.LoadScene(newSceneIndex);
    }
}
