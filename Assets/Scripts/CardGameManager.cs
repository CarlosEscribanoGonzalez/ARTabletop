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

    public void UpdateCardsInfo()
    {
        foreach (var trackable in trackedImageManager.trackables)
        {
            if (cardImagesList.Contains(trackable.referenceImage))
            {
                index = cardImagesList.IndexOf(trackable.referenceImage);
                if (index >= 0 && index < cardsInfo.Length)
                {
                    if (cardsInfo[index].sprite != null) trackable.GetComponentInChildren<Card>(true)?.SetSprite(randomizedInfo[index].sprite);
                    trackable.GetComponentInChildren<Card>(true)?.SetText(randomizedInfo[index].text);
                }
            }
        }
    }
}
