using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Linq;
using System.Collections.Generic;

public class CardGameManager : MonoBehaviour
{
    [SerializeField] private CardInfo[] cardsInfo;
    [SerializeField] private XRReferenceImageLibrary imageLibrary;
    private ARTrackedImageManager trackedImageManager;
    private List<XRReferenceImage> cardImagesList = new();
    private CardInfo[] randomizedInfo;
    int index = 0;

    private void Awake()
    {
        trackedImageManager = FindFirstObjectByType<ARTrackedImageManager>();
        randomizedInfo = cardsInfo.OrderBy(x => UnityEngine.Random.value).ToArray();
        cardImagesList = imageLibrary.Where(img => img.name.ToLower().Contains("card") 
                                                    && !img.name.ToLower().Contains("special")).ToList();
    }

    Card currentCard;
    public void UpdateCardsInfo()
    {
        foreach (var trackable in trackedImageManager.trackables)
        {
            if (cardImagesList.Contains(trackable.referenceImage))
            {
                index = cardImagesList.IndexOf(trackable.referenceImage);
                if (index >= 0 && index < randomizedInfo.Length)
                {
                    currentCard = trackable.GetComponentInChildren<Card>(true);
                    trackable.GetComponent<PlayableUnit>().DisplayUnit();
                    currentCard?.SetSprite(randomizedInfo[index].sprite);
                    currentCard?.SetText(randomizedInfo[index].text);
                    currentCard?.SetSize(randomizedInfo[index].sizeMult);
                }
            }
        }
    }
}
