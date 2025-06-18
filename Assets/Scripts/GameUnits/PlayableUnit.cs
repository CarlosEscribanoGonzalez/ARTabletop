using System.Collections;
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
    private Quaternion initRotation;
    private bool inAnchorCooldown = false;
    //Distintas cámaras de distintos dispositivos no tienen la misma calibración,
    //por lo que los objetos han de escalarse dependiendo de la misma para que siempre tengan el mismo tamaño aparente
    public static float ScaleCameraFactor { get; set; } = 1; 

    private void Start() //Se activa la game unit asociada al marcador, aún sin información
    {
        trackable = GetComponent<ARTrackedImage>(); //Se detecta el tipo de marcador del objeto
        if (trackable.referenceImage.name.ToLower().Contains("card")) gameUnit = card.GetComponent<Card>();
        else if (trackable.referenceImage.name.ToLower().Contains("piece")) gameUnit = piece.GetComponent<Piece>();
        else if (trackable.referenceImage.name.ToLower().Contains("board")) gameUnit = board.GetComponent<Board>();
        //this.transform.localScale *= ScaleCameraFactor; //Se escala el objeto dependiendo de la calibración de la cámara del dispositivo
        initRotation = gameUnit.transform.localRotation; //Se obtiene la rotación inicial para hacer bien el detach (XT)
        StartCoroutine(EnableVisualizationWhenTracked()); //El objeto se verá una vez esté bien posicionado por el tracking
    }

    public void DisplayNoInfoIndicator() 
    {
        //Si no hay información disponible para el marcador una vez el objeto correspondiente se ha activado
        //y ha tratado de conseguirla se activa el indicador
        card.SetActive(false);
        piece.SetActive(false);
        board.SetActive(false);
        noInfoIndicator.SetActive(true); //Hay un bug en algunos móviles que lo hace visible por la cara
        foreach (Transform t in noInfoIndicator.GetComponentsInChildren<Transform>(true)) t.gameObject.SetActive(true);
        gameUnit = null;
    }

    public void ManageTracking(ARTrackable trackedImage)
    {
        if(gameUnit == null)
        {
            noInfoIndicator.SetActive(trackedImage.trackingState == TrackingState.Tracking);
            return;
        }
        if (!ExtendedTrackingManager.IsXTEnabled && !gameUnit.InForceMaintain)
        {
            gameUnit.gameObject.SetActive(trackedImage.trackingState == TrackingState.Tracking);
            if(attached) DetachFromAnchor();
        }
        else
        {
            if(ExtendedTrackingManager.IsXTEnabled && !ExtendedTrackingManager.ISXTReady)
            {
                gameUnit.gameObject.SetActive(false);
                return;
            }
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                gameUnit.gameObject.SetActive(true);
                if (attached) DetachFromAnchor();
            }
            else if (gameUnit.gameObject.activeSelf && !attached && !inAnchorCooldown && ExtendedTrackingManager.ISXTReady)
            {
                anchor = AnchorCreator.Instance.CreateAnchor(gameUnit.gameObject);
                AttachToAnchor(); 
                StartCoroutine(SetAnchorCooldown());
            }
        }
    }

    private void AttachToAnchor()
    {
        if(anchor == null) anchor = AnchorCreator.Instance.CreateAnchor(gameUnit.gameObject);
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

    private void DetachFromAnchor()
    {
        if (anchor == null) return;
        Debug.Log("Detaching from anchor...");
        gameUnit.transform.localScale /= scaleFactor;
        gameUnit.transform.SetParent(this.transform);
        gameUnit.transform.localPosition = Vector3.zero;
        gameUnit.transform.localRotation = initRotation;
        attached = false;
        Destroy(anchor.gameObject);
        anchor = null;
        Debug.LogWarning("Unanchored lossy scale: " + gameUnit.transform.lossyScale);
    }

    IEnumerator SetAnchorCooldown()
    {
        inAnchorCooldown = true;
        yield return new WaitForSeconds(0.5f);
        inAnchorCooldown = false;
    }

    IEnumerator EnableVisualizationWhenTracked()
    {
        while (trackable.trackingState != TrackingState.Tracking) yield return null;
        if(gameUnit != null) gameUnit.gameObject.SetActive(true);
    }
}
