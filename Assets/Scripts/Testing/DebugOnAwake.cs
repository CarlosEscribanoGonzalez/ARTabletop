using UnityEngine;

public class DebugOnAwake : MonoBehaviour
{
    void Start()
    {
        FeedbackManager.Instance.DisplayMessage("HOLA", Color.yellow);
    }
}
