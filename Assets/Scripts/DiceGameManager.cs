using UnityEngine;
using TMPro;

public class DiceGameManager : MonoBehaviour
{
    [SerializeField] private Dice dice;
    [SerializeField] private GameObject diceCamera;
    [SerializeField] private TMP_Dropdown typeDropdown;
    [SerializeField] private TMP_Dropdown numDropdown;
    int numFaces;
    int numThrows; 

    public void ToggleDiceThrow(bool enable)
    {
        diceCamera.SetActive(enable);
        if (enable)
        {
            numFaces = int.Parse(typeDropdown.options[typeDropdown.value].text.Substring(1));
            numThrows = int.Parse(numDropdown.options[numDropdown.value].text);
            dice.ThrowDice(numFaces, numThrows);
        }
    }
}
