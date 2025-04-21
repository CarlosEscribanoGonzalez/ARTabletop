using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class GameManager : MonoBehaviour
{
    [SerializeField] private CardInfo[] cardsInfo;
    [SerializeField] private XRReferenceImageLibrary imagesLibrary;
    [SerializeField] private ARTrackedImageManager trackedImageManager;

    int index;
    public void UpdateCardsInfo()
    {
        foreach(var card in trackedImageManager.trackables)
        {
            index = imagesLibrary.indexOf(card.referenceImage);
            card.GetComponent<Card>().SetSprite(cardsInfo[index].sprite);
            card.GetComponent<Card>().SetText(cardsInfo[index].text);
        }
    }
}
