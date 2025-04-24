using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Linq;
using System.Collections.Generic;

public class SpecialCardGameManager : MonoBehaviour
{
    [SerializeField] private CardInfo[] cardsInfo;
    [SerializeField] private XRReferenceImageLibrary imageLibrary;
    private ARTrackedImageManager trackedImageManager;
    private XRReferenceImage image;
    private CardInfo[] randomizedInfo;
    private static Queue<int> indexList = new(new[] { 1, 2, 3, 4, 5 });
    private int managerIndex = 0;
    private int currentCardIndex = 0;
    private Card specialCard;

    private void Awake()
    {
        managerIndex = indexList.Dequeue();
        trackedImageManager = FindFirstObjectByType<ARTrackedImageManager>();
        randomizedInfo = cardsInfo.OrderBy(x => UnityEngine.Random.value).ToArray();
        image = imageLibrary.ToList().Find(img => img.name.Equals("SpecialCard0" + managerIndex));
    }

    public void UpdateCardsInfo()
    {
        foreach (var trackable in trackedImageManager.trackables)
        {
            if (image == trackable.referenceImage)
            {
                if (currentCardIndex >= 0 && currentCardIndex < cardsInfo.Length)
                {
                    trackable.GetComponent<PlayableUnit>().DisplayUnit();
                    specialCard = trackable.GetComponentInChildren<Card>(true);
                    this.transform.parent = specialCard.transform;
                    this.transform.localPosition = Vector3.zero;
                    if (cardsInfo[currentCardIndex].sprite != null)
                        specialCard?.SetSprite(randomizedInfo[currentCardIndex].sprite);
                    specialCard?.SetText(randomizedInfo[currentCardIndex].text);
                    specialCard.SetSpecial(this);
                }
            }
        }
    }

    public void UpdateCard()
    {
        if (++currentCardIndex >= cardsInfo.Length)
        {
            CardInfo prevCardInfo = randomizedInfo[cardsInfo.Length - 1];
            do
                randomizedInfo = cardsInfo.OrderBy(x => UnityEngine.Random.value).ToArray();
            while (prevCardInfo == randomizedInfo[0]);
            currentCardIndex = 0;
        }
        if (cardsInfo[currentCardIndex].sprite != null)
            specialCard?.SetSprite(randomizedInfo[currentCardIndex].sprite);
        specialCard?.SetText(randomizedInfo[currentCardIndex].text);
    }
}
