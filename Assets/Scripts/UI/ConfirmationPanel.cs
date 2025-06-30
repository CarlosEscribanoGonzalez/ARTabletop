using UnityEngine;
using TMPro;

public class ConfirmationPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI disclaimerText; //Texto de aviso cuando un juego va a ser eliminado
    public GameInfo Info { get; set; } = null; //Bot�n del juego que se quiere borrar

    private void OnEnable()
    {
        disclaimerText.text = $"Are you sure you want to delete {Info.gameName}?";
    }

    public void OnDeleteConfirmed()
    {
        FindFirstObjectByType<GameDetailedInfo>().DeleteGame(); //Se borran el bot�n y el juego cuando se pulsa el bot�n de confirmar
    }
}
