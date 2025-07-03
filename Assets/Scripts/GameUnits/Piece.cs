using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Piece : AGameUnit
{
    [SerializeField] private TMP_InputField pieceName; //Input Field editable que muestra el nombre del jugador de la pieza
    private PieceGameManager manager; //Manager de las piezas
    private Color randomColor; //Color aleatorio en caso de que las default pieces estén configuradas para la distinción de colores
    private Dictionary<Material, Color> originalColors = new();
    public int Index { get; set; } = -1; //Índice de la pieza, usado por el manager.

    private void Start()
    {
        manager = FindFirstObjectByType<PieceGameManager>();
        manager.OnNameChanged += (() => SetName(manager.Names[Index]));
        RequestInfo(manager); //Al ser escaneada por primera vez la pieza le pide la información al manager
        if (Index == -1) return;
        if (!GameSettings.Instance.IsOnline) SetName(manager.Names[Index]); //Si la partida es offline directamente se pone el nombre por defecto
        pieceName.GetComponentInParent<Canvas>().worldCamera = GameObject.FindWithTag("SecondCam").GetComponent<Camera>();
        FindFirstObjectByType<Settings>().OnColorSettingChanged += ManageColorDifferentiation;
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
        int randomSeed = (int)(Random.value * Index * 9973);
        System.Random rand = new System.Random(randomSeed);
        randomColor = new Color((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1);
        ManageColorDifferentiation(null, FindFirstObjectByType<Settings>().IsRandomColorEnabled);
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

    private void ManageColorDifferentiation(object s, bool activateRandomColor)
    {
        if (manager.Pieces[Index] != manager.DefaultPiece) return;
        foreach (Renderer rend in unitModel.GetComponentsInChildren<Renderer>())
        {
            foreach (Material mat in rend.materials)
            {
                if (!originalColors.ContainsKey(mat)) originalColors.Add(mat, mat.color);
                mat.color = activateRandomColor ? randomColor : originalColors[mat];
            }
        }
    }
}
