using UnityEngine;
using UnityEngine.UI;

public class RNGSection : MonoBehaviour
{
    [SerializeField] private Button diceButton; //Bot�n que activa el men� de dados
    [SerializeField] private Button wheelButton; //Bot�n que activa el men� de la ruleta
    [SerializeField] private Button coinButton; //Bot�n que activa el men� de las monedas
    public bool GameHasDice { get; set; } = true; //Indica si el dado est� configurado para el juego
    public bool GameHasWheel { get; set; } = true; //Indica si la rueda est� configurada para el juego
    public bool GameHasCoins { get; set; } = true; //Indica si las monedas est�n configuradas para el juego

    private void Start()
    {
        GameSettings.Instance.OnSeedSet += InitializeRNGSection;
    }

    private void InitializeRNGSection()
    {
        if (!GameHasDice) Destroy(diceButton.gameObject); //Si no hay dados el bot�n se destruye
        if (!GameHasWheel) Destroy(wheelButton.gameObject); //Si no hay ruleta el bot�n se destruye
        if (!GameHasCoins) Destroy(coinButton.gameObject); //Si no hay ruleta el bot�n se destruye
        GetComponentInChildren<Canvas>(true).enabled = true; //Cuando se entra en partida se activa la UI
    }
}
