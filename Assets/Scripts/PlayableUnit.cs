using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PlayableUnit : MonoBehaviour
{
    [SerializeField] private GameObject card;
    [SerializeField] private GameObject piece;
    private ARTrackedImage trackable;

    void Start()
    {
        trackable = GetComponent<ARTrackedImage>();
        card.SetActive(trackable.referenceImage.name.ToLower().Contains("card"));
        piece.SetActive(trackable.referenceImage.name.ToLower().Contains("piece"));
    }
}
