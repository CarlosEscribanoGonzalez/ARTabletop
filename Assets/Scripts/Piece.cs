using UnityEngine;

public class Piece : MonoBehaviour
{
    [SerializeField] private float maxSize = 0.2f;
    private GameObject pieceModel;
    private Collider pieceCollider;

    private void OnTriggerEnter(Collider other)
    {
        if(pieceCollider == null) pieceCollider = pieceModel.GetComponent<Collider>();
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
        if (pieceModel.GetComponentInChildren<Collider>() == null) pieceCollider = pieceModel.AddComponent<BoxCollider>();
        if (pieceModel.GetComponentInChildren<Rigidbody>() == null) pieceModel.AddComponent<Rigidbody>();
        pieceModel.GetComponent<Rigidbody>().isKinematic = true;
        pieceModel.GetComponent<Rigidbody>().useGravity = false;
    }

    private void AdjustModelSize()
    {
        MeshFilter mesh = pieceModel.GetComponentInChildren<MeshFilter>();
        if (mesh != null)
        {
            float scaleFactor = maxSize / mesh.sharedMesh.bounds.size.magnitude;
            pieceModel.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        } 
        else Destroy(pieceModel);
    }
}
