using UnityEngine;
using UnityEngine.EventSystems;

public class PieceNameToggler : MonoBehaviour, IPointerClickHandler
{
    private Piece piece;

    private void Awake()
    {
        piece = GetComponentInParent<Piece>();
    }

    public void OnPointerClick(PointerEventData _)
    {
        piece.ToggleName();
    }
}
