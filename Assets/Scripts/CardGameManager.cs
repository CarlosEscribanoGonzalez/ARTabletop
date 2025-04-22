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
        cardImagesList = imageLibrary.Where(img => img.name.ToLower().Contains("card")).ToList();
    }

    public void UpdateCardsInfo()
    {
        foreach (var card in trackedImageManager.trackables)
        {
            if (cardImagesList.Contains(card.referenceImage))
            {
                index = cardImagesList.IndexOf(card.referenceImage);
                if (cardsInfo[index].sprite != null) card.GetComponent<Card>().SetSprite(randomizedInfo[index].sprite);
                card.GetComponent<Card>().SetText(randomizedInfo[index].text);
            }
        }
    }
}
