using UnityEngine;
using TMPro;

public class DiceManager : MonoBehaviour
{
    [SerializeField] private bool gameHasDice = true; 
    [SerializeField] private Dice dice;
    [SerializeField] private GameObject diceCamera; //Cámara en overlay que apunta al dado desde arriba
    [SerializeField] private TMP_Dropdown typeDropdown; //Dropdown con el tipo de dados
    [SerializeField] private TMP_Dropdown numDropdown; //Dropdown con el número de dados a lanzar
    private int numFaces; //Número de caras del dado lanzado
    private int numThrows; //Número de dados lanzados simultáneamente

    private void Awake()
    {
        if (!gameHasDice) Destroy(this.gameObject); //Si el juego en cuestión no tiene dado este objeto es eliminado
    }

    public void ToggleDiceThrow(bool enable) //Activa y desactiva la cámara que apunta al dado y se encarga de su lanzamiento
    {
        diceCamera.SetActive(enable);
        if (enable)
        {
            //La info sobre el número de lanzamientos y tipo se encuentra en los nombres de los posibles valores de los dropdown
            numFaces = int.Parse(typeDropdown.options[typeDropdown.value].text.Substring(1));
            numThrows = int.Parse(numDropdown.options[numDropdown.value].text);
            dice.ThrowDice(numFaces, numThrows);
        }
    }
}
