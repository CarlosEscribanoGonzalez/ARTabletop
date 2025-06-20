using UnityEngine;
using System.Collections.Generic;
using TMPro;

public abstract class ABuilder<T> : MonoBehaviour where T : new()
{
    [SerializeField] protected APreview<T> preview;
    [SerializeField] protected TMP_Dropdown contentDropdown;
    [SerializeField] protected TextMeshProUGUI indexText;
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private GameObject arrows;
    [SerializeField] private bool newContentIsNull = false;
    protected int index = 0;
    private int newLength;
    public List<T> Content { get; set; } = new();
    public int Index { get { return index; } set { index = value; } }

    public abstract void SetInitInfo(GameInfo gameInfo);

    public virtual void UpdateIndex(int dir)
    {
        if (Content.Count == 0) return;
        index += dir;
        if (index >= Content.Count) index = 0;
        else if (index < 0) index = Content.Count - 1;
        indexText.text = (index + 1).ToString();
        preview?.UpdateValues(Content[index] ?? GetDefaultContent());
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
        CheckArrowsVisibility();
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
        CheckArrowsVisibility();
    }

    protected void CheckArrowsVisibility()
    {
        arrows.SetActive(Content.Count > 1);
    }

    public abstract T GetDefaultContent();

    public abstract void DownloadInfo();
}
