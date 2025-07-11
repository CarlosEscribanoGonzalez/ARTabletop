using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public abstract class ABuilder<T> : MonoBehaviour where T : new()
{
    [SerializeField] protected APreview<T> preview;
    [SerializeField] protected TMP_Dropdown contentDropdown;
    [SerializeField] protected TextMeshProUGUI indexText;
    [SerializeField] protected int initLength = 4;
    [SerializeField] protected int maxLength = 16;
    [SerializeField] protected bool addZero = false;
    [SerializeField] protected Canvas confirmationPanel;
    [SerializeField] private GameObject arrows;
    [SerializeField] private bool newContentIsNull = false;
    [SerializeField] private Button addContentButton;
    [SerializeField] private Button removeContentButton;
    protected int index = 0;
    private int newLength; //Length a la que se quiere poner el contenido desde el dropdown
    public List<T> Content { get; set; } = new();
    public int Index { get { return index; } set { index = value; } }

    protected virtual void Awake()
    {
        if (contentDropdown == null) return;
        int initialValue = contentDropdown.value; //SetInitInfo se ejecuta antes que el Awake, as� que puede haber alterado el value antes de estar el dropdown configurado
        contentDropdown.ClearOptions();
        List<string> options = new();
        if (addZero) options.Add("0");
        for (int i = 0; i < maxLength; i++) options.Add((i + 1).ToString());
        contentDropdown.AddOptions(options);
        contentDropdown.SetValueWithoutNotify(initialValue);
    }

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
            contentDropdown.SetValueWithoutNotify(Content.Count - 1); //Se "cancela" el cambio a la espera de la confirmaci�n
            confirmationPanel.enabled = true;
        }
        else if (Content.Count < newLength)
            while (Content.Count < newLength) Content.Add(newContentIsNull ? default : new T());
        CheckContentButtons();
    }

    public virtual void ConfirmChange()
    {
        Content.RemoveRange(newLength, Content.Count - newLength);
        contentDropdown.SetValueWithoutNotify(newLength - 1);
        if (index >= Content.Count)
        {
            index = Content.Count - 1;
            UpdateIndex(0);
        }
        CheckContentButtons();
    }

    public virtual void AddContent()
    {
        Content.Add(newContentIsNull ? default : new T());
        contentDropdown.SetValueWithoutNotify(Content.Count - 1);
        Index = Content.Count - 1;
        UpdateIndex(0);
        CheckContentButtons();
    }

    public virtual void RemoveContent()
    {
        Content.RemoveAt(Index);
        contentDropdown.SetValueWithoutNotify(Content.Count - 1);
        if (Index == Content.Count) Index--;
        UpdateIndex(0);
        CheckContentButtons();
    }

    protected void CheckArrowsVisibility()
    {
        arrows.SetActive(Content.Count > 1);
    }

    protected void CheckContentButtons()
    {
        if (removeContentButton == null || addContentButton == null) return;
        removeContentButton.interactable = Content.Count > 1;
        addContentButton.interactable = Content.Count < maxLength;
        CheckArrowsVisibility();
    }

    public abstract void SetInitInfo(GameInfo gameInfo);

    public abstract T GetDefaultContent();

    public abstract void DownloadInfo();
}
