using UnityEngine;
using UnityEngine.UI;

public class RNGSection : MonoBehaviour
{
    [SerializeField] private Button diceButton; //Botón que activa el menú de dados
    [SerializeField] private Button wheelButton; //Botón que activa el menú de la ruleta
    [SerializeField] private Button coinButton; //Botón que activa el menú de las monedas
    public bool GameHasDice { get; set; } = true; //Indica si el dado está configurado para el juego
    public bool GameHasWheel { get; set; } = true; //Indica si la rueda está configurada para el juego
    public bool GameHasCoins { get; set; } = true; //Indica si las monedas están configuradas para el juego

    private void Start()
    {
        GameSettings.Instance.OnSeedSet += InitializeRNGSection;
    }

    private void InitializeRNGSection()
    {
        if (!GameHasDice) Destroy(diceButton.gameObject); //Si no hay dados el botón se destruye
        if (!GameHasWheel) Destroy(wheelButton.gameObject); //Si no hay ruleta el botón se destruye
        if (!GameHasCoins) Destroy(coinButton.gameObject); //Si no hay ruleta el botón se destruye
        GetComponentInChildren<Canvas>(true).enabled = true; //Cuando se entra en partida se activa la UI
    }
}
