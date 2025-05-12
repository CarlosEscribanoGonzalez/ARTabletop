using UnityEngine;
using TMPro;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using System.Collections;

public class Card : AGameUnit, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshPro text; //Texto de la carta
    [SerializeField] private GameObject buttonCanvas; //Canvas que contiene el botón de cambio de contenido, para las cartas especiales
    [SerializeField] private Sprite defaultSprite; //Sprite por defecto en caso de que no haya ninguno asociado a la información a mostrar
    [SerializeField] private float clickThreshold = 1; //Tiempo que se tarda en mantener pulsado para hacer toggle de los botones
    private SpecialCardGameManager specialCardManager; //Manager encargado de gestionar la carta en caso de que sea especial
    private SortingGroup[] sortingGroups;
    private DetailedViewCardManager detailedViewManager;
    private CardInfo cardInfo;
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
        detailedViewManager = FindFirstObjectByType<DetailedViewCardManager>();
    }

    private void Update() //Se fuerza a que se vean por encima siempre las cartas más cercanas a la cámara
    {
        for (int i = 0; i < sortingGroups.Length; i++)
        {
            sortingGroups[i].sortingOrder = 1000 - (int)(Vector3.Distance(this.transform.position, Camera.main.transform.position) * 1000) + i;
        }
    }

    public void SetInfo(CardInfo info, bool resetScale = false)
    {
        text.text = info.text;
        spriteRend.sprite = info.sprite ?? defaultSprite;
        SetSize(info.sizeMult, resetScale);
        cardInfo = info;
    }

    public void ChangeContent(bool returnToPrevious) //Se actualiza el contenido cuando el botón de las cartas especiales es pulsado
    {
        specialCardManager.UpdateCard(returnToPrevious ? -1 : 1);
    }

    public void RequestShuffle()
    {
        if (GameSettings.Instance.IsOnline) specialCardManager.RequestShuffleServerRpc();
        else specialCardManager.Shuffle();
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
        if (IsSpecial) buttonCanvas.transform.localScale = Vector3.one / sizeMult; //El tamaño del botón se mantiene
        //Si la foto no cuenta con textura tendrá el mismo tamaño que el sprite mask que permite su visualización
        if (spriteRend.sprite.texture is null) spriteScaleMult = 1;
        //Si cuenta con textura el tamaño del sprite se ajusta para que su contenido se vea completamente
        else AdjustSpriteSize();
    }

    private bool isPressing = false;
    private float pressStartTime;
    public void OnPointerDown(PointerEventData data)
    {
        isPressing = true;
        pressStartTime = Time.time;
        if (IsSpecial) StartCoroutine(ToggleButtonsCoroutine());
    }

    public void OnPointerUp(PointerEventData data)
    {
        if (!isPressing) return;
        isPressing = false;
        if (Time.time - pressStartTime > clickThreshold) return;
        detailedViewManager.SetDetailedInfo(cardInfo);
        if (IsSpecial) StopAllCoroutines();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPressing = false;
        if (IsSpecial) StopAllCoroutines();
    }

    IEnumerator ToggleButtonsCoroutine()
    {
        yield return new WaitForSeconds(clickThreshold + 0.2f);
        buttonCanvas.SetActive(!buttonCanvas.activeSelf);
    }
}
