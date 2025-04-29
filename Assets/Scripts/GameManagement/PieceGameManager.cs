using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Linq;
using System.Collections.Generic;

public class PieceGameManager : MonoBehaviour, IGameManager
{
    [SerializeField] private GameObject[] pieces; //Array de piezas del juego
    [SerializeField] private XRReferenceImageLibrary imageLibrary;
    private List<XRReferenceImage> pieceImagesList = new(); //Marcadores asociados a las piezas
    private int index = 0;

    private void Awake()
    {
        pieceImagesList = imageLibrary.Where(img => img.name.ToLower().Contains("piece")).ToList(); //Se almacenan los marcadores asociados
    }

    ARTrackedImage trackable;
    public bool ProvideInfo(AGameUnit unit) //Se proporciona la información a las piezas escaneadas
    {
        trackable = unit.GetComponentInParent<ARTrackedImage>();
        index = pieceImagesList.IndexOf(trackable.referenceImage); //Se calcula la pieza a enseñar a partir del índice de su marcador
        if (index >= 0 && index < pieces.Length)
        {
            unit.SetModel(pieces[index]); //Se aplica el modelo
            return true;
        }
        return false;
    }
}
