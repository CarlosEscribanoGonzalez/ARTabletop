using UnityEngine;
using TMPro;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using System.Collections;

public class Card : AGameUnit, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshPro text; //Texto de la carta
    [SerializeField] private GameObject buttonCanvas; //Canvas que contiene el bot�n de cambio de contenido, para las cartas especiales
    [SerializeField] private float clickThreshold = 1; //Threshold que diferencia un click normal de "mantener pulsado"
    private IGameManager manager; //Manager encargado de gestionar la carta en caso de que sea especial
    private SortingGroup[] sortingGroups; //Sorting groups en los hijos de la carta, usados para que siempre se vea bien su contenido
    private DetailedViewCardManager detailedViewManager; //Manager encargado de la vista detallada cuando se pulsa sobre una carta
    private CardInfo cardInfo; //CardInfo asociada al objeto, proporcionada por el manager
    private bool IsSpecial => manager is SpecialCardGameManager; //Indica si la carta es especial o no
    public GameObject PrevButton { get; private set; } = null; //Bot�n de volver atr�s para las cartas especiales
    private Vector2 initTextSize;

    private void Start()
    {
        ARTrackedImage trackedImg = GetComponentInParent<ARTrackedImage>();
        desiredTextureSize = new Vector2(640, 896); //Tama�o deseado con las dimensiones de una carta de p�ker
        if (trackedImg.referenceImage.name.ToLower().Contains("dynamic")) //Si es especial se obtiene su manager
        {
            foreach (Transform t in buttonCanvas.transform) if (t.name.ToLower().Contains("prev")) PrevButton = t.gameObject;
            //GameSettings se encarga de asociar cartas especiales a sus managers correspondientes a partir de su marcador
            manager = GameSettings.Instance.GetSpecialCardManager(trackedImg.referenceImage.name, this);
            if (manager is null) GetComponentInParent<PlayableUnit>().DisplayNoInfoIndicator();
            else
            {
                buttonCanvas.SetActive(true); //Si se ha conseguido se activa el canvas de los botones
                buttonCanvas.GetComponent<Canvas>().worldCamera = GameObject.FindWithTag("SecondCam").GetComponent<Camera>();
                RequestInfo(manager); //Se le pide la info al manager 
            }
        }
        else
        {
            manager = FindFirstObjectByType<CardGameManager>();
            RequestInfo(manager); //Si es una carta normal le pide la info directamente al �nico manager
        }
        sortingGroups = GetComponentsInChildren<SortingGroup>(true);
        detailedViewManager = FindFirstObjectByType<DetailedViewCardManager>();
    }

    private void Update() //Se fuerza a que se vean por encima siempre las cartas m�s cercanas a la c�mara
    {
        for (int i = 0; i < sortingGroups.Length; i++)
        {
            //Al sumar i garantizamos adem�s que el orden de visualizaci�n dentro de la propia c�mara se mantiene
            //pues la jerarqu�a est� organizada de forma que los elementos con mayor sorting order son los que mayor �ndice tienen en el array
            sortingGroups[i].sortingOrder = 1000 - (int)(Vector3.Distance(this.transform.position, Camera.main.transform.position) * 1000) + i;
        }
    }

    public void SetInfo(CardInfo info, bool resetScale = false) //Asigna la informaci�n a la carta y la guarda
    {
        text.text = info.text;
        if(!IsSpecial) spriteRend.sprite = info.sprite ?? (manager as CardGameManager).DefaultImage;
        else spriteRend.sprite = info.sprite ?? (manager as SpecialCardGameManager).DefaultImage;
        SetSize(info.sizeMult == 0 ? 1 : info.sizeMult, resetScale);
        cardInfo = info;
    }

    public void ChangeContent(bool returnToPrevious) //Se actualiza el contenido cuando el bot�n de las cartas especiales es pulsado
    {
        (manager as SpecialCardGameManager).UpdateCard(returnToPrevious ? -1 : 1); //La misma funci�n sirve para los botones next y prev
    }

    public void RequestShuffle() //Llamada al pulsar el bot�n de shuffle
    {
        if (GameSettings.Instance.IsOnline) (manager as SpecialCardGameManager).RequestShuffleServerRpc();
        else (manager as SpecialCardGameManager).Shuffle();
    }

    float prevSizeMult = 0; //Anterior multiplicador del tama�o de la carta entera, almacenado en caso de que se tenga que resetear el tama�o
    bool scaled = false; //Determina si la carta ha sido escalada ya o no
    public void SetSize(float sizeMult, bool resetScale = false) //Se ajusta el tama�o para que se visualice bien la carta
    {
        if (resetScale) //Las cartas especiales necesitan resetear su escala antes de cambiar de contenido
        {
            scaled = false;
            spriteRend.transform.localScale /= spriteScaleMult; //Se resetea el ajuste de la carta al tama�o m�ximo
            transform.localScale /= prevSizeMult; //Se resetea el escalado de la carta entera
        }
        
        if (scaled) return; //Coger la escala original no funciona porque es un n�mero peque�o y en la build cuenta como 0
        scaled = true;
        prevSizeMult = sizeMult;
        //El tama�o del objeto se ajusta autom�ticamente
        transform.localScale *= sizeMult;
        if (IsSpecial) buttonCanvas.transform.localScale = Vector3.one / sizeMult; //El tama�o del bot�n se mantiene
        //Si la foto no cuenta con textura tendr� el mismo tama�o que el sprite mask que permite su visualizaci�n
        if (spriteRend.sprite.texture is null) spriteScaleMult = 1; //Se iguala a 1 para que resetear la escala funcione bien
        //Si cuenta con textura el tama�o del sprite se ajusta para que su contenido se vea completamente
        else AdjustSpriteSize();
    }

    private bool isPressing = false; //Determina si el usuario est� pulsando la carta
    private float pressStartTime; //Almacena el momento en el que la carta se empez� a pulsar
    public void OnPointerDown(PointerEventData data) //Cuando la carta es pulsada
    {
        isPressing = true; 
        pressStartTime = Time.time;
        if (IsSpecial) StartCoroutine(ToggleButtonsCoroutine()); //Comienza la corrutina de toggle de los botones
    }

    public void OnPointerUp(PointerEventData data) //Cuando la carta deja de ser pulsada
    {
        if (!isPressing) return;
        isPressing = false;
        if (Time.time - pressStartTime > clickThreshold) return; //Se valora si ha recibido un click o si se ha mantenido pulsada
        detailedViewManager.SetDetailedInfo(cardInfo); //Si s�lo ha recibido un click se abre la vista detallada
        if (IsSpecial) StopAllCoroutines(); //La corrutina se para si s�lo ha recibido un click, dejando que contin�e y finalice en caso contrario
    }

    public void OnPointerExit(PointerEventData eventData) //Cuando la carta es pulsada pero pierde el foco tras un arrastre
    {
        //Se cancela la interacci�n con la carta
        isPressing = false;
        if (IsSpecial) StopAllCoroutines();
    }

    protected override void AdjustSpriteSize()
    {
        base.AdjustSpriteSize();
        if (initTextSize == default) initTextSize = text.rectTransform.sizeDelta;
        float visibleWidth, visibleHeight;
        if (ratio >= 1f)
        {
            visibleWidth = initTextSize.x;
            visibleHeight = initTextSize.y / (ratio + 0.6f);
        }
        else
        {
            visibleHeight = initTextSize.y;
            visibleWidth = initTextSize.x * (ratio + 0.25f);
        }
        text.rectTransform.sizeDelta = new Vector2(visibleWidth, visibleHeight);
    }

    IEnumerator ToggleButtonsCoroutine()
    {
        yield return new WaitForSeconds(clickThreshold + 0.1f); //Deja un peque�o margen de tiempo de 0.1f 
        buttonCanvas.SetActive(!buttonCanvas.activeSelf);
    }
}
