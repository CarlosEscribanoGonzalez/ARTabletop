using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class DiceResults : MonoBehaviour
{
    [SerializeField] private Transform results;
    [SerializeField] private TextMeshProUGUI totalResultText;
    private Dice dice;
    private List<int> resultsList = new();
    private int totalResult = 0;

    private void Awake()
    {
        dice = FindFirstObjectByType<Dice>();
    }

    private void OnEnable()
    {
        resultsList = dice.Results;
        totalResult = 0;
        for(int i = 0; i < results.childCount; i++)
        {
            if(i < resultsList.Count)
            {
                results.GetChild(i).gameObject.SetActive(true);
                results.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = resultsList[i].ToString();
                totalResult += resultsList[i];
            }
            else results.GetChild(i).gameObject.SetActive(false);
        }
        totalResultText.text = totalResult.ToString();
    }
}
