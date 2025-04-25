using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using System.Linq;

public class SpecialCardGameManager : MonoBehaviour
{
    [SerializeField] private CardInfo[] cardsInfo;
    [SerializeField] private XRReferenceImageLibrary imageLibrary;
    private CardInfo[] randomizedInfo;
    private int currentInfoIndex = 0;
    private Card specialCard;

    private void Awake()
    {
        randomizedInfo = cardsInfo.OrderBy(x => UnityEngine.Random.value).ToArray();
    }

    public bool ProvideInfo(Card card)
    {
        specialCard = card;
        if (currentInfoIndex >= 0 && currentInfoIndex < randomizedInfo.Length)
        {
            specialCard.SetSprite(randomizedInfo[currentInfoIndex].sprite);
            specialCard.SetText(randomizedInfo[currentInfoIndex].text);
            specialCard.SetSize(randomizedInfo[currentInfoIndex].sizeMult);
            return true;
        }
        return false;
    }

    public void UpdateCard()
    {
        if (++currentInfoIndex >= cardsInfo.Length)
        {
            CardInfo prevCardInfo = randomizedInfo[cardsInfo.Length - 1];
            do
                randomizedInfo = cardsInfo.OrderBy(x => UnityEngine.Random.value).ToArray();
            while (prevCardInfo == randomizedInfo[0]);
            currentInfoIndex = 0;
        }
        specialCard.SetSprite(randomizedInfo[currentInfoIndex].sprite);
        specialCard.SetText(randomizedInfo[currentInfoIndex].text);
        specialCard.SetSize(randomizedInfo[currentInfoIndex].sizeMult, true);
    }
}
