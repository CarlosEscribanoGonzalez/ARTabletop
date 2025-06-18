using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine.XR.ARFoundation;

public class GameSettings : NetworkBehaviour
{
    public static GameSettings Instance { get; private set; }
    public Dictionary<SpecialCardGameManager, Card> SpecialCardsDictionary { get; private set; } = new(); //Diccionario con los managers de las cartas especiales y sus cartas asociadas
    public bool AutoShuffle { get; set; } = true; //Indica si las cartas especiales se barajan solas al agotarse
    public bool IsOnline { get; set; } = false; //Indica si la partida es online u offline
    public NetworkVariable<int> RandomSeed { get; private set; } = new(); //Semilla para aleatorizar las cartas de todos los jugadores
    public event System.Action OnSeedSet; //Evento que se dispara cuando la semilla es recibida

    private void Awake()
    {
        Instance = this;
        SpecialCardGameManager[] scardGameManagers = FindObjectsByType<SpecialCardGameManager>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
        foreach (var scardManager in scardGameManagers) 
        {
            SpecialCardsDictionary.Add(scardManager, null); //Se crea una entrada para cada manager
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer) RandomSeed.Value = Random.Range(0, 100000); //Si es el host se calcula una semilla aleatoria
        SetSeed(RandomSeed.Value); //Se aplica la semilla
    }

    public void SetSeed(int seed)
    {
        Random.InitState(seed); //La semilla configura Random para que en todos los clientes se sincronicen los números aleatorios
        OnSeedSet?.Invoke();
        FindFirstObjectByType<ARSession>().Reset();
        FindFirstObjectByType<ExtendedTrackingManager>().enabled = true;
        FindFirstObjectByType<ARTrackedImageManager>().enabled = true; //En el momento que la semilla está configurada se activa el tracking
    }

    public SpecialCardGameManager GetSpecialCardManager(string markerName, Card card) //Asigna el manager a las cartas especiales que lo pidan, basándose en el nombre de su marcador
    {
        int index = int.Parse(markerName.Substring(markerName.Length - 2)) - 1; //El índice del marcador es el último char de su nombre
        if (index >= 0 && index < SpecialCardsDictionary.Count)
        {
            var manager = SpecialCardsDictionary.ElementAt(index).Key; //Obtiene el manager asociado al índice y lo devuelve tras asociarlo a la carta
            SpecialCardsDictionary[manager] = card;
            return manager;
        }
        else return null;
    }
}
