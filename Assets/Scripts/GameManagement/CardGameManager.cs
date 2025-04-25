using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Linq;
using System.Collections.Generic;

public class CardGameManager : MonoBehaviour
{
    [SerializeField] private CardInfo[] cardsInfo;
    [SerializeField] private XRReferenceImageLibrary imageLibrary;
    private List<XRReferenceImage> cardImagesList = new();
    private CardInfo[] randomizedInfo;
    int index = 0;

    private void Awake()
    {
        randomizedInfo = cardsInfo.OrderBy(x => UnityEngine.Random.value).ToArray();
        cardImagesList = imageLibrary.Where(img => img.name.ToLower().Contains("card") 
                                                    && !img.name.ToLower().Contains("special")).ToList();
    }

    ARTrackedImage trackable;
    public bool ProvideInfo(Card card)
    {
        trackable = card.GetComponentInParent<ARTrackedImage>();
        index = cardImagesList.IndexOf(trackable.referenceImage);
        if (index >= 0 && index < randomizedInfo.Length)
        {
            card.SetSprite(randomizedInfo[index].sprite);
            card.SetText(randomizedInfo[index].text);
            card.SetSize(randomizedInfo[index].sizeMult);
            return true;
        }
        return false;
    }
}
