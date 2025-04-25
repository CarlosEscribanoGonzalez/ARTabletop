using UnityEngine;

public class Piece : MonoBehaviour
{
    [SerializeField] private float maxSize = 0.2f; //Tamaño máximo que puede tener una ficha
    private GameObject pieceModel; //Modelo de la pieza
    private Collider pieceCollider; //Collider de la pieza, asociada a su modelo (pieceModel), no a este gameObject

    private void Start()
    {
        if (!FindFirstObjectByType<PieceGameManager>().ProvideInfo(this)) //Pide la información al manager
            GetComponentInParent<PlayableUnit>().DisplayNoInfoIndicator();  //Si no la obtiene se enciende el indicador
    }

    private void OnTriggerEnter(Collider other) //La posición de las fichas se ajusta automáticamente para que estén encima del tablero siempre
    {
        if (pieceCollider == null) return;
        if (other == pieceCollider)
        {
            pieceModel.transform.localPosition += transform.up * 0.005f;
            //Se desactiva y reactiva el collider para volver a detectar la colisión, en caso de que haya que subir más la pieza
            pieceCollider.enabled = false;
            pieceCollider.enabled = true;
        }
    }

    public void SetModel(GameObject model) //Instancia el modelo y ajusta su tamaño
    {
        if (pieceModel != null) return;
        pieceModel = Instantiate(model, this.transform);
        AdjustModelSize();
    }

    private void AdjustModelSize() //Se ajusta el tamaño del modelo para que su magnitud no sea mayor que maxSize
    {
        MeshFilter mesh = pieceModel.GetComponentInChildren<MeshFilter>();
        if (mesh != null)
        {
            float scaleFactor = maxSize / mesh.sharedMesh.bounds.size.magnitude;
            pieceModel.transform.localScale = Vector3.one * scaleFactor;
            //Se le dan al modelo los componentes necesarios para calcular su posición en Y con OnTriggerEnter
            if (pieceModel.GetComponentInChildren<Collider>() == null) pieceCollider = mesh.gameObject.AddComponent<BoxCollider>();
            if (pieceModel.GetComponentInChildren<Rigidbody>() == null) mesh.gameObject.AddComponent<Rigidbody>();
            pieceModel.GetComponentInChildren<Rigidbody>().isKinematic = true;
            pieceModel.GetComponentInChildren<Rigidbody>().useGravity = false;
        } 
        else Destroy(pieceModel); //Si por algún casual no hay modelo se destruye el objeto
    }
}
