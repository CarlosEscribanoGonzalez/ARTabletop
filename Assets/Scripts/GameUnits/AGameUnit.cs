using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class AGameUnit : MonoBehaviour
{
    [SerializeField] private float maxSize = 0.2f; //Tamaño máximo que puede tener la GameUnit
    protected GameObject unitModel; //Modelo de la unidad
    protected Collider unitCollider; //Collider de la unidad
    protected SpriteRenderer spriteRend; //Renderer de sprites de la unidad
    protected Vector2 desiredTextureSize = new Vector2(50, 50); //Tamaño de la textura deseado
    protected float spriteScaleMult = 0; //Multiplicador del tamaño del sprite para que se ajuste al máximo espacio posible
    protected float ratio = 1;
    protected float desiredRatio;
    private bool inForceMaintain = false; //Indica que el objeto debe mantenerse aunque su marcador no esté siendo trackeado
    public bool InForceMaintain { get { return inForceMaintain; }
        set
        {
            inForceMaintain = value;
            //Si el setter lo pone en falso verifica en ese momento su tracking state y la configuración de Extended Tracking
            ARTrackedImage trackable = GetComponentInParent<ARTrackedImage>();
            if (!inForceMaintain && !ExtendedTrackingManager.IsXTEnabled && trackable.trackingState != TrackingState.Tracking)
                trackable.gameObject.SetActive(false); //Si ya no está trackeado y no hay extended tracking se desactiva
        }
    }

    private void Awake()
    {
        spriteRend = GetComponentInChildren<SpriteRenderer>();
    }

    protected void RequestInfo(IGameManager manager) //Le pide la información al manager correspondiente
    {
        if (!manager.ProvideInfo(this)) //Si encuentra información se aplica
            GetComponentInParent<PlayableUnit>().DisplayNoInfoIndicator(); //Si no la encuentra se activa el indicador
    }

    private void OnTriggerEnter(Collider other) //La posición de los modelos se ajusta automáticamente para que estén encima del tablero siempre
    {
        if (unitCollider == null) return;
        else if (other == unitCollider) //Las GameUnits cuentan con un collider que actúa como "base" sobre la que colocar el modelo con unitCollider
        {
            unitModel.transform.position += unitModel.transform.TransformDirection(Vector3.up) * 0.005f; 
            //Se desactiva y reactiva el collider para volver a detectar la colisión, en caso de que haya que subir más la unidad
            unitCollider.enabled = false;
            unitCollider.enabled = true;
        }
    }

    public virtual void SetModel(GameObject model) //Instancia el modelo y ajusta su tamaño
    {
        unitModel = Instantiate(model, this.transform);
        unitModel.SetActive(true);
        foreach (Transform t in unitModel.GetComponentsInChildren<Transform>())
            t.gameObject.layer = this.gameObject.layer;
        AdjustModelSize();
    }

    public virtual void SetSprite(Sprite s) //Se aplica el sprite y se ajusta su tamaño
    {
        spriteRend.sprite = s;
        AdjustSpriteSize();
    }

    protected virtual void AdjustModelSize() //Se ajusta el tamaño del modelo para que su magnitud no sea mayor que maxSize
    {
        float scaleFactor = ContentScaler.ScaleModel(unitModel, maxSize);
        unitModel.transform.localScale = Vector3.one * scaleFactor; //Todos los objetos así tienen la misma magnitud
        //Se le dan al modelo los componentes necesarios para calcular su posición en Y con OnTriggerEnter
        //Si no tiene collider se le asigna se pone en trigger
        if (unitModel.GetComponentInChildren<Collider>() == null)
        {
            MeshFilter[] meshes = unitModel.GetComponentsInChildren<MeshFilter>();
            MeshFilter biggestMesh = null;
            float biggestSize = 0;
            foreach(var mesh in meshes)
            {
                if(mesh.sharedMesh.bounds.size.magnitude > biggestSize)
                {
                    biggestMesh = mesh;
                    biggestSize = mesh.sharedMesh.bounds.size.magnitude;
                }
            }
            unitCollider = biggestMesh.gameObject.AddComponent<BoxCollider>();
        }
        else unitCollider = unitModel.GetComponentInChildren<Collider>();
        unitCollider.isTrigger = true;
    }

    protected virtual void AdjustSpriteSize() //Ajusta el sprite al tamaño deseado
    {
        if(desiredRatio == default) desiredRatio = desiredTextureSize.x / desiredTextureSize.y;
        ratio = (float)spriteRend.sprite.texture.width / (float)spriteRend.sprite.texture.height;
        //Se tiene en cuenta si es más ancho o alto para calcular el factor de escala
        if (ratio > desiredRatio) 
            spriteScaleMult = desiredTextureSize.x / spriteRend.sprite.texture.width;
        else spriteScaleMult = desiredTextureSize.y / spriteRend.sprite.texture.height;
        spriteRend.transform.localScale *= spriteScaleMult;
    }
}
