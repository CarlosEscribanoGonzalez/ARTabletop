using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class GameSettings : NetworkBehaviour
{
    [SerializeField] private GameObject diceUI; //Managers del juego que se activan cuadno se entra en la partida
    [SerializeField] private bool extendedTracking = false; //Indica si el tracking extendido está activo para el juego o no
    [SerializeField] private bool autoShuffle = true; //Indica si las cartas especiales se barajan solas cuando se llega al final
    private List<SpecialCardGameManager> specialCardManagers; //Lista con los managers de las cartas especiales
    public static GameSettings Instance { get; private set; }
    public bool ExtendedTracking { get { return extendedTracking; } } 
    public bool AutoShuffle { get { return autoShuffle; } } 
    public NetworkVariable<int> RandomSeed { get; private set; } = new(); //Semilla para aleatorizar las cartas de todos los jugadores
    public event System.Action OnSeedSet;

    private void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep; //La pantalla se configura para no apagarse sola
        specialCardManagers = FindObjectsByType<SpecialCardGameManager>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer) RandomSeed.Value = Random.Range(0, 100000);
        diceUI.SetActive(true);
        Random.InitState(RandomSeed.Value);
        OnSeedSet?.Invoke();
    }

    public SpecialCardGameManager GetSpecialCardManager(string markerName) //Asigna el manager a las cartas especiales que lo pidan, basándose en el nombre de su marcador
    {
        int index = int.Parse(markerName.Substring(markerName.Length - 1)) - 1; //El índice del marcador es el último char de su nombre
        if (index >= 0 && index < specialCardManagers.Count) return specialCardManagers[index];
        else return null;
    }
}
