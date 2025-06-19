using System.Collections;
using UnityEngine;

public class DebugOnAwake : MonoBehaviour
{
    void Awake()
    {
        StartCoroutine(SayTime());
    }

    IEnumerator SayTime()
    {
        float startTime = Time.time;
        yield return new WaitForSeconds(5);
        Debug.Log($"Tiempo transcurrido: {Time.time - startTime}s");
    }
}
