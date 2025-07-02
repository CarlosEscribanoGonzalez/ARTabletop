using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollFixer : MonoBehaviour
{
    //Los input field se abren autom�ticamente cuando se hace un "drag" sobre ellos,
    //haciendo que no se pueda arrastrar el scroll rect sobre el que est�n.
    //ScrollFixer es un bot�n que se sit�a sobre un InputField que propaga el arrastre y
    //permite la edici�n del input field cuando es clicado
    [SerializeField] private TMP_InputField inputField; //Input field sobre el que se sit�a el ScrollFixer
    private ScrollRect scrollRect; //Scroll rect sobre el que hacer scroll
    private CanvasGroup canvasGroup; //Canvas group dle ScrollFixer
    private bool scrolling = false;

    void Start()
    {
        scrollRect = GetComponentInParent<ScrollRect>();
        canvasGroup = GetComponent<CanvasGroup>();
        inputField.onDeselect.AddListener((_) => OnFieldDeselected());
    }

    public void BeginScroll(BaseEventData data) //Detecta el comienzo de un arrastre
    {
        PointerEventData pointerData = (PointerEventData)data;
        scrollRect.OnBeginDrag(pointerData);
        scrolling = true;
    }

    public void Scroll(BaseEventData data) //Propaga el arrastre al scrollRect
    {
        PointerEventData pointerData = (PointerEventData)data;
        scrollRect.OnDrag(pointerData);
    }

    public void EndScroll(BaseEventData data) //Detecta la finalizaci�n de un arrastre
    {
        PointerEventData pointerData = (PointerEventData)data;
        scrollRect.OnEndDrag(pointerData);
        scrolling = false;
    }

    public void OpenKeyBoard(BaseEventData data) //Cuando se clica sobre el scrollFixer se llama a esta funci�n
    {
        if (scrolling) return;
        ExecuteEvents.Execute(inputField.gameObject, data, ExecuteEvents.pointerDownHandler); //Se activa manualmente el Input Field
        canvasGroup.blocksRaycasts = false; //El ScrollFixer deja de bloquear raycast para permitir que el usuario clicke directamente sobre el Input Field para editar el texto
    }

    private void OnFieldDeselected() //Cuando el Input Field se deselecciona el ScrollFixer vuelve a ser activado
    {
        canvasGroup.blocksRaycasts = true;
    }
}
