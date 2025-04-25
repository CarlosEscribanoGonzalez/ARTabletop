using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Linq;
using System.Collections.Generic;

public class PieceGameManager : MonoBehaviour
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
    public bool ProvideInfo(Piece piece) //Se proporciona la informaci�n a las piezas escaneadas
    {
        trackable = piece.GetComponentInParent<ARTrackedImage>();
        index = pieceImagesList.IndexOf(trackable.referenceImage); //Se calcula la pieza a ense�ar a partir del �ndice de su marcador
        if (index >= 0 && index < pieces.Length)
        {
            piece.SetModel(pieces[index]); //Se aplica el modelo
            return true;
        }
        return false;
    }
}
