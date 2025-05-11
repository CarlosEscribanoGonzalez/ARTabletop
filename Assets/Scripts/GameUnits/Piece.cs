using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class Piece : AGameUnit
{
    [SerializeField] private TMP_InputField pieceName;
    private PieceGameManager manager;
    public int Index { get; set; } = -1;

    private void Start()
    {
        manager = FindFirstObjectByType<PieceGameManager>();
        manager.OnNameChanged += (() => SetName(manager.Names[Index]));
        RequestInfo(manager);
    }

    protected override void AdjustModelSize()
    {
        base.AdjustModelSize();
        unitCollider.gameObject.AddComponent<PieceNameToggler>();
    }

    public void RequestNameChange(string name)
    {
        manager.ChangeNameServerRpc(Index, name);
    }

    public void ToggleName()
    {
        pieceName.gameObject.SetActive(!pieceName.gameObject.activeSelf);
    }

    private void SetName(string name)
    {
        pieceName.SetTextWithoutNotify(name); 
    }
}
