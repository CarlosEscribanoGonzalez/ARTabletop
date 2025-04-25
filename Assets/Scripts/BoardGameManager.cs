using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Linq;
using System.Collections.Generic;

public class BoardGameManager : MonoBehaviour
{
    [SerializeField] private GameObject[] boardModels;
    [SerializeField] private Sprite[] boardSprites;
    [SerializeField] private XRReferenceImageLibrary imageLibrary;
    private ARTrackedImageManager trackedImageManager;
    private List<XRReferenceImage> boardImagesList = new();
    int index = 0;

    private void Awake()
    {
        trackedImageManager = FindFirstObjectByType<ARTrackedImageManager>();
        boardImagesList = imageLibrary.Where(img => img.name.ToLower().Contains("board")).ToList();
    }

    ARTrackedImage trackable;
    public bool ProvideInfo(Board board)
    {
        trackable = board.GetComponentInParent<ARTrackedImage>();
        index = boardImagesList.IndexOf(trackable.referenceImage);
        if (index >= 0 && index < boardModels.Length)
        {
            board.SetModel(boardModels[index]);
            return true;
        }
        else
        {
            index -= boardModels.Length;
            if (index >= 0 && index < boardSprites.Length)
            {
                board.SetSprite(boardSprites[index]);
                return true;
            }
        }
        return false;
    }
}
