using UnityEngine;
using TMPro;
using UnityEngine.XR.ARFoundation;

public class Card : MonoBehaviour
{
    [SerializeField] private TextMeshPro text; //Texto de la carta
    [SerializeField] private SpriteRenderer spriteRend; //Visuales de la carta
    [SerializeField] private GameObject buttonCanvas; //Canvas que contiene el bot�n de cambio de contenido, para las cartas especiales
    [SerializeField] private Sprite defaultSprite; //Sprite por defecto en caso de que no haya ninguno asociado a la informaci�n a mostrar
    private SpecialCardGameManager specialCardManager; //Manager encargado de gestionar la carta en caso de que sea especial

    private void Start()
    {
        ARTrackedImage trackedImg = GetComponentInParent<ARTrackedImage>();
        if (trackedImg.referenceImage.name.ToLower().Contains("special")) //Si es especial se obtiene su manager
        {
            specialCardManager = GameSettings.Instance.GetSpecialCardManager(trackedImg.referenceImage.name);
            if (specialCardManager is null || !specialCardManager.ProvideInfo(this)) //Si no se ha conseguido la informaci�n se despliega el indicador
            {
                GetComponentInParent<PlayableUnit>().DisplayNoInfoIndicator();
            }
            else buttonCanvas.SetActive(true); //Si se ha conseguido se activa el bot�n
        }
        else if (!FindFirstObjectByType<CardGameManager>().ProvideInfo(this)) //Si no es carta especial simplemente se obtiene la info
            GetComponentInParent<PlayableUnit>().DisplayNoInfoIndicator();
    }

    private void Update() //Se actualiza el bot�n para que siempre mire a c�mara
    {
        if (!buttonCanvas.activeSelf) return;
        buttonCanvas.transform.forward = buttonCanvas.transform.position - Camera.main.transform.position;
    }

    public void SetText(string t) //Se actualiza el texto
    {
        text.text = t;
    }

    public void SetSprite(Sprite s) //Se actualiza el sprite
    {
        spriteRend.sprite = s ?? defaultSprite;
    }

    public void ChangeContent() //Se actualiza el contenido cuando el bot�n de las cartas especiales es pulsado
    {
        specialCardManager.UpdateCard();
    }

    Vector2 desiredTextureSize = new Vector2(640, 896); //Tama�o deseado de la textura, para que cuadre con la carta cuando su multiplicador sea 1
    float spriteScaleMult = 0; //Multiplica el tama�o del sprite �nicamente para que se ajuste a todo el espacio posible
    float prevSizeMult = 0; //Anterior multiplicador del tama�o de la carta entera, almacenado en caso de que se tenga que resetear el tama�o
    bool scaled = false; //Determina si la carta ha sido escalada ya o no
    public void SetSize(float sizeMult, bool resetScale = false) //Se ajusta el tama�o para que se visualice bien la carta
    {
        if (resetScale) //Las cartas especiales necesitan resetear su escala antes de cambiar de contenido
        {
            scaled = false;
            spriteRend.transform.localScale /= (spriteScaleMult * prevSizeMult);
            GetComponentInChildren<SpriteMask>().transform.localScale /= prevSizeMult;
            text.transform.localScale /= prevSizeMult;
        }
        
        if (scaled) return; //Coger la escala original no funciona porque es un n�mero peque�o y en la build cuenta como 0
        scaled = true;
        prevSizeMult = sizeMult;
        //El tama�o del texto y el sprite se ajustan autom�ticamente
        GetComponentInChildren<SpriteMask>().transform.localScale *= sizeMult;
        text.transform.localScale *= sizeMult;
        //Si la foto no cuenta con textura tendr� el mismo tama�o que el sprite mask que permite su visualizaci�n
        if(spriteRend.sprite.texture is null)
        {
            spriteRend.transform.localScale *= sizeMult;
            spriteScaleMult = 1;
            return;
        }
        //Si cuenta con textura el tama�o del sprite se ajusta para que su contenido se vea completamente
        if (spriteRend.sprite.texture.width > spriteRend.sprite.texture.height) 
            spriteScaleMult = desiredTextureSize.x / spriteRend.sprite.texture.width;
        else spriteScaleMult = desiredTextureSize.y / spriteRend.sprite.texture.height;
        spriteRend.transform.localScale *= spriteScaleMult * sizeMult;
    }
}
