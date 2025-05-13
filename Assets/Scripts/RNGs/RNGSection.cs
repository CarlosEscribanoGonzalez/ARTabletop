using UnityEngine;
using UnityEngine.UI;

public class RNGSection : MonoBehaviour
{
    [SerializeField] private bool gameHasDice = true; //Indica si el dado está configurado para el juego
    [SerializeField] private bool gameHasWheel = true; //Indica si la rueda está configurada para el juego
    [SerializeField] private Button diceButton; //Botón que activa el menú de dados
    [SerializeField] private Button wheelButton; //Botón que activa el menú de la ruleta

    private void Start()
    {
        if (!gameHasDice) Destroy(diceButton.gameObject); //Si no hay dados el botón se destruye
        if (!gameHasWheel) Destroy(wheelButton.gameObject); //Si no hay ruleta el botón se destruye
        GameSettings.Instance.OnSeedSet += () => GetComponentInChildren<Canvas>(true).enabled = true; //Cuando se entra en partida se activa la UI
    }
}
