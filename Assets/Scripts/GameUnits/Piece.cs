using UnityEngine;

public class Piece : MonoBehaviour
{
    [SerializeField] private float maxSize = 0.2f;
    private GameObject pieceModel;
    private Collider pieceCollider;

    private void Start()
    {
        if (!FindFirstObjectByType<PieceGameManager>().ProvideInfo(this))
            GetComponentInParent<PlayableUnit>().DisplayNoInfoIndicator();  
    }

    private void OnTriggerEnter(Collider other)
    {
        if (pieceCollider == null) return;
        if (other == pieceCollider)
        {
            pieceModel.transform.localPosition += transform.up * 0.005f;
            pieceCollider.enabled = false;
            pieceCollider.enabled = true;
        }
    }

    public void SetModel(GameObject model)
    {
        if (pieceModel != null) return;
        pieceModel = Instantiate(model, this.transform);
        AdjustModelSize();
    }

    private void AdjustModelSize()
    {
        MeshFilter mesh = pieceModel.GetComponentInChildren<MeshFilter>();
        if (mesh != null)
        {
            float scaleFactor = maxSize / mesh.sharedMesh.bounds.size.magnitude;
            pieceModel.transform.localScale = Vector3.one * scaleFactor;
            if (pieceModel.GetComponentInChildren<Collider>() == null) pieceCollider = mesh.gameObject.AddComponent<BoxCollider>();
            if (pieceModel.GetComponentInChildren<Rigidbody>() == null) mesh.gameObject.AddComponent<Rigidbody>();
            pieceModel.GetComponentInChildren<Rigidbody>().isKinematic = true;
            pieceModel.GetComponentInChildren<Rigidbody>().useGravity = false;
        } 
        else Destroy(pieceModel);
    }
}
