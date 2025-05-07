using UnityEngine;
using UnityEngine.UI;

public class RNGSection : MonoBehaviour
{
    [SerializeField] private bool gameHasDice = true;
    [SerializeField] private bool gameHasWheel = true;
    [SerializeField] private Button diceButton;
    [SerializeField] private Button wheelButton;

    private void Start()
    {
        if (!gameHasDice) Destroy(diceButton.gameObject);
        if (!gameHasWheel) Destroy(wheelButton.gameObject);
        GameSettings.Instance.OnSeedSet += () => GetComponentInChildren<Canvas>(true).enabled = true; //Cuando se entra en partida se activa la UI
    }
}
