using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeleteConfirmation : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI disclaimerText; //Texto de aviso cuando un juego va a ser eliminado
    private GameInfo info;
    public GameInfo Info { get { return info; } 
        set 
        {
            info = value;
            disclaimerText.text = $"Are you sure you want to delete '{info.gameName}'?";
        } 
    }

    public void OnDeleteConfirmed()
    {
        FindFirstObjectByType<GameDetailedMenu>().DeleteGame(); //Se borran el botón y el juego cuando se pulsa el botón de confirmar
    }
}
