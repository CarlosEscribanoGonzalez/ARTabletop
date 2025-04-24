using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Linq;
using System.Collections.Generic;

public class PieceGameManager : MonoBehaviour
{
    [SerializeField] private GameObject[] pieces;
    [SerializeField] private XRReferenceImageLibrary imageLibrary;
    private ARTrackedImageManager trackedImageManager;
    private List<XRReferenceImage> pieceImagesList = new();
    int index = 0;

    private void Awake()
    {
        trackedImageManager = FindFirstObjectByType<ARTrackedImageManager>();
        pieceImagesList = imageLibrary.Where(img => img.name.ToLower().Contains("piece")).ToList();
    }

    public void UpdatePieceInfo()
    {
        foreach (var trackable in trackedImageManager.trackables)
        {
            if (pieceImagesList.Contains(trackable.referenceImage))
            {
                index = pieceImagesList.IndexOf(trackable.referenceImage);
                if (index >= 0 && index < pieces.Length)
                {
                    trackable.GetComponent<PlayableUnit>().DisplayUnit();
                    trackable.GetComponentInChildren<Piece>(true)?.SetModel(pieces[index]);
                }
            }
        }
    }
}
