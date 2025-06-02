using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class AGameUnit : MonoBehaviour
{
    [SerializeField] private float maxSize = 0.2f; //Tama�o m�ximo que puede tener la GameUnit
    protected GameObject unitModel; //Modelo de la unidad
    protected Collider unitCollider; //Collider de la unidad
    protected SpriteRenderer spriteRend; //Renderer de sprites de la unidad
    protected Vector2 desiredTextureSize = new Vector2(64, 64); //Tama�o de la textura deseado
    protected float spriteScaleMult = 0; //Multiplicador del tama�o del sprite para que se ajuste al m�ximo espacio posible
    private bool inForceMaintain = false; //Indica que el objeto debe mantenerse aunque su marcador no est� siendo trackeado
    public bool InForceMaintain { get { return inForceMaintain; }
        set
        {
            inForceMaintain = value;
            //Si el setter lo pone en falso verifica en ese momento su tracking state y la configuraci�n de Extended Tracking
            ARTrackedImage trackable = GetComponentInParent<ARTrackedImage>();
            if (!inForceMaintain && !ExtendedTrackingManager.isXTEnabled && trackable.trackingState != TrackingState.Tracking)
                trackable.gameObject.SetActive(false); //Si ya no est� trackeado y no hay extended tracking se desactiva
        }
    }

    private void Awake()
    {
        spriteRend = GetComponentInChildren<SpriteRenderer>();
    }

    protected void RequestInfo(IGameManager manager) //Le pide la informaci�n al manager correspondiente
    {
        if (!manager.ProvideInfo(this)) //Si encuentra informaci�n se aplica
            GetComponentInParent<PlayableUnit>().DisplayNoInfoIndicator(); //Si no la encuentra se activa el indicador
    }

    private void OnTriggerEnter(Collider other) //La posici�n de los modelos se ajusta autom�ticamente para que est�n encima del tablero siempre
    {
        if (unitCollider == null) return;
        else if (other == unitCollider) //Las GameUnits cuentan con un collider que act�a como "base" sobre la que colocar el modelo con unitCollider
        {
            unitModel.transform.localPosition += transform.up * 0.005f;
            //Se desactiva y reactiva el collider para volver a detectar la colisi�n, en caso de que haya que subir m�s la unidad
            unitCollider.enabled = false;
            unitCollider.enabled = true;
        }
    }

    public virtual void SetModel(GameObject model) //Instancia el modelo y ajusta su tama�o
    {
        unitModel = Instantiate(model, this.transform);
        unitModel.SetActive(true);
        AdjustModelSize();
    }

    public virtual void SetSprite(Sprite s) //Se aplica el sprite y se ajusta su tama�o
    {
        spriteRend.sprite = s;
        AdjustSpriteSize();
    }

    protected virtual void AdjustModelSize() //Se ajusta el tama�o del modelo para que su magnitud no sea mayor que maxSize
    {
        MeshFilter mesh = unitModel.GetComponentInChildren<MeshFilter>();
        if (mesh != null)
        {
            float scaleFactor = maxSize / mesh.sharedMesh.bounds.size.magnitude; //Se calcula el factor de escala
            unitModel.transform.localScale = Vector3.one * scaleFactor; //Todos los objetos as� tienen la misma magnitud
            //Se le dan al modelo los componentes necesarios para calcular su posici�n en Y con OnTriggerEnter
            //Si no tiene collider se le asigna se pone en trigger
            if (unitModel.GetComponentInChildren<Collider>() == null) unitCollider = mesh.gameObject.AddComponent<BoxCollider>();
            else unitCollider = unitModel.GetComponentInChildren<Collider>();
            unitCollider.isTrigger = true;
            //Si no tiene rigidbody se le asigna y se pone en kinem�tico y sin usar gravedad
            if (unitModel.GetComponentInChildren<Rigidbody>() == null) mesh.gameObject.AddComponent<Rigidbody>();
            unitModel.GetComponentInChildren<Rigidbody>().isKinematic = true;
            unitModel.GetComponentInChildren<Rigidbody>().useGravity = false;
        }
    }

    protected virtual void AdjustSpriteSize() //Ajusta el sprite al tama�o deseado
    {
        //Se tiene en cuenta si es m�s ancho o alto para calcular el factor de escala
        if (spriteRend.sprite.texture.width > spriteRend.sprite.texture.height) 
            spriteScaleMult = desiredTextureSize.x / spriteRend.sprite.texture.width;
        else spriteScaleMult = desiredTextureSize.y / spriteRend.sprite.texture.height;
        spriteRend.transform.localScale *= spriteScaleMult;
    }
}
