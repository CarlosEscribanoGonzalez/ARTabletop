using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Linq;

public class CardGameManager : MonoBehaviour
{
    [SerializeField] private CardInfo[] cardsInfo;
    [SerializeField] private XRReferenceImageLibrary imagesLibrary;
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    private CardInfo[] randomizedInfo;
    int index;

    private void Awake()
    {
        randomizedInfo = cardsInfo.OrderBy(x => UnityEngine.Random.value).ToArray();
    }

    public void UpdateCardsInfo()
    {
        foreach(var card in trackedImageManager.trackables)
        {
            index = imagesLibrary.indexOf(card.referenceImage);
            if(cardsInfo[index].sprite != null) card.GetComponent<Card>().SetSprite(randomizedInfo[index].sprite);
            card.GetComponent<Card>().SetText(randomizedInfo[index].text);
        }
    }
}
