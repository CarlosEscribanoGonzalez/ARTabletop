using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class SpecialCardGameManager : NetworkBehaviour, IGameManager
{
    [SerializeField] private CardInfo[] cardsInfo; //Array de información de las cartas a mostrar
    private CardInfo[] randomizedInfo; //Array aleatorio de información; cardsInfo pero aleatorio
    private Card specialCard; //Carta especial asociada a este manager
    private NetworkVariable<int> currentInfoIndex = new(0); //Índice de la información mostrada por la carta actualmente

    private void Start()
    {
        //Se aleatoriza la información una vez la semilla ha sido establecida
        GameSettings.Instance.OnSeedSet += () => randomizedInfo = cardsInfo.OrderBy(x => Random.Range(0f, 1f)).ToArray();
        currentInfoIndex.OnValueChanged += (int prevIndex, int currentIndex) => ApplyInfo(true); //La info se actualiza cuando el índice cambia
    }

    public bool ProvideInfo(AGameUnit unit) //Se proporciona información sobre la carta a las cartas escaneadas
    {
        specialCard = unit as Card;
        if (currentInfoIndex.Value >= 0 && currentInfoIndex.Value < randomizedInfo.Length)
        {
            ApplyInfo(false); //Se aplica la información a la carta
            return true;
        }
        return false;
    }

    public void UpdateCard(int dir) //Se actualiza el contenido de la carta al ser pulsado el botón de cambio de contenido
    {
        UpdateIndexServerRpc(dir);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestShuffleServerRpc()
    {
        CardInfo prevCardInfo = randomizedInfo[currentInfoIndex.Value];
        int randSeed;
        System.Random rand;
        do
        {
            randSeed = Random.Range(0, 100000);
            rand = new System.Random(randSeed);
            randomizedInfo = cardsInfo.OrderBy(x => rand.Next()).ToArray();
        } while (prevCardInfo == randomizedInfo[0]); //Se garantiza que la nueva carta no sea la misma que la última mostrada
        ShuffleClientRpc(randSeed); 
        currentInfoIndex.Value = 0;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateIndexServerRpc(int dir)
    {
        //Si ya se han mostrado todas las cartas posibles estas son barajadas y se reinicia el índice
        if (currentInfoIndex.Value + dir >= cardsInfo.Length)
        {
            if (GameSettings.Instance.AutoShuffle) RequestShuffleServerRpc();
            else currentInfoIndex.Value = 0;
        }
        else if (currentInfoIndex.Value + dir < 0)
        {
            if (!GameSettings.Instance.AutoShuffle) currentInfoIndex.Value = cardsInfo.Length - 1;
            return;
        }
        else currentInfoIndex.Value += dir;
    }

    private void ApplyInfo(bool recalculateScale)
    {
        if (specialCard == null) return;
        specialCard.SetSprite(randomizedInfo[currentInfoIndex.Value].sprite); //Se aplica el sprite
        specialCard.SetText(randomizedInfo[currentInfoIndex.Value].text); //Se aplica el texto
        specialCard.SetSize(randomizedInfo[currentInfoIndex.Value].sizeMult, recalculateScale); //Se ajusta el tamaño
    }

    [ClientRpc]
    private void ShuffleClientRpc(int randSeed) //Se baraja igual en todos los clientes al compartir semilla
    {
        System.Random rand = new System.Random(randSeed);
        randomizedInfo = cardsInfo.OrderBy(x => rand.Next()).ToArray();
        ApplyInfo(true);
    }
}
