using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine.XR.ARFoundation;

public class GameSettings : NetworkBehaviour
{
    [SerializeField] private bool extendedTracking = false; //Indica si el tracking extendido está activo para el juego o no
    [SerializeField] private bool autoShuffle = true; //Indica si las cartas especiales se barajan solas cuando se llega al final
    public Dictionary<SpecialCardGameManager, Card> SpecialCardsDictionary { get; private set; } = new(); //Diccionario con los managers de las cartas especiales y sus cartas asociadas
    public static GameSettings Instance { get; private set; }
    public bool ExtendedTracking { get { return extendedTracking; } } 
    public bool AutoShuffle { get { return autoShuffle; } } 
    public NetworkVariable<int> RandomSeed { get; private set; } = new(); //Semilla para aleatorizar las cartas de todos los jugadores
    public event System.Action OnSeedSet;
    public bool RequiresOnline { get { return SpecialCardsDictionary.Count > 0; } private set { } }

    private void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep; //La pantalla se configura para no apagarse sola
        foreach(SpecialCardGameManager manager in FindObjectsByType<SpecialCardGameManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            SpecialCardsDictionary[manager] = null; //Se crea una entrada para cada uno
        }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer) RandomSeed.Value = Random.Range(0, 100000);
        SetSeed(RandomSeed.Value);
    }

    public void SetSeed(int seed)
    {
        Random.InitState(seed);
        OnSeedSet?.Invoke();
        FindFirstObjectByType<ARTrackedImageManager>().enabled = true;
    }

    public SpecialCardGameManager GetSpecialCardManager(string markerName, Card card) //Asigna el manager a las cartas especiales que lo pidan, basándose en el nombre de su marcador
    {
        int index = int.Parse(markerName.Substring(markerName.Length - 1)) - 1; //El índice del marcador es el último char de su nombre
        if (index >= 0 && index < SpecialCardsDictionary.Count)
        {
            var manager = SpecialCardsDictionary.ElementAt(index).Key;
            SpecialCardsDictionary[manager] = card;
            return manager;
        }
        else return null;
    }
}
