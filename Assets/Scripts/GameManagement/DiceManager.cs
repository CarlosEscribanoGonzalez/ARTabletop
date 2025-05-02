using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class DiceManager : MonoBehaviour
{
    [SerializeField] private bool gameHasDice = true; 
    [SerializeField] private GameObject diceCamera; //Cámara en overlay que apunta al dado desde arriba
    [SerializeField] private TMP_Dropdown typeDropdown; //Dropdown con el tipo de dados
    [SerializeField] private TMP_Dropdown numDropdown; //Dropdown con el número de dados a lanzar
    [SerializeField] private GameObject skipButton; //Botón de saltar animación
    private int numFaces; //Número de caras del dado lanzado
    private int numThrows; //Número de dados lanzados simultáneamente
    private List<Dice> dices = new();
    public List<int> Results { get; private set; } = new(); //Lista de resultados para todas las tiradas

    private void Start()
    {
        if (!gameHasDice) Destroy(this.gameObject); //Si el juego en cuestión no tiene dado este objeto es eliminado
        GameSettings.Instance.OnSeedSet += () => GetComponentInChildren<Canvas>(true).gameObject.SetActive(true); //Cuando se entra en partida se activa la UI
        dices = GetComponentsInChildren<Dice>(true).ToList();
    }

    public void ToggleDiceThrow(bool enable) //Activa y desactiva la cámara que apunta al dado y se encarga de su lanzamiento
    {
        diceCamera.SetActive(enable);
        if (enable)
        {
            //La info sobre el número de lanzamientos y tipo se encuentra en los nombres de los posibles valores de los dropdown
            numFaces = int.Parse(typeDropdown.options[typeDropdown.value].text.Substring(1));
            numThrows = int.Parse(numDropdown.options[numDropdown.value].text);
            GenerateResults();
            for (int i = 0; i < dices.Count; i++)
            {
                dices[i].transform.parent.gameObject.SetActive(i < numThrows);
            }
        }
        else foreach (Dice d in dices) d.transform.parent.gameObject.SetActive(false);
    }

    public void NotifyDiceStopped()
    {
        if (--numThrows == 0) SkipAnimation();
    }

    public void SkipAnimation()
    {
        FindFirstObjectByType<DiceResults>(FindObjectsInactive.Include).gameObject.SetActive(true);
        skipButton.SetActive(false);
    }

    public int GetDiceResult(Dice dice)
    {
        return Results[dices.IndexOf(dice)];
    }

    private void GenerateResults() //Se genera un resultado por lanzamiento
    {
        Results.Clear();
        for (int i = 0; i < numThrows; i++)
        {
            Results.Add(Random.Range(0, numFaces) + 1);
        }
    }
}
