using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Linq;
using System.Collections.Generic;

public class PieceGameManager : MonoBehaviour
{
    [SerializeField] private GameObject[] pieces;
    [SerializeField] private XRReferenceImageLibrary imageLibrary;
    private List<XRReferenceImage> pieceImagesList = new();
    int index = 0;

    private void Awake()
    {
        pieceImagesList = imageLibrary.Where(img => img.name.ToLower().Contains("piece")).ToList();
    }

    ARTrackedImage trackable;
    public bool ProvideInfo(Piece piece)
    {
        trackable = piece.GetComponentInParent<ARTrackedImage>();
        index = pieceImagesList.IndexOf(trackable.referenceImage);
        if (index >= 0 && index < pieces.Length)
        {
            piece.SetModel(pieces[index]);
            return true;
        }
        return false;
    }
}
