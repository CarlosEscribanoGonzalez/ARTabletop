using UnityEngine;
using TMPro;

public class CoinResultsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI totalHeadsText; //Texto que muestra la suma de las caras
    [SerializeField] private TextMeshProUGUI totalTailsText; //Texto que muestra la suma de las cruces
    private CoinManager manager; 

    private void Awake()
    {
        manager = FindFirstObjectByType<CoinManager>();
    }

    private void OnEnable()
    {
        //El background de la cámara es traído al frente para que se lean bien los resultados
        manager.GetComponentInChildren<RNGCamera>(true).ZoomBackground();
        totalHeadsText.text = "x" + manager.NumHeads.ToString(); //Se escribe el resultado de las caras
        totalTailsText.text = "x" + manager.NumTails.ToString(); //Se escribe el resultado de las cruces
    }
}
