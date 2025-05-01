using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class SpecialCardGameManager : NetworkBehaviour, IGameManager
{
    [SerializeField] private CardInfo[] cardsInfo; //Array de informaci�n de las cartas a mostrar
    private CardInfo[] randomizedInfo; //Array aleatorio de informaci�n; cardsInfo pero aleatorio
    private Card specialCard; //Carta especial asociada a este manager
    private NetworkVariable<int> currentInfoIndex = new(0); //�ndice de la informaci�n mostrada por la carta actualmente

    private void Start()
    {
        //Se aleatoriza la informaci�n una vez la semilla ha sido establecida
        GameSettings.Instance.OnSeedSet += () => randomizedInfo = cardsInfo.OrderBy(x => Random.Range(0f, 1f)).ToArray();
        currentInfoIndex.OnValueChanged += (int prevIndex, int currentIndex) => ApplyInfo(true); //La info se actualiza cuando el �ndice cambia
    }

    public bool ProvideInfo(AGameUnit unit) //Se proporciona informaci�n sobre la carta a las cartas escaneadas
    {
        specialCard = unit as Card;
        if (currentInfoIndex.Value >= 0 && currentInfoIndex.Value < randomizedInfo.Length)
        {
            ApplyInfo(false); //Se aplica la informaci�n a la carta
            return true;
        }
        return false;
    }

    public void UpdateCard() //Se actualiza el contenido de la carta al ser pulsado el bot�n de cambio de contenido
    {
        UpdateIndexServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateIndexServerRpc()
    {
        //Si ya se han mostrado todas las cartas posibles estas son barajadas y se reinicia el �ndice
        if (currentInfoIndex.Value + 1 >= cardsInfo.Length)
        {
            ShuffleClientRpc();
            currentInfoIndex.Value = 0;
        }
        else currentInfoIndex.Value++;
    }

    private void ApplyInfo(bool recalculateScale)
    {
        if (specialCard == null) return;
        specialCard.SetSprite(randomizedInfo[currentInfoIndex.Value].sprite); //Se aplica el sprite
        specialCard.SetText(randomizedInfo[currentInfoIndex.Value].text); //Se aplica el texto
        specialCard.SetSize(randomizedInfo[currentInfoIndex.Value].sizeMult, recalculateScale); //Se ajusta el tama�o
    }

    [ClientRpc]
    private void ShuffleClientRpc() //Se baraja igual en todos los clientes al compartir semilla
    {
        CardInfo prevCardInfo = randomizedInfo[cardsInfo.Length - 1];
        do
            randomizedInfo = cardsInfo.OrderBy(x => Random.Range(0f, 1f)).ToArray();
        while (prevCardInfo == randomizedInfo[0]); //Se garantiza que la nueva carta no sea la misma que la �ltima mostrada
    }
}
