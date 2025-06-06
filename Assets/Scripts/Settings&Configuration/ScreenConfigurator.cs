using UnityEngine;

public class ScreenConfigurator : MonoBehaviour
{
    [SerializeField] private bool forcePortrait = false;
    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.autorotateToLandscapeLeft = Screen.autorotateToLandscapeRight = !forcePortrait;
        if(forcePortrait && 
            (Screen.orientation == ScreenOrientation.LandscapeRight || Screen.orientation == ScreenOrientation.LandscapeLeft))
        {
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.orientation = ScreenOrientation.AutoRotation;
        }
    }
}
