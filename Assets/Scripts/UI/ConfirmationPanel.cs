using UnityEngine;
using TMPro;

public class ConfirmationPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI disclaimerText; //Texto de aviso cuando un juego va a ser eliminado
    public GameOption CurrentOption { get; set; } = null; //Botón del juego que se quiere borrar

    private void OnEnable()
    {
        disclaimerText.text = $"Are you sure you want to delete {CurrentOption.Info.gameName}?";
    }

    public void OnDeleteConfirmed()
    {
        CurrentOption.DeleteGame(); //Se borran el botón y el juego cuando se pulsa el botón de confirmar
    }
}
