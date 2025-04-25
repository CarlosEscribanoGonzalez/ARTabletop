using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRend; //Renderer del sprite, en caso de que sea un tablero de sprite
    [SerializeField] private float maxSize = 2f; //M�ximo tama�o del tablero, en caso de que sea un modelo
    private GameObject boardModel; //GameObject con el modelo del tablero
    private Collider boardCollider; //Collider del tablero

    private void Start()
    {
        if (!FindFirstObjectByType<BoardGameManager>().ProvideInfo(this)) //Si encuentra informaci�n se aplica
            GetComponentInParent<PlayableUnit>().DisplayNoInfoIndicator(); //Si no se activa el indicador 
    }

    private void OnTriggerEnter(Collider other) //Igual que las piezas, el tablero autoajusta su posici�n en Y
    {
        if (boardCollider == null) return;
        if (other == boardCollider)
        {
            boardModel.transform.localPosition += transform.up * 0.05f;
            boardCollider.enabled = false;
            boardCollider.enabled = true;
        }
    }

    public void SetSprite(Sprite s) //Se aplica el sprite y se ajusta su tama�o
    {
        spriteRend.sprite = s;
        AdjustSpriteSize();
    }

    public void SetModel(GameObject model) //Se aplica el modelo y se ajusta su tama�o
    {
        if (boardModel != null) return;
        boardModel = Instantiate(model, this.transform);
        AdjustModelSize();
    }

    Vector2 desiredTextureSize = new Vector2(64, 64); //Tama�o de la textura deseado
    float scaleMult = 0; //Multiplicador del tama�o del sprite para que se ajuste al m�ximo espacio posible
    private void AdjustSpriteSize() //Se ajusta el tama�o del sprite igual que se hace con las cartas
    {
        if (spriteRend.sprite.texture.width > spriteRend.sprite.texture.height)
            scaleMult = desiredTextureSize.x / spriteRend.sprite.texture.width;
        else scaleMult = desiredTextureSize.y / spriteRend.sprite.texture.height;
        spriteRend.transform.localScale = Vector3.one * scaleMult;
    }

    private void AdjustModelSize() //Se ajusta el tama�o del modelo igual que se hace con las piezas
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
    }
}
