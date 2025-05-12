using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Linq;
using System.Collections.Generic;
using Unity.Netcode;

public class PieceGameManager : NetworkBehaviour, IGameManager
{
    [SerializeField] private GameObject[] pieces; //Array de piezas del juego
    [SerializeField] private XRReferenceImageLibrary imageLibrary;
    private List<XRReferenceImage> pieceImagesList = new(); //Marcadores asociados a las piezas
    private int index = 0;
    public string[] Names { get; private set; }
    public event System.Action OnNameChanged;

    private void Awake()
    {
        pieceImagesList = imageLibrary.Where(img => img.name.ToLower().Contains("piece")).ToList(); //Se almacenan los marcadores asociados
        Names = new string[pieces.Length];
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
            (unit as Piece).Index = index;
            RequestNameServerRpc(index);
            return true;
        }
        return false;
    }

    [ServerRpc (RequireOwnership = false)]
    public void ChangeNameServerRpc(int index, string name)
    {
        UpdateNamesClientRpc(index, name);
    }

    [ClientRpc]
    private void UpdateNamesClientRpc(int index, string name)
    {
        Names[index] = name;
        OnNameChanged?.Invoke();
    }

    [ServerRpc (RequireOwnership = false)]
    public void RequestNameServerRpc(int index)
    {
        UpdateNamesClientRpc(index, Names[index]);
    }
}
