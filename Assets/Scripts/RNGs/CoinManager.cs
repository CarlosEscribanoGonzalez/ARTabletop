using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class CoinManager : MonoBehaviour
{
    [SerializeField] private GameObject coinCamera; //Cámara en overlay que apunta a las monedas desde arriba
    [SerializeField] private TMP_Dropdown numDropdown; //Dropdown con el número de monedas a lanzar
    [SerializeField] private GameObject skipButton; //Botón de saltar animación
    private int numThrows; //Número de monedas lanzadas simultáneamente
    private List<Coin> coins = new(); //Lista de los monedas lanzables
    public int NumHeads { get; private set; } = 0; //Número de monedas que han caído en cara
    public int NumTails { get; private set; } = 0; //Número de monedas que han caído en cruz

    private void Start()
    {
        coins = GetComponentsInChildren<Coin>(true).ToList();
    }

    public void ToggleCoinThrow(bool enable) //Activa y desactiva la cámara que apunta a la moneda y se encarga de su lanzamiento
    {
        coinCamera.SetActive(enable);
        if (enable)
        {
            //La info sobre el número de lanzamientos se encuentra en los nombres de los posibles valores de los dropdown
            numThrows = int.Parse(numDropdown.options[numDropdown.value].text);
            NumTails = 0; 
            NumHeads = 0;
            for (int i = 0; i < numThrows; i++)
            {
                //Se activan el número de monedas equivalente a numThrows. Las monedas se lanzan solas al ser activadas
                coins[i].transform.parent.gameObject.SetActive(true);
            }
            GetComponentInChildren<RNGCamera>().ZoomOutBackground();
        }
        else foreach (Coin c in coins) c.transform.parent.gameObject.SetActive(false);
    }

    public void NotifyCoinResult(CoinResult result)
    {
        //Cuando todas las monedas lanzadas notifican que han mostrado su resultado se enseñan los resultados
        if (result == CoinResult.Heads) NumHeads++;
        else NumTails++;
        if (NumHeads + NumTails == numThrows) ShowResults();
    }

    public void ShowResults()
    {
        Time.timeScale = 1;
        FindFirstObjectByType<CoinResultsUI>(FindObjectsInactive.Include).gameObject.SetActive(true); //Se activa la pantalla de resultados
        skipButton.SetActive(false); //Se desactiva el botón de skip (lo activó el botón de lanzar monedas al ser pulsado)
    }

    public void Skip()
    {
        Time.timeScale = 25;
    }

    public void ThrowAgain()
    {
        foreach (Coin c in coins) c.transform.parent.gameObject.SetActive(false);
        ToggleCoinThrow(true);
    }
}

public enum CoinResult
{
    Heads, 
    Tails
}
