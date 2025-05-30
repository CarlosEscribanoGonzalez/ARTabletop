using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PieceNameToggler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private float clickThreshold = 0.3f; //Tiempo pulsado a partir del cual se considerar� la interacci�n
    private Piece piece; //Pieza
    private bool isPressing = false; //Booleano que indica si se est� pulsando la pieza actualmente
    private Coroutine toggleCoroutine; //Corrutina de toggle del nombre

    private void Awake()
    {
        piece = GetComponentInParent<Piece>();
    }

    public void OnPointerDown(PointerEventData _) //Se considera que la pulsaci�n ha comenzado y comienza la corrutina de Toggle del nombre
    {
        isPressing = true;
        toggleCoroutine = StartCoroutine(ToggleNameCoroutine());
    }

    public void OnPointerUp(PointerEventData _) //Si se estaba pulsando, se cancela la pulsaci�n y en caso de que la corrutina est� activa se desactiva
    {
        if (!isPressing) return;
        isPressing = false;
        if (toggleCoroutine != null)
        {
            toggleCoroutine = null; 
            StopAllCoroutines();
        }
    }

    public void OnPointerExit(PointerEventData _) //Si se arrastra el dedo fuera de la pieza mientras se pulsa la pulsaci�n se cancela
    {
        isPressing = false;
        if (toggleCoroutine != null)
        {
            toggleCoroutine = null;
            StopAllCoroutines();
        }
    }

    IEnumerator ToggleNameCoroutine() //Tras el threshold hace toggle del nombre
    {
        yield return new WaitForSeconds(clickThreshold);
        piece.ToggleName();
        toggleCoroutine = null; //Evita llamadas innecesarias a StopAllCoroutines
    }
}
