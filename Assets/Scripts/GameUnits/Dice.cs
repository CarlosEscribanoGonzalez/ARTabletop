using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Dice : MonoBehaviour
{
    [SerializeField] private Vector2 verticalThrustMinMax;
    [SerializeField] private Vector2 angularThrustMinMax;
    [SerializeField] private float textFadeSpeed;
    public List<int> Results { get; private set; } = new();
    private Rigidbody rb;
    private TextMeshPro [] numbers;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        numbers = GetComponentsInChildren<TextMeshPro>();
    }

    public void ThrowDice(int numFaces, int numThrows)
    {
        ResetDice();
        rb.AddForce(Vector3.up * Random.Range(verticalThrustMinMax[0], verticalThrustMinMax[1]), ForceMode.Impulse);
        Vector3 randomTorque = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        rb.AddTorque(randomTorque * Random.Range(angularThrustMinMax[0], angularThrustMinMax[1]), ForceMode.Impulse);
        GenerateResults(numFaces, numThrows);
        StartCoroutine(CheckDiceStopped());
    }

    public void ResetDice()
    {
        transform.localPosition = Vector3.zero;
        transform.rotation = Random.rotation;
        foreach (var num in numbers)
        {
            num.text = "?";
            num.fontStyle &= ~FontStyles.Underline;
        }
        StopAllCoroutines();
    }

    private void GenerateResults(int numFaces, int numThrows)
    {
        Results.Clear();
        for(int i = 0; i < numThrows; i++)
        {
            Results.Add(Random.Range(0, numFaces) + 1);
        }
    }

    IEnumerator CheckDiceStopped()
    {
        do 
            yield return new WaitForSeconds(0.1f); 
        while (rb.linearVelocity.magnitude >= 0.05f);
        StartCoroutine(ShowResult(Results[0]));
    }

    IEnumerator ShowResult(int result)
    {
        while (numbers[0].color.a > 0)
        {
            yield return null;
            foreach(var num in numbers)
            {
                num.color -= new Color(0, 0, 0, textFadeSpeed * Time.deltaTime);
            }
        }
        foreach (var num in numbers)
        {
            num.text = result.ToString();
            if(result == 6 || result == 9 || result == 69 || result == 96) num.fontStyle |= FontStyles.Underline;
            else num.fontStyle &= ~FontStyles.Underline;
        }
        while (numbers[0].color.a < 1)
        {
            yield return null;
            foreach (var num in numbers)
            {
                num.color += new Color(0, 0, 0, textFadeSpeed * Time.deltaTime);
            }
        }
        yield return new WaitForSeconds(0.2f);
        FindFirstObjectByType<DiceResults>(FindObjectsInactive.Include).gameObject.SetActive(true);
    }
}
