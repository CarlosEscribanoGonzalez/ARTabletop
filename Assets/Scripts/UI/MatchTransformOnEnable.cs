using UnityEngine;

public class MatchTransformOnEnable : MonoBehaviour
{
    [SerializeField] private Transform transformToMatch;

    private void OnEnable()
    {
        this.transform.position = transformToMatch.position;
    }
}
