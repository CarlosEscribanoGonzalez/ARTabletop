using UnityEngine;
using TMPro;
using System.Collections;

public class Dice : MonoBehaviour
{
    [SerializeField] private Vector2 verticalThrustMinMax; //Mínimo y máximo de fuerza para el lanzamiento vertical del dado
    [SerializeField] private Vector2 angularThrustMinMax; //Mínimo y máximo de torque para su giro
    [SerializeField] private float textFadeSpeed; //Velocidad a la que se muestra el resultado en el dado
    private Rigidbody rb;
    private TextMeshPro [] numbers; //Textos del dado, uno por cara
    private DiceManager manager;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        numbers = GetComponentsInChildren<TextMeshPro>();
        manager = FindFirstObjectByType<DiceManager>();
    }

    public void OnEnable() //Se lanza el dado y se calculan los resultados
    {
        ResetDice();
        //Impulso vertical, dirección de giro y magnitud del torque son aleatorios para cada lanzamiento:
        rb.AddForce(Vector3.up * Random.Range(verticalThrustMinMax[0], verticalThrustMinMax[1]), ForceMode.Impulse);
        Vector3 randomTorque = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        rb.AddTorque(randomTorque * Random.Range(angularThrustMinMax[0], angularThrustMinMax[1]), ForceMode.Impulse);
        StartCoroutine(CheckDiceStopped()); //Se empieza a comprobar periódicamente si ya ha parado de moverse
    }

    public void ResetDice() //Se resetea el dado para que la animación de lanzamiento se haga correctamente
    {
        transform.localPosition = Vector3.zero;
        transform.rotation = Random.rotation; //Aleatoriza aun más el resultado
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        foreach (var num in numbers) //Se resetean los textos
        {
            num.text = "?";
            num.fontStyle &= ~FontStyles.Underline; //Se quita el subrayado, activado por algunos resultados
            num.color = num.color + new Color(0, 0, 0, 1); //Se pone de nuevo en opaco el número
        }
        StopAllCoroutines(); //Se paran todas las corrutinas para evitar bugs
    }

    IEnumerator CheckDiceStopped() //Se comprueba cuándo ha parado el dado de girar para enseñar los resultados
    {
        yield return new WaitForSeconds(0.3f); //Un poco de retraso para evitar que por error se muestre el resultado antes de aplicar el movimiento
        do 
            yield return new WaitForSeconds(0.1f); 
        while (rb.linearVelocity.magnitude >= 0.025f); //Cada 0.1 segundos se comprueba si ya está prácticamente quieto
        StartCoroutine(ShowResult(manager.GetDiceResult(this))); //Cuando está quieto el dado muestra su resultado, proporcionado por el manager
    }

    IEnumerator ShowResult(int result)
    {
        while (numbers[0].color.a > 0) //Los textos, que muestran "?", se vuelven gradualmente invisibles
        {
            foreach(var num in numbers) //Se le da transparencia progresiva a los números
            {
                num.color -= new Color(0, 0, 0, textFadeSpeed * Time.deltaTime);
            }
            yield return null;
        }
        foreach (var num in numbers) //Se aplica el resultado a los textos
        {
            num.text = result.ToString();
            //Algunos resultados necesitan subrayado para diferenciarse entre sí
            if(result == 6 || result == 9 || result == 69 || result == 96) num.fontStyle |= FontStyles.Underline;
        }
        while (numbers[0].color.a < 1) //Se les devuelve la visibilidad a los textos
        {
            foreach (var num in numbers)
            {
                num.color += new Color(0, 0, 0, textFadeSpeed * Time.deltaTime);
            }
            yield return null;
        }
        yield return new WaitForSeconds(0.2f); //Tras un pequeño cooldown se notifica que el resultado ha sido mostrado
        manager.NotifyDiceStopped();
    }
}
