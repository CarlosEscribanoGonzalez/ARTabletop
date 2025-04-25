using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameSettings : MonoBehaviour
{
    [SerializeField] private bool extendedTracking = false;
    private List<SpecialCardGameManager> specialCardManagers;
    public static GameSettings Instance;

    private void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        if (extendedTracking) FindFirstObjectByType<ImageDetectionManager>().enabled = false;
        specialCardManagers = FindObjectsByType<SpecialCardGameManager>(FindObjectsSortMode.None).ToList();
        Instance = this;
    }

    public SpecialCardGameManager GetSpecialCardManager(string markerName)
    {
        int index = int.Parse(markerName.Substring(markerName.Length - 1)) - 1;
        if (index >= 0 && index < specialCardManagers.Count) return specialCardManagers[index];
        else return null;
    }
}
