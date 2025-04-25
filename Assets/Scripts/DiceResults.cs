using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class DiceResults : MonoBehaviour
{
    [SerializeField] private Transform results; //Transform cuyos hijos son todos los resultados
    [SerializeField] private TextMeshProUGUI totalResultText; //Texto que muestra la suma de los resultados
    private Dice dice; //El dado
    private List<int> resultsList = new(); //Lista de resultados
    private int totalResult = 0; //Suma de los resultados

    private void Awake()
    {
        dice = FindFirstObjectByType<Dice>();
    }

    private void OnEnable()
    {
        resultsList = dice.Results; //Se obtiene la lista de resultados
        totalResult = 0;
        for(int i = 0; i < results.childCount; i++) //Se activan las casillas pertinentes a cada resultado a la vez que estos se van sumando
        {
            //Las casillas están en un Grid Layout Group, así que no hay que ajustar su posición, solamente si están o no activos
            if(i < resultsList.Count)
            {
                results.GetChild(i).gameObject.SetActive(true);
                results.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = resultsList[i].ToString();
                totalResult += resultsList[i];
            }
            else results.GetChild(i).gameObject.SetActive(false);
        }
        totalResultText.text = totalResult.ToString(); //Se escribe el resultado
    }
}
