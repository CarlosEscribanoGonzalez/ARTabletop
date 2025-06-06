using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class DecimalInputField : MonoBehaviour
{
    private TMP_InputField inputField;
    private const string validChars = "0123456789.";
    private bool separatorUsed;
    private string filteredText;
    public UnityEvent OnValueFiltered;

    private void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.onValueChanged.AddListener(ValidateInput);
    }

    private void ValidateInput(string input)
    {
        filteredText = "";
        separatorUsed = false;
        foreach (char c in input)
        {
            if (validChars.Contains(c))
            {
                if(c == validChars[validChars.Length - 1])
                {
                    if (!separatorUsed)
                    {
                        separatorUsed = true;
                        if (filteredText.Length == 0) filteredText += "0";
                    }
                    else continue;
                }
                filteredText += c;
            }
        }
        inputField.SetTextWithoutNotify(filteredText);
        OnValueFiltered?.Invoke();
    }
}
