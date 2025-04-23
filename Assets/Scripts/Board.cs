using UnityEditor;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRend;
    [SerializeField] private float maxSize = 2f;
    private float originalScale; 
    private GameObject boardModel;
    private Collider boardCollider;

    private void Awake()
    {
        originalScale = spriteRend.transform.localScale.x;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (boardCollider == null) return;
        if (other == boardCollider)
        {
            boardModel.transform.localPosition += transform.up * 0.05f;
            boardCollider.enabled = false;
            boardCollider.enabled = true;
        }
    }

    public void SetSprite(Sprite s)
    {
        spriteRend.sprite = s;
        AdjustSpriteSize();
    }

    public void SetModel(GameObject model)
    {
        if (boardModel != null) return;
        boardModel = Instantiate(model, this.transform);
        AdjustModelSize();
    }

    Vector2 desiredTextureSize = new Vector2(1792, 1792);
    float scaleMult = 0;
    private void AdjustSpriteSize()
    {
        if (spriteRend.sprite.texture.width > spriteRend.sprite.texture.height)
            scaleMult = desiredTextureSize.x / spriteRend.sprite.texture.width;
        else scaleMult = desiredTextureSize.y / spriteRend.sprite.texture.height;
        spriteRend.transform.localScale = new Vector3(originalScale * scaleMult, originalScale * scaleMult, originalScale * scaleMult);
    }

    private void AdjustModelSize()
    {
        MeshFilter mesh = boardModel.GetComponentInChildren<MeshFilter>();
        if (mesh != null)
        {
            float scaleFactor = maxSize / mesh.sharedMesh.bounds.size.magnitude;
            boardModel.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
            if (boardModel.GetComponentInChildren<Collider>() == null) boardCollider = mesh.gameObject.AddComponent<BoxCollider>();
            if (boardModel.GetComponentInChildren<Rigidbody>() == null) mesh.gameObject.AddComponent<Rigidbody>();
            boardModel.GetComponentInChildren<Rigidbody>().isKinematic = true;
            boardModel.GetComponentInChildren<Rigidbody>().useGravity = false;
        }
        else Destroy(boardModel);
    }
}
