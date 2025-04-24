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

    public void UpdateBoardInfo()
    {
        foreach (var trackable in trackedImageManager.trackables)
        {
            if (boardImagesList.Contains(trackable.referenceImage))
            {
                index = boardImagesList.IndexOf(trackable.referenceImage);
                if (index >= 0 && index < boardModels.Length)
                {
                    trackable.GetComponent<PlayableUnit>().DisplayUnit();
                    trackable.GetComponentInChildren<Board>(true)?.SetModel(boardModels[index]);
                }
                else if(index >= boardModels.Length)
                {
                    index -= boardModels.Length;
                    if(index < boardSprites.Length)
                    {
                        trackable.GetComponent<PlayableUnit>().DisplayUnit();
                        trackable.GetComponentInChildren<Board>(true)?.SetSprite(boardSprites[index]);
                    }
                }
            }
        }
    }
}
