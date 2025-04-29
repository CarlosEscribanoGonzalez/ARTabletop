using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using System.Linq;

public class SpecialCardGameManager : MonoBehaviour, IGameManager
{
    [SerializeField] private CardInfo[] cardsInfo; //Array de informaci�n de las cartas a mostrar
    private CardInfo[] randomizedInfo; //Array aleatorio de informaci�n; cardsInfo pero aleatorio
    private int currentInfoIndex = 0; //�ndice de la informaci�n mostrada por la carta actualmente
    private Card specialCard; //Carta especial asociada a este manager

    private void Awake()
    {
        randomizedInfo = cardsInfo.OrderBy(x => UnityEngine.Random.value).ToArray(); //Se aleatoriza la informaci�n
    }

    public bool ProvideInfo(AGameUnit unit) //Se proporciona informaci�n sobre la carta a las cartas escaneadas
    {
        specialCard = unit as Card;
        if (currentInfoIndex >= 0 && currentInfoIndex < randomizedInfo.Length)
        {
            ApplyInfo(false); //Se aplica la informaci�n a la carta
            return true;
        }
        return false;
    }

    public void UpdateCard() //Se actualiza el contenido de la carta al ser pulsado el bot�n de cambio de contenido
    {
        //Si ya se han mostrado todas las cartas posibles estas son barajadas y se reinicia el �ndice
        if (++currentInfoIndex >= cardsInfo.Length) 
        {
            CardInfo prevCardInfo = randomizedInfo[cardsInfo.Length - 1];
            do
                randomizedInfo = cardsInfo.OrderBy(x => UnityEngine.Random.value).ToArray();
            while (prevCardInfo == randomizedInfo[0]); //Se garantiza que la nueva carta no sea la misma que la �ltima mostrada
            currentInfoIndex = 0;
        }
        ApplyInfo(true); //Se aplica la informaci�n a la carta
    }

    private void ApplyInfo(bool recalculateScale)
    {
        specialCard.SetSprite(randomizedInfo[currentInfoIndex].sprite); //Se aplica el sprite
        specialCard.SetText(randomizedInfo[currentInfoIndex].text); //Se aplica el texto
        specialCard.SetSize(randomizedInfo[currentInfoIndex].sizeMult, recalculateScale); //Se ajusta el tama�o
    }
}
