using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class DiceResults : MonoBehaviour
{
    [SerializeField] private Transform results; //Transform cuyos hijos son todos los resultados
    [SerializeField] private TextMeshProUGUI totalResultText; //Texto que muestra la suma de los resultados
    private DiceManager manager; //El dado
    private int totalResult = 0; //Suma de los resultados

    private void Awake()
    {
        manager = FindFirstObjectByType<DiceManager>();
    }

    private void OnEnable()
    {
        //El background de la cámara es traído al frente para que se lean bien los resultados
        FindFirstObjectByType<DiceCamera>(FindObjectsInactive.Include).ZoomBackground(); 
        totalResult = 0;
        for(int i = 0; i < results.childCount; i++) //Se activan las casillas pertinentes a cada resultado a la vez que estos se van sumando
        {
            //Las casillas están en un Grid Layout Group, así que no hay que ajustar su posición, solamente si están o no activos
            if(i < manager.Results.Count)
            {
                results.GetChild(i).gameObject.SetActive(true);
                results.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = manager.Results[i].ToString();
                totalResult += manager.Results[i];
            }
            else results.GetChild(i).gameObject.SetActive(false);
        }
        totalResultText.text = totalResult.ToString(); //Se escribe el resultado total
    }
}
