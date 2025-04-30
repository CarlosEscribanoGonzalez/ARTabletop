using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameSettings : MonoBehaviour
{
    [SerializeField] private bool extendedTracking = false; //Indica si el tracking extendido está activo para el juego o no
    private List<SpecialCardGameManager> specialCardManagers; //Lista con los managers de las cartas especiales
    public static GameSettings Instance { get; private set; }
    public bool ExtendedTracking { get { return extendedTracking; } }

    private void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep; //La pantalla se configura para no apagarse sola
        specialCardManagers = FindObjectsByType<SpecialCardGameManager>(FindObjectsSortMode.None).ToList();
        Instance = this;
    }

    public SpecialCardGameManager GetSpecialCardManager(string markerName) //Asigna el manager a las cartas especiales que lo pidan, basándose en el nombre de su marcador
    {
        int index = int.Parse(markerName.Substring(markerName.Length - 1)) - 1; //El índice del marcador es el último char de su nombre
        if (index >= 0 && index < specialCardManagers.Count) return specialCardManagers[index];
        else return null;
    }
}
