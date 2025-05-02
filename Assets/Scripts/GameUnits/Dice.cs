using UnityEngine;
using TMPro;
using System.Collections;

public class Dice : MonoBehaviour
{
    [SerializeField] private Vector2 verticalThrustMinMax; //M�nimo y m�ximo de fuerza para el lanzamiento vertical del dado
    [SerializeField] private Vector2 angularThrustMinMax; //M�nimo y m�ximo de torque para su giro
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
        //Impulso vertical, direcci�n de giro y magnitud del torque son aleatorios para cada lanzamiento:
        rb.AddForce(Vector3.up * Random.Range(verticalThrustMinMax[0], verticalThrustMinMax[1]), ForceMode.Impulse);
        Vector3 randomTorque = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        rb.AddTorque(randomTorque * Random.Range(angularThrustMinMax[0], angularThrustMinMax[1]), ForceMode.Impulse);
        StartCoroutine(CheckDiceStopped());
    }

    public void ResetDice() //Se resetea el dado para que la animaci�n de lanzamiento se haga correctamente
    {
        transform.localPosition = Vector3.zero;
        transform.rotation = Random.rotation;
        foreach (var num in numbers)
        {
            num.text = "?";
            num.fontStyle &= ~FontStyles.Underline; //Se quita el subrayado, activado por algunos resultados
        }
        StopAllCoroutines(); //Se paran todas las corrutinas para evitar bugs
    }

    IEnumerator CheckDiceStopped() //Se comprueba cu�ndo ha parado el dado de girar para ense�ar los resultados
    {
        do 
            yield return new WaitForSeconds(0.1f); 
        while (rb.linearVelocity.magnitude >= 0.025f);
        StartCoroutine(ShowResult(manager.GetDiceResult(this)));
    }

    IEnumerator ShowResult(int result)
    {
        while (numbers[0].color.a > 0) //Los textos, que muestran "?", se vuelven gradualmente invisibles
        {
            yield return null;
            foreach(var num in numbers)
            {
                num.color -= new Color(0, 0, 0, textFadeSpeed * Time.deltaTime);
            }
        }
        foreach (var num in numbers) //Se aplica el resultado a los textos
        {
            num.text = result.ToString();
            //Algunos resultados necesitan subrayado para diferenciarse entre s�
            if(result == 6 || result == 9 || result == 69 || result == 96) num.fontStyle |= FontStyles.Underline;
            else num.fontStyle &= ~FontStyles.Underline;
        }
        while (numbers[0].color.a < 1) //Se les devuelve la visibilidad a los textos
        {
            yield return null;
            foreach (var num in numbers)
            {
                num.color += new Color(0, 0, 0, textFadeSpeed * Time.deltaTime);
            }
        }
        yield return new WaitForSeconds(0.2f); //Tras un peque�o cooldown se activa la pantalla de resultados
        manager.NotifyDiceStopped();
    }
}
