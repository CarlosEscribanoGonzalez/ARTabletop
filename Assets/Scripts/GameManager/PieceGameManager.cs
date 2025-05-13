using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Linq;
using System.Collections.Generic;
using Unity.Netcode;

public class PieceGameManager : NetworkBehaviour, IGameManager
{
    [SerializeField] private GameObject[] pieces; //Array de piezas del juego
    [SerializeField] private XRReferenceImageLibrary imageLibrary; //Librería de marcadores AR
    private List<XRReferenceImage> pieceImagesList = new(); //Marcadores asociados a las piezas
    private int index = 0;
    public string[] Names { get; private set; } //Nombres de los jugadores asociados a las piezas
    public event System.Action OnNameChanged;

    private void Awake()
    {
        pieceImagesList = imageLibrary.Where(img => img.name.ToLower().Contains("piece")).ToList(); //Se almacenan los marcadores asociados
        Names = new string[pieces.Length]; //Se inicializa el array de nombres y se le da a cada pieza un nombre génerico por defecto
        for(int i = 0; i < Names.Length; i++) { Names[i] = "Player " + (i + 1); }
    }

    ARTrackedImage trackable;
    public bool ProvideInfo(AGameUnit unit) //Se proporciona la información a las piezas escaneadas
    {
        trackable = unit.GetComponentInParent<ARTrackedImage>();
        index = pieceImagesList.IndexOf(trackable.referenceImage); //Se calcula la pieza a enseñar a partir del índice de su marcador
        if (index >= 0 && index < pieces.Length)
        {
            unit.SetModel(pieces[index]); //Se aplica el modelo
            (unit as Piece).Index = index; //Se le proporciona el índice del marcador, que corresponde al del array Names
            if (GameSettings.Instance.IsOnline) RequestNameServerRpc(index); //Se pide el nombre almacenado en el servidor al ser escaneado por primera vez
            return true;
        }
        return false;
    }

    [ServerRpc (RequireOwnership = false)]
    public void ChangeNameServerRpc(int index, string name) //Hace que el cambio de nombre de una pieza se aplique en todos los clientes
    {
        UpdateNamesClientRpc(index, name);
    }

    [ClientRpc]
    private void UpdateNamesClientRpc(int index, string name) //Actualiza un nombre y obliga a las piezas a actualizar su UI para mostrarlo
    {
        Names[index] = name;
        OnNameChanged?.Invoke();
    }

    [ServerRpc (RequireOwnership = false)]
    public void RequestNameServerRpc(int index) //Simplemente pide el nombre actual de una pieza sin cambiarlo, para que se sincronicen los clientes
    {
        UpdateNamesClientRpc(index, Names[index]);
    }
}
