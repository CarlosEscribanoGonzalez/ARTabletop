using UnityEngine;
using TMPro;

public class Piece : AGameUnit
{
    [SerializeField] private TMP_InputField pieceName; //Input Field editable que muestra el nombre del jugador de la pieza
    private PieceGameManager manager; //Manager de las piezas
    public int Index { get; set; } = -1; //Índice de la pieza, usado por el manager.

    private void Start()
    {
        manager = FindFirstObjectByType<PieceGameManager>();
        manager.OnNameChanged += (() => SetName(manager.Names[Index]));
        RequestInfo(manager); //Al ser escaneada por primera vez la pieza le pide la información al manager
        if(!GameSettings.Instance.IsOnline) SetName(manager.Names[Index]); //Si la partida es offline directamente se pone el nombre por defecto
    }

    private void OnEnable()
    {
        //Si es online, le pide el nombre al host cada vez que la ficha aparece para que este siempre esté sincronizado
        if (GameSettings.Instance.IsOnline && manager != null) manager.RequestNameServerRpc(Index); 
    }

    protected override void AdjustModelSize() 
    {
        base.AdjustModelSize();
        //Además de ajustar el tamaño del modelo se encarga de agregar el componente PieceNameToggler al collider, para que el jugador pueda hacer toggle del nombre pulsando la pieza
        unitCollider.gameObject.AddComponent<PieceNameToggler>();
        MeshFilter mesh = unitModel.GetComponentInChildren<MeshFilter>();
        if (mesh != null) //También ajusta la altura del Input Field del nombre para que siempre aparezca a una distancia razonable del modelo
        {
            float offsetY = mesh.sharedMesh.bounds.extents.y + mesh.sharedMesh.bounds.size.y; //Altura de un objeto y medio
            offsetY *= mesh.transform.lossyScale.y; //Se tiene en cuenta la escala
            pieceName.transform.parent.position += new Vector3(0, offsetY, 0); //Se mueve el canvas a la altura deseada
        }
    }

    public void OnNameEditionEnter(string _) //Cuando el input field es activado 
    {
        InForceMaintain = true; //La ficha no debería desaparecer mientras se le pone un nombre nuevo
    }

    public void RequestNameChange(string name) //Cuando el input field es desactivado
    {
        if (GameSettings.Instance.IsOnline) manager.ChangeNameServerRpc(Index, name); //Si es online cambia el nombre. Si no, no hace falta hacer nada más
        InForceMaintain = false;
    }

    public void ToggleName() //Al ser pulsado el modelo (PieceNameToggler) se le hace toggle al nombre
    {
        pieceName.gameObject.SetActive(!pieceName.gameObject.activeSelf);
    }

    private void SetName(string name) //Actualiza el valor del InputField para mostrar un nuevo nombre
    {
        pieceName.SetTextWithoutNotify(name); 
    }
}
