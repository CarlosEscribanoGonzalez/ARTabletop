using UnityEngine;
using TMPro;

public class ConfirmationPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI disclaimerText;
    public GameOption CurrentOption { get; set; } = null;

    private void OnEnable()
    {
        disclaimerText.text = $"Are you sure you want to delete {CurrentOption.Info.gameName}?";
    }

    public void OnDeleteConfirmed()
    {
        CurrentOption.RemoveGame();
    }
}
