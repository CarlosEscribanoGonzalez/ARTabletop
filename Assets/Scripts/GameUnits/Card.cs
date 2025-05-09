using UnityEngine;
using TMPro;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;

public class Card : AGameUnit, IPointerDownHandler
{
    [SerializeField] private TextMeshPro text; //Texto de la carta
    [SerializeField] private GameObject buttonCanvas; //Canvas que contiene el botón de cambio de contenido, para las cartas especiales
    [SerializeField] private Sprite defaultSprite; //Sprite por defecto en caso de que no haya ninguno asociado a la información a mostrar
    private SpecialCardGameManager specialCardManager; //Manager encargado de gestionar la carta en caso de que sea especial
    private SortingGroup[] sortingGroups;
    private bool IsSpecial => specialCardManager != null;
    public GameObject PrevButton { get; private set; } = null; //Botón de volver atrás para las cartas especiales

    private void Start()
    {
        ARTrackedImage trackedImg = GetComponentInParent<ARTrackedImage>();
        desiredTextureSize = new Vector2(640, 896);
        if (trackedImg.referenceImage.name.ToLower().Contains("special")) //Si es especial se obtiene su manager
        {
            foreach (Transform t in buttonCanvas.transform) if (t.name.ToLower().Contains("prev")) PrevButton = t.gameObject;
            specialCardManager = GameSettings.Instance.GetSpecialCardManager(trackedImg.referenceImage.name, this);
            if (specialCardManager is null) GetComponentInParent<PlayableUnit>().DisplayNoInfoIndicator();
            else
            {
                buttonCanvas.SetActive(true); //Si se ha conseguido se activa el botón
                RequestInfo(specialCardManager);    
            }
        }
        else RequestInfo(FindFirstObjectByType<CardGameManager>());
        sortingGroups = GetComponentsInChildren<SortingGroup>(true);
    }

    private void Update() //Se actualiza el botón para que siempre mire a cámara
    {
        for (int i = 0; i < sortingGroups.Length; i++)
        {
            sortingGroups[i].sortingOrder = 1000 - (int)(Vector3.Distance(this.transform.position, Camera.main.transform.position) * 1000) + i;
        }
        if (!buttonCanvas.activeSelf) return;
        buttonCanvas.transform.forward = buttonCanvas.transform.position - Camera.main.transform.position;
    }

    public void SetText(string t) //Se actualiza el texto
    {
        text.text = t;
    }

    public override void SetSprite(Sprite s) //Se actualiza el sprite
    {
        spriteRend.sprite = s ?? defaultSprite;
    }

    public void ChangeContent(bool returnToPrevious) //Se actualiza el contenido cuando el botón de las cartas especiales es pulsado
    {
        specialCardManager.UpdateCard(returnToPrevious ? -1 : 1);
    }

    public void RequestShuffle()
    {
        specialCardManager.RequestShuffleServerRpc();
    }

    float prevSizeMult = 0; //Anterior multiplicador del tamaño de la carta entera, almacenado en caso de que se tenga que resetear el tamaño
    bool scaled = false; //Determina si la carta ha sido escalada ya o no
    public void SetSize(float sizeMult, bool resetScale = false) //Se ajusta el tamaño para que se visualice bien la carta
    {
        if (resetScale) //Las cartas especiales necesitan resetear su escala antes de cambiar de contenido
        {
            scaled = false;
            spriteRend.transform.localScale /= spriteScaleMult;
            transform.localScale /= prevSizeMult;
        }
        
        if (scaled) return; //Coger la escala original no funciona porque es un número pequeño y en la build cuenta como 0
        scaled = true;
        prevSizeMult = sizeMult;
        //El tamaño del objeto se ajusta automáticamente
        transform.localScale *= sizeMult;
        //Si la foto no cuenta con textura tendrá el mismo tamaño que el sprite mask que permite su visualización
        if (spriteRend.sprite.texture is null) spriteScaleMult = 1;
        //Si cuenta con textura el tamaño del sprite se ajusta para que su contenido se vea completamente
        else AdjustSpriteSize();
    }

    public void OnPointerDown(PointerEventData data)
    {
        if (IsSpecial) buttonCanvas.SetActive(!buttonCanvas.activeSelf);
    }
}
