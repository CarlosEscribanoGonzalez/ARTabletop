using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlayableUnit : MonoBehaviour
{
    [SerializeField] private GameObject card;
    [SerializeField] private GameObject piece;
    [SerializeField] private GameObject board;
    [SerializeField] private GameObject noInfoIndicator; //Indicador que aparece en caso de que el marcador no tenga info asociada
    private ARTrackedImage trackable; //El marcador
    private AGameUnit gameUnit;
    private ARAnchor anchor;
    private bool attached = false;
    private float scaleFactor = 1;
    //Distintas c�maras de distintos dispositivos no tienen la misma calibraci�n,
    //por lo que los objetos han de escalarse dependiendo de la misma para que siempre tengan el mismo tama�o aparente
    public static float ScaleCameraFactor { get; set; } = 1; 

    private void Start() //Se activa la game unit asociada al marcador, a�n sin informaci�n
    {
        trackable = GetComponent<ARTrackedImage>(); //Se detecta el tipo de marcador del objeto y se activa su contenido correspondiente
        card.SetActive(trackable.referenceImage.name.ToLower().Contains("card"));
        piece.SetActive(trackable.referenceImage.name.ToLower().Contains("piece"));
        board.SetActive(trackable.referenceImage.name.ToLower().Contains("board"));
        gameUnit = GetComponentInChildren<AGameUnit>(false);
        //Se escala el objeto dependiendo de la calibraci�n de la c�mara del dispositivo
        this.transform.localScale *= ScaleCameraFactor;
    }

    public void DisplayNoInfoIndicator() 
    {
        //Si no hay informaci�n disponible para el marcador una vez el objeto correspondiente se ha activado
        //y ha tratado de conseguirla se activa el indicador
        card.SetActive(false);
        piece.SetActive(false);
        board.SetActive(false);
        noInfoIndicator.SetActive(true);
    }

    public void ManageTracking(ARTrackable trackedImage)
    {
        if (gameUnit == null) return;
        if (!ExtendedTrackingManager.IsXTEnabled && !gameUnit.InForceMaintain)
        {
            gameUnit.gameObject.SetActive(trackedImage.trackingState == TrackingState.Tracking);
            if(attached) DetachFromAnchor();
        }
        else
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                gameUnit.gameObject.SetActive(true);
                if (attached) DetachFromAnchor();
            }
            else
            {
                if (anchor == null) anchor = AnchorCreator.CreateAnchor(gameUnit.gameObject);
                if(!attached) AttachToAnchor();
            }
        }
    }

    private void AttachToAnchor()
    {
        if (anchor != null)
        {
            Debug.Log("Attaching to anchor...");
            float initDist = Vector3.Distance(gameUnit.transform.position, Camera.main.transform.position);
            Debug.Log("INIT POS: " + gameUnit.transform.position);
            gameUnit.transform.SetParent(anchor.transform);
            gameUnit.transform.localPosition = Vector3.zero;
            float finalDist = Vector3.Distance(gameUnit.transform.position, Camera.main.transform.position);
            scaleFactor = finalDist / initDist;
            Debug.Log("FINAL POS: " + gameUnit.transform.position);
            Debug.Log($"Init dist: {initDist}; final dist: {finalDist}; scaleFactor: {scaleFactor}");
            if (scaleFactor > 0) gameUnit.transform.localScale *= scaleFactor;
            attached = true;
            Debug.LogWarning("Anchored lossy scale: " + gameUnit.transform.lossyScale);
        }
        else
        {
            Debug.LogError("Error: anchor es null");
        }
    }

    private void DetachFromAnchor()
    {
        if (anchor == null) return;
        Debug.Log("Detaching from anchor...");
        gameUnit.transform.localScale /= scaleFactor;
        gameUnit.transform.SetParent(this.transform);
        //gameUnit.transform.localPosition = Vector3.zero; -> Est� generando comportamiento raro y no s� por qu�
        attached = false;
        Debug.LogWarning("Unanchored lossy scale: " + gameUnit.transform.lossyScale);
    }
}
