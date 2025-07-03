using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class SceneLoadBlock : MonoBehaviour
{
    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(WaitUntilSceneFullyReady());
    }

    IEnumerator WaitUntilSceneFullyReady()
    {
        yield return null;
        yield return new WaitForEndOfFrame();
        FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include).gameObject.SetActive(true);
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Destroy(this.gameObject);
    }
}

