using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class DiceManager : MonoBehaviour
{
    [SerializeField] private GameObject diceCamera; //Cámara en overlay que apunta al dado desde arriba
    [SerializeField] private TMP_Dropdown typeDropdown; //Dropdown con el tipo de dados
    [SerializeField] private TMP_Dropdown numDropdown; //Dropdown con el número de dados a lanzar
    [SerializeField] private GameObject skipButton; //Botón de saltar animación
    private int numFaces; //Número de caras del dado lanzado
    private int numThrows; //Número de dados lanzados simultáneamente
    private List<Dice> dices = new(); //Lista de los 15 dados lanzables
    public List<int> Results { get; private set; } = new(); //Lista de resultados para cada tirada

    private void Start()
    {
        dices = GetComponentsInChildren<Dice>(true).ToList();
    }

    public void ToggleDiceThrow(bool enable) //Activa y desactiva la cámara que apunta al dado y se encarga de su lanzamiento
    {
        diceCamera.SetActive(enable);
        if (enable)
        {
            //La info sobre el número de lanzamientos y tipo se encuentra en los nombres de los posibles valores de los dropdown
            numFaces = int.Parse(typeDropdown.options[typeDropdown.value].text.Substring(1)); //Siguen formato "dxx", siendo xx el número de caras del dado
            numThrows = int.Parse(numDropdown.options[numDropdown.value].text); 
            GenerateResults(); //Se generan los resultados a partir de numFace y numThrows
            for (int i = 0; i < numThrows; i++)
            {
                //Se activan el número de dados equivalente a numThrows. Los dados se lanzan solos al ser activados
                dices[i].transform.parent.gameObject.SetActive(true); 
            }
        }
        else foreach (Dice d in dices) d.transform.parent.gameObject.SetActive(false);
    }

    public void NotifyDiceStopped()
    {
        //Cuando todos los dados lanzados notifican que han mostrado su resultado se enseñan los resultados
        if (--numThrows == 0) ShowResults(); 
    }

    public void ShowResults()
    {
        FindFirstObjectByType<DiceResults>(FindObjectsInactive.Include).gameObject.SetActive(true); //Se activa la pantalla de resultados
        skipButton.SetActive(false); //Se desactiva el botón de skip (lo activó el botón de lanzar dados al ser pulsado)
    }

    public int GetDiceResult(Dice dice) //Devuelve el resultado correspondiente a un dado
    {
        return Results[dices.IndexOf(dice)];
    }

    private void GenerateResults() //Se genera un resultado por lanzamiento
    {
        Results.Clear();
        for (int i = 0; i < numThrows; i++)
        {
            Results.Add(Random.Range(0, numFaces) + 1); //El resultado depende de numFaces
        }
    }
}
