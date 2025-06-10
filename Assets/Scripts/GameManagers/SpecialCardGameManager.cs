using UnityEngine;
using Unity.Netcode;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SpecialCardGameManager : NetworkBehaviour, IGameManager
{
    private CardInfo[] randomizedInfo; //Array aleatorio de información; cardsInfo pero aleatorio
    private Card specialCard; //Carta especial asociada a este manager
    private NetworkVariable<int> currentInfoIndex = new(0); //Índice de la información mostrada por la carta actualmente
    private NetworkVariable<int> totalDrags = new(0); //Índice de cartas sacadas sin barajar
    private NetworkVariable<int> randomizerSeed = new(0); //Semilla con la que se han barajado las cartas especiales
    public CardInfo[] CardsInfo { get; set; } = null; //Array de información de las cartas a mostrar
    public string CardTypeName { get; set; } = "SpecialCard"; //Nombre de las cartas mostradas (Suerte, Caja de Comunidad... por ejemplo)
    public Sprite DefaultImage { get; set; } = null; //Imagen por defecto a mostrar por las cartas de este manager

    private void Start()
    {
        //Se aleatoriza la información una vez la semilla ha sido establecida
        GameSettings.Instance.OnSeedSet += () => ApplyShuffle(false);
        currentInfoIndex.OnValueChanged += (int prevIndex, int currentIndex) => ApplyInfo(true); //La info se actualiza cuando el índice cambia
        totalDrags.OnValueChanged += (int prevIndex, int currentIndex) =>
        {
            if (specialCard != null) specialCard.PrevButton.GetComponent<Button>().interactable = currentIndex != 0; //El botón se actualiza cuando se cambia el contenido
        };
        randomizerSeed.OnValueChanged += (int prevSeed, int newSeed) => ApplyShuffle(); //Cuando se barajan las cartas se aplican los cambios en todos los clientes
        GetComponentInChildren<TextMeshProUGUI>().text = $"{CardTypeName} cards have been shuffled.";
        foreach (CardInfo card in CardsInfo) if (card.sprite == null) card.sprite = DefaultImage;
    }

    public bool ProvideInfo(AGameUnit unit) //Se proporciona información sobre la carta a las cartas escaneadas
    {
        specialCard = unit as Card;
        specialCard.PrevButton.GetComponent<Button>().interactable = totalDrags.Value != 0; //Se inicializa el estado de PrevButton (totalDrags.OnValueChanged no afecta a la carta si no se ha llamado antes a ProvideInfo)
        if (currentInfoIndex.Value >= 0 && currentInfoIndex.Value < randomizedInfo.Length)
        {
            ApplyInfo(false); //Se aplica la información a la carta
            return true;
        }
        return false;
    }

    public void UpdateCard(int dir) //Se actualiza el contenido de la carta al ser pulsado el botón de cambio de contenido
    {
        if (GameSettings.Instance.IsOnline) UpdateIndexServerRpc(dir);
        else UpdateIndex(dir);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestShuffleServerRpc()
    {
        Shuffle();
    }

    public void Shuffle() //Las cartas se barajan a partir de una nueva semilla que se comparte entre todos los clientes
    {
        CardInfo prevCardInfo = randomizedInfo[currentInfoIndex.Value];
        int randSeed;
        System.Random rand;
        do
        {
            randSeed = Random.Range(0, 100000);
            rand = new System.Random(randSeed);
            randomizedInfo = CardsInfo.OrderBy(x => rand.Next()).ToArray();
        } while (prevCardInfo == randomizedInfo[0]); //Se garantiza que la nueva carta no sea la misma que la última mostrada
        randomizerSeed.Value = randSeed;
        currentInfoIndex.Value = 0;
        totalDrags.Value = 0;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateIndexServerRpc(int dir)
    {
        UpdateIndex(dir);
    }

    private void UpdateIndex(int dir)
    {
        if (totalDrags.Value + dir < 0) return;
        totalDrags.Value += dir; //totalDrags se actualiza independientemente de lo que pase con currentInfoIndex
        //Si ya se han mostrado todas las cartas posibles estas son barajadas y se reinicia el índice
        if (currentInfoIndex.Value + dir >= CardsInfo.Length)
        {
            if (GameSettings.Instance.AutoShuffle)
            {
                if(GameSettings.Instance.IsOnline) RequestShuffleServerRpc();
                else Shuffle();
            }
            else currentInfoIndex.Value = 0;
        }
        else if (currentInfoIndex.Value + dir < 0)
        {
            if (!GameSettings.Instance.AutoShuffle) currentInfoIndex.Value = CardsInfo.Length - 1;
        }
        else currentInfoIndex.Value += dir;
    }

    private void ApplyInfo(bool resetScale)
    {
        specialCard ??= GameSettings.Instance.SpecialCardsDictionary[this]; //Tras llamadas RPC special card se puede desasignar al no ser una variable en red
        if (specialCard == null) return;
        specialCard.SetInfo(randomizedInfo[currentInfoIndex.Value], resetScale);
    }

    private void ApplyShuffle(bool displayFeedback = true) //Se baraja igual en todos los clientes al compartir semilla
    {
        int randomSeed = randomizerSeed.Value != default ? randomizerSeed.Value : Random.Range(0, 100000);
        System.Random rand = new System.Random(randomSeed);
        randomizedInfo = CardsInfo.OrderBy(x => rand.Next()).ToArray();
        ApplyInfo(true);
        if(displayFeedback) StartCoroutine(DisplayShuffleFeedback());
    }

    private static SpecialCardGameManager currentManagerDisplaying; //Sólo un SpecialCardManager puede hacer display del feedback a la vez
    IEnumerator DisplayShuffleFeedback() //Corrutina que enseña en la pantalla de todos los clientes cuando una carta especial ha sido barajada
    {
        if (currentManagerDisplaying != this) //Si ya se está desplegando feedback sobre este manager la llamada se ignora
        {
            while(currentManagerDisplaying != null) yield return new WaitForSeconds(0.3f); //Se espera a que termine el feedback anterior
            currentManagerDisplaying = this;
            GetComponent<Animator>().SetTrigger("Shuffled"); //Activa la animación
            yield return new WaitForSeconds(2.1f); //Espera un cooldown para que termine la animación antes de liberar currentManagerDisplaying
            currentManagerDisplaying = null;
        }  
    }
}
