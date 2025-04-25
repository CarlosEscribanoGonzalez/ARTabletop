using UnityEngine;

public class GameSettings : MonoBehaviour
{
    [SerializeField] private bool extendedTracking = false;
    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        if (extendedTracking) FindFirstObjectByType<ImageDetectionManager>().enabled = false;
    }
}
