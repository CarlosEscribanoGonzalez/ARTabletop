using UnityEngine;
using System.Collections;

public class Coin : MonoBehaviour
{
    [SerializeField] private Vector2 verticalThrustMinMax; //Mínimo y máximo de fuerza para el lanzamiento vertical de la moneda
    [SerializeField] private Vector2 angularThrustMinMax; //Mínimo y máximo de torque para su giro
    private Rigidbody rb;
    private CoinManager manager;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        manager = FindFirstObjectByType<CoinManager>();
    }

    public void OnEnable() //Se lanza la moneda y se calculan los resultados
    {
        ResetCoin();
        //Impulso vertical, dirección de giro y magnitud del torque son aleatorios para cada lanzamiento
        rb.AddForce(Vector3.up * Random.Range(verticalThrustMinMax[0], verticalThrustMinMax[1]), ForceMode.Impulse);
        Vector3 randomTorque = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        rb.AddTorque(randomTorque * Random.Range(angularThrustMinMax[0], angularThrustMinMax[1]), ForceMode.Impulse);
        StartCoroutine(CheckCoinStopped()); //Se empieza a comprobar periódicamente si ya ha parado de moverse
    }

    public void ResetCoin() //Se resetea el dado para que la animación de lanzamiento se haga correctamente
    {
        transform.localPosition = Vector3.zero;
        transform.rotation = Quaternion.identity * Quaternion.Euler(180 * Random.Range(0,2), 0, 0); //Aleatoriza aun más el resultado
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        StopAllCoroutines(); //Se paran todas las corrutinas para evitar bugs
    }

    private CoinResult CalculateResult()
    {
        if (Vector3.Dot(transform.up, Vector3.up) > 0) return CoinResult.Heads;
        else return CoinResult.Tails;
    }

    IEnumerator CheckCoinStopped() //Se comprueba cuándo ha parado el dado de girar para enseñar los resultados
    {
        yield return new WaitForSeconds(0.3f); //Un poco de retraso para evitar que por error se muestre el resultado antes de aplicar el movimiento
        do
            yield return new WaitForSeconds(0.1f);
        while (rb.linearVelocity.magnitude >= 0.005f); //Cada 0.1 segundos se comprueba si ya está prácticamente quieto
        StartCoroutine(ShowResult(CalculateResult())); //Cuando está quieta la moneda comunica su resultado
    }

    IEnumerator ShowResult(CoinResult result)
    {
        yield return new WaitForSeconds(0.2f); //Tras un pequeño cooldown se notifica el resultado
        manager.NotifyCoinResult(result);
    }
}
