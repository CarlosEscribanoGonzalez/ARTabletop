using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Linq;
using System.Collections.Generic;

public class CardGameManager : MonoBehaviour, IGameManager
{
    [SerializeField] private CardInfo[] cardsInfo; //Información de las cartas a mostrar
    [SerializeField] private XRReferenceImageLibrary imageLibrary; //Librería de los markers AR
    private List<XRReferenceImage> cardImagesList = new(); //Marcadores asociados a las cartas normales
    private CardInfo[] randomizedInfo; //Array de información de las cartas. Es cardsInfo pero aleatorio
    private int index = 0;

    private void Start()
    {
        //Se aleatoriza la información una vez la semilla ha sido establecida
        GameSettings.Instance.OnSeedSet += () => randomizedInfo = cardsInfo.OrderBy(x => Random.Range(0f, 1f)).ToArray();
        cardImagesList = imageLibrary.Where(img => img.name.ToLower().Contains("card")
                                                    && !img.name.ToLower().Contains("special")).ToList(); //Se almacenan los marcadores de cartas normales
    }

    ARTrackedImage trackable;
    public bool ProvideInfo(AGameUnit unit) //Se proporciona información a las cartas escaneadas
    {
        trackable = unit.GetComponentInParent<ARTrackedImage>();
        index = cardImagesList.IndexOf(trackable.referenceImage); //Se calcula la información a mostrar a partir del índice del marcador
        if (index >= 0 && index < randomizedInfo.Length)
        {
            (unit as Card).SetInfo(randomizedInfo[index]);
            return true;
        }
        return false;
    }
}
