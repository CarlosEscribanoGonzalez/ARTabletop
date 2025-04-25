using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PlayableUnit : MonoBehaviour
{
    [SerializeField] private GameObject card;
    [SerializeField] private GameObject piece;
    [SerializeField] private GameObject board;
    [SerializeField] private GameObject noInfoIndicator;
    private ARTrackedImage trackable;

    private void Start() //Se activa la game unit asociada al marcador, a�n sin informaci�n
    {
        trackable = GetComponent<ARTrackedImage>();
        card.SetActive(trackable.referenceImage.name.ToLower().Contains("card"));
        piece.SetActive(trackable.referenceImage.name.ToLower().Contains("piece"));
        board.SetActive(trackable.referenceImage.name.ToLower().Contains("board"));
    }

    public void DisplayNoInfoIndicator() //Si no hay informaci�n disponible para el marcador se activa el indicador
    {
        card.SetActive(false);
        piece.SetActive(false);
        board.SetActive(false);
        noInfoIndicator.SetActive(true);
    }
}
