using UnityEngine;
using UnityEngine.UI;

public class RNGSection : MonoBehaviour
{
    [SerializeField] private bool gameHasDice = true; //Indica si el dado est� configurado para el juego
    [SerializeField] private bool gameHasWheel = true; //Indica si la rueda est� configurada para el juego
    [SerializeField] private Button diceButton; //Bot�n que activa el men� de dados
    [SerializeField] private Button wheelButton; //Bot�n que activa el men� de la ruleta

    private void Start()
    {
        if (!gameHasDice) Destroy(diceButton.gameObject); //Si no hay dados el bot�n se destruye
        if (!gameHasWheel) Destroy(wheelButton.gameObject); //Si no hay ruleta el bot�n se destruye
        GameSettings.Instance.OnSeedSet += () => GetComponentInChildren<Canvas>(true).enabled = true; //Cuando se entra en partida se activa la UI
    }
}
