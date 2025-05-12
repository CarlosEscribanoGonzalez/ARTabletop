using UnityEngine;
using TMPro;

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
        MeshFilter mesh = unitModel.GetComponentInChildren<MeshFilter>();
        if (mesh != null)
        {
            float offsetY = mesh.sharedMesh.bounds.extents.y + mesh.sharedMesh.bounds.size.y; //Altura de un objeto y medio
            offsetY *= mesh.transform.lossyScale.y; //Se tiene en cuenta la escala
            pieceName.transform.parent.position += new Vector3(0, offsetY, 0);
        }
    }

    public void OnNameEditionEnter(string _) //Cuando el input field es activado 
    {
        InForceMaintain = true;
    }

    public void RequestNameChange(string name) //Cuando el input field es desactivado
    {
        manager.ChangeNameServerRpc(Index, name);
        InForceMaintain = false;
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
