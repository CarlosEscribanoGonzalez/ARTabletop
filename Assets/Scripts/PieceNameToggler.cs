using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PieceNameToggler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private float clickThreshold = 0.3f;
    private Piece piece;
    private bool isPressing = false;
    private float pressStartTime;

    private void Awake()
    {
        piece = GetComponentInParent<Piece>();
    }

    public void OnPointerDown(PointerEventData _)
    {
        isPressing = true;
        pressStartTime = Time.time;
        StartCoroutine(ToggleNameCoroutine());
    }

    public void OnPointerUp(PointerEventData _)
    {
        if (!isPressing) return;
        isPressing = false;
        if (Time.time - pressStartTime < clickThreshold) StopAllCoroutines();
    }

    public void OnPointerExit(PointerEventData _)
    {
        isPressing = false;
        StopAllCoroutines();
    }

    IEnumerator ToggleNameCoroutine()
    {
        yield return new WaitForSeconds(clickThreshold + 0.01f);
        piece.ToggleName();
    }
}
