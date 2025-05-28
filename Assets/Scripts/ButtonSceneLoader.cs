using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonSceneLoader : MonoBehaviour
{
    public void ChangeScene(int newSceneIndex)
    {
        SceneManager.LoadScene(newSceneIndex);
    }
}
