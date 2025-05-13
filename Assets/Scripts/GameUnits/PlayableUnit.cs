using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PlayableUnit : MonoBehaviour
{
    [SerializeField] private GameObject card;
    [SerializeField] private GameObject piece;
    [SerializeField] private GameObject board;
    [SerializeField] private GameObject noInfoIndicator; //Indicador que aparece en caso de que el marcador no tenga info asociada
    private ARTrackedImage trackable; //El marcador
    //Distintas cámaras de distintos dispositivos no tienen la misma calibración,
    //por lo que los objetos han de escalarse dependiendo de la misma para que siempre tengan el mismo tamaño aparente
    public static float ScaleCameraFactor { get; set; } = 1; 

    private void Start() //Se activa la game unit asociada al marcador, aún sin información
    {
        trackable = GetComponent<ARTrackedImage>(); //Se detecta el tipo de marcador del objeto y se activa su contenido correspondiente
        card.SetActive(trackable.referenceImage.name.ToLower().Contains("card"));
        piece.SetActive(trackable.referenceImage.name.ToLower().Contains("piece"));
        board.SetActive(trackable.referenceImage.name.ToLower().Contains("board"));
        //Se escala el objeto dependiendo de la calibración de la cámara del dispositivo
        this.transform.localScale *= ScaleCameraFactor;
    }

    public void DisplayNoInfoIndicator() 
    {
        //Si no hay información disponible para el marcador una vez el objeto correspondiente se ha activado
        //y ha tratado de conseguirla se activa el indicador
        card.SetActive(false);
        piece.SetActive(false);
        board.SetActive(false);
        noInfoIndicator.SetActive(true);
    }
}
