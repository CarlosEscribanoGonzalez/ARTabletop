using UnityEngine;
using System.Collections.Generic;
using TMPro;

public abstract class ABuilder<T> : MonoBehaviour where T : new()
{
    [SerializeField] protected APreview<T> preview;
    [SerializeField] protected TMP_Dropdown contentDropdown;
    [SerializeField] private TextMeshProUGUI indexText;
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private bool newContentIsNull = false;
    protected int index = 0;
    private int newLength;
    public List<T> Content { get; private set; } = new();

    public virtual void UpdateIndex(int dir)
    {
        index += dir;
        if (index >= Content.Count) index = 0;
        else if (index < 0) index = Content.Count - 1;
        indexText.text = (index + 1).ToString();
        preview.UpdateValues(Content[index] ?? GetDefaultContent());
    }

    public virtual void UpdateLength(int value)
    {
        newLength = value + 1;
        if (Content.Count > newLength)
        {
            contentDropdown.SetValueWithoutNotify(Content.Count - 1); //Se "cancela" el cambio a la espera de la confirmación
            confirmationPanel.SetActive(true);
        }
        else if (Content.Count < newLength)
            while (Content.Count < newLength) Content.Add(newContentIsNull ? default : new T());
    }

    public virtual void ConfirmChange()
    {
        Content.RemoveRange(newLength, Content.Count - newLength);
        contentDropdown.SetValueWithoutNotify(newLength - 1);
        if (index >= Content.Count)
        {
            index = Content.Count - 1;
            preview.UpdateValues(Content[index]);
        }
        indexText.text = (index + 1).ToString();
    }

    public abstract T GetDefaultContent();

    public abstract void DownloadInfo();
}
