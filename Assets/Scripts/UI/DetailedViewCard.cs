using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class DetailedViewCard : MonoBehaviour
{
    [SerializeField] private Image image; //Imagen de la carta
    [SerializeField] private TextMeshProUGUI textMesh; //Texto de la carta
    [SerializeField] private float interpolationSpeed; //Velocidad a la que la carta hace la animaci�n de salir de pantalla
    [SerializeField] private float swipThreshold = 4; //Threshold que modifica lo que hay que arrastrar la detailed card para realizar una acci�n (a mayor valor m�s hay que arrastrar)
    [SerializeField] private Vector2 maskVerticalPadding = new Vector2(0.2f, 0.066f); //Padding superior e inferior que afecta a la detailed card, modifica el porcentaje de pantalla que ocupa
    [SerializeField] private Sprite defaultSprite; //Sprite por default si el CardInfo asociado no tiene sprite
    [SerializeField] private bool isDetailedCard = false; //Indica si la carta es la usada para la vista detallada o no
    private RectTransform rectTransform; //RectTransform del objeto (en espec�fico, de la Mask)
    private ScreenOrientation prevOrientation; //Orientaci�n almacenada del dispositivo
    private ScrollRect scrollRect; //Scroll rect de las cartas mantenidas
    private DetailedViewCardManager manager; //Manager de la vista detallada
    private bool isDragging = false; //Booleano que indica si se est� arrastrando la carta o no
    public CardInfo Info { get; private set; } = null; //Info desplegada por la carta

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>(); 
        scrollRect = GetComponentInParent<ScrollRect>();
        manager = FindFirstObjectByType<DetailedViewCardManager>();
        prevOrientation = Screen.orientation;
    }

    private void Update()
    {
        if (!isDetailedCard) return;
        if (Screen.orientation != prevOrientation) //El size de la DetailedCard cambia seg�n la orientaci�n de la pantalla
        {
            AdjustSize();
            prevOrientation = Screen.orientation;
        }
        //La escala de la imagen var�a, disminuyendo cuanto m�s lejos se encuentra de su punto original
        float scale = Mathf.Clamp01(1 - Mathf.Abs((rectTransform.anchoredPosition.y - image.rectTransform.anchoredPosition.y) / Screen.height*2) + 0.1f);
        image.rectTransform.localScale = new Vector3(scale, scale, scale);
    }

    public void SetInfo(CardInfo info) //Actualiza la informaci�n de la carta
    {
        image.sprite = info.sprite ?? defaultSprite; //Si no tiene sprite pone el por defecto
        textMesh.text = info.text;
        Info = info; //Se guarda la informaci�n
        AdjustSize();
    }

    public void OnBeginDrag(BaseEventData data)
    {
        if (isDetailedCard) image.transform.parent = rectTransform.parent; //Si es la carta detallada se desacopla de la m�scara para que se pueda seguir viendo al ser arrastrada
        else scrollRect.OnBeginDrag((PointerEventData) data); //Si no, se propaga el inicio del drag al scrollRect
        isDragging = true;
    }

    public void OnDrag(BaseEventData data)
    {
        PointerEventData pointerData = (PointerEventData)data;
        if(isDetailedCard) image.rectTransform.anchoredPosition += new Vector2(0, pointerData.delta.y); //Si es detailedCard se modifica su posici�n en y
        else scrollRect.OnDrag(pointerData); //Si no, se propaga el drag al scrollRect
    }

    public void OnEndDrag(BaseEventData data)
    {
        if (isDetailedCard) //Si es detailed card determina si la carta ha sido swipeada hacia arriba o hacia abajo
        {
            float heightTarget; //Altura a la que se mover�
            MovementType action = MovementType.None; //Acci�n que realizar�
            float offset = image.rectTransform.anchoredPosition.y - rectTransform.anchoredPosition.y; //Distancia entre el punto original y la posici�n actual
            //El criterio para determinar es ver si ha arrastrado una distancia correspondiente a "un d�cimo de la altura de la pantalla * threshold"
            if (offset > Screen.height / 10 * swipThreshold) //Si la distancia supera el threshold por arriba la carta se descarta 
            {
                heightTarget = Screen.height * 0.75f;
                action = MovementType.Remove;
            }
            else if (offset < -(Screen.height / 10 * swipThreshold)) //Si supera el threshold por abajo la carta se mantiene
            {
                heightTarget = -Screen.height * 0.5f;
                action = MovementType.Add;
            }
            else heightTarget = (int)rectTransform.anchoredPosition.y; //Si no supera el threshold la carta vuelve a su posici�n inicial
            StartCoroutine(MoveToHeight(heightTarget, action)); //El movimiento autom�tico de la carta comienza
        }
        else scrollRect.OnEndDrag((PointerEventData)data); //Si no se comunica al scroll rect que el drag ha terminado
        isDragging = false;
    }

    public void OnClick(BaseEventData _)
    {
        if(!isDetailedCard && !isDragging) manager.SetDetailedInfo(Info); //Si una carta guardada se clica sin arrastrar su info pasa a DetailedView
    }

    private void AdjustSize() //Se ajusta el tama�o de la carta autom�ticamente para que su sprite ocupe el m�ximo espacio posible
    {
        if (isDetailedCard) //En el caso de la detailedCard hay que ajustar tambi�n el tama�o de su m�scara
        {
            rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, Screen.height * maskVerticalPadding[0]);
            rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, -Screen.height * maskVerticalPadding[1]);
        }
        image.rectTransform.sizeDelta = ContentScaler.ScaleImage(image.sprite.texture, rectTransform.rect);
    }

    IEnumerator MoveToHeight(float desiredHeight, MovementType action) //Corrutina que mueve autom�ticamente la carta hacia su objetivo
    {
        float currentY;
        float speed = action == MovementType.None ? interpolationSpeed : interpolationSpeed * 2; //Cuando se guarda o descarta una carta la velocidad es el doble (motivos est�ticos)
        while (Mathf.Abs(image.rectTransform.anchoredPosition.y - desiredHeight) > 5) //5 px de margen para considerar que ya est�
        {
            currentY = Mathf.MoveTowards(image.rectTransform.anchoredPosition.y, desiredHeight, speed * Time.deltaTime); 
            image.rectTransform.anchoredPosition = new Vector2(0, currentY); //Se mueve la carta a velocidad constante
            yield return null;
        }
        //Se resetean el padre y la posici�n de la detailedCard
        image.transform.parent = rectTransform; 
        image.rectTransform.anchoredPosition = Vector2.zero;
        if (action != MovementType.None) manager.ToggleView(false); //La detailed view se cierra tanto si se ha mantenido como si se ha descartado
        if (action == MovementType.Add) manager.KeepCard(Info); //Si se ha mantenido la carta es a�adida
        else if (action == MovementType.Remove) manager.RemoveCard(Info); //Si no la carta es eliminada
    }
}

public enum MovementType
{
    None,
    Add,
    Remove
}
