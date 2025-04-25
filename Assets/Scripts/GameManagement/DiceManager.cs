using UnityEngine;
using TMPro;

public class DiceManager : MonoBehaviour
{
    [SerializeField] private bool gameHasDice = true; 
    [SerializeField] private Dice dice;
    [SerializeField] private GameObject diceCamera; //C�mara en overlay que apunta al dado desde arriba
    [SerializeField] private TMP_Dropdown typeDropdown; //Dropdown con el tipo de dados
    [SerializeField] private TMP_Dropdown numDropdown; //Dropdown con el n�mero de dados a lanzar
    private int numFaces; //N�mero de caras del dado lanzado
    private int numThrows; //N�mero de dados lanzados simult�neamente

    private void Awake()
    {
        if (!gameHasDice) Destroy(this.gameObject); //Si el juego en cuesti�n no tiene dado este objeto es eliminado
    }

    public void ToggleDiceThrow(bool enable) //Activa y desactiva la c�mara que apunta al dado y se encarga de su lanzamiento
    {
        diceCamera.SetActive(enable);
        if (enable)
        {
            //La info sobre el n�mero de lanzamientos y tipo se encuentra en los nombres de los posibles valores de los dropdown
            numFaces = int.Parse(typeDropdown.options[typeDropdown.value].text.Substring(1));
            numThrows = int.Parse(numDropdown.options[numDropdown.value].text);
            dice.ThrowDice(numFaces, numThrows);
        }
    }
}
