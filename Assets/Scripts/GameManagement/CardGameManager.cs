using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Linq;
using System.Collections.Generic;

public class CardGameManager : MonoBehaviour
{
    [SerializeField] private CardInfo[] cardsInfo; //Informaci�n de las cartas a mostrar
    [SerializeField] private XRReferenceImageLibrary imageLibrary;
    private List<XRReferenceImage> cardImagesList = new(); //Marcadores asociados a las cartas normales
    private CardInfo[] randomizedInfo; //Array de informaci�n de las cartas. Es cardsInfo pero aleatorio
    private int index = 0;

    private void Awake()
    {
        randomizedInfo = cardsInfo.OrderBy(x => UnityEngine.Random.value).ToArray(); //Se aleatoriza la info
        cardImagesList = imageLibrary.Where(img => img.name.ToLower().Contains("card") 
                                                    && !img.name.ToLower().Contains("special")).ToList(); //Se almacenan los marcadores de cartas normales
    }

    ARTrackedImage trackable;
    public bool ProvideInfo(Card card) //Se proporciona informaci�n a las cartas escaneadas
    {
        trackable = card.GetComponentInParent<ARTrackedImage>();
        index = cardImagesList.IndexOf(trackable.referenceImage); //Se calcula la informaci�n a mostrar a partir del �ndice del marcador
        if (index >= 0 && index < randomizedInfo.Length)
        {
            card.SetSprite(randomizedInfo[index].sprite); //Se aplica el sprite
            card.SetText(randomizedInfo[index].text); //Se aplica el texto
            card.SetSize(randomizedInfo[index].sizeMult); //Se ajusta el tama�o
            return true;
        }
        return false;
    }
}
