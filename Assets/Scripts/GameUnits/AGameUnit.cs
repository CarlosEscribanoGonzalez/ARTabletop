using UnityEngine;

public class AGameUnit : MonoBehaviour
{
    [SerializeField] private float maxSize = 0.2f; //Tamaño máximo que puede tener la GameUnit
    protected GameObject unitModel; //Modelo de la unidad
    protected Collider unitCollider; //Collider de la unidad
    protected SpriteRenderer spriteRend; //Renderer de sprites de la unidad
    protected Vector2 desiredTextureSize = new Vector2(64, 64); //Tamaño de la textura deseado
    protected float spriteScaleMult = 0; //Multiplicador del tamaño del sprite para que se ajuste al máximo espacio posible

    private void Awake()
    {
        spriteRend = GetComponentInChildren<SpriteRenderer>();
    }

    protected void RequestInfo(IGameManager manager)
    {
        if (!manager.ProvideInfo(this)) //Si encuentra información se aplica
            GetComponentInParent<PlayableUnit>().DisplayNoInfoIndicator(); //Si no se activa el indicador
    }

    private void OnTriggerEnter(Collider other) //La posición de las fichas se ajusta automáticamente para que estén encima del tablero siempre
    {
        if (unitCollider == null) return;
        if (other == unitCollider)
        {
            unitModel.transform.localPosition += transform.up * 0.005f;
            //Se desactiva y reactiva el collider para volver a detectar la colisión, en caso de que haya que subir más la unidad
            unitCollider.enabled = false;
            unitCollider.enabled = true;
        }
    }

    public virtual void SetModel(GameObject model) //Instancia el modelo y ajusta su tamaño
    {
        if (unitModel != null) return;
        unitModel = Instantiate(model, this.transform);
        AdjustModelSize();
    }

    public virtual void SetSprite(Sprite s) //Se aplica el sprite y se ajusta su tamaño
    {
        spriteRend.sprite = s;
        AdjustSpriteSize();
    }

    protected virtual void AdjustModelSize() //Se ajusta el tamaño del modelo para que su magnitud no sea mayor que maxSize
    {
        MeshFilter mesh = unitModel.GetComponentInChildren<MeshFilter>();
        if (mesh != null)
        {
            float scaleFactor = maxSize / mesh.sharedMesh.bounds.size.magnitude;
            unitModel.transform.localScale = Vector3.one * scaleFactor;
            //Se le dan al modelo los componentes necesarios para calcular su posición en Y con OnTriggerEnter
            if (unitModel.GetComponentInChildren<Collider>() == null) unitCollider = mesh.gameObject.AddComponent<BoxCollider>();
            else unitCollider = unitModel.GetComponentInChildren<Collider>();
            unitCollider.isTrigger = true;
            if (unitModel.GetComponentInChildren<Rigidbody>() == null) mesh.gameObject.AddComponent<Rigidbody>();
            unitModel.GetComponentInChildren<Rigidbody>().isKinematic = true;
            unitModel.GetComponentInChildren<Rigidbody>().useGravity = false;
        }
    }

    protected virtual void AdjustSpriteSize() 
    {
        if (spriteRend.sprite.texture.width > spriteRend.sprite.texture.height)
            spriteScaleMult = desiredTextureSize.x / spriteRend.sprite.texture.width;
        else spriteScaleMult = desiredTextureSize.y / spriteRend.sprite.texture.height;
        spriteRend.transform.localScale *= spriteScaleMult;
    }
}
