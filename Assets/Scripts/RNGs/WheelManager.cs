using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class WheelManager : MonoBehaviour
{
    [SerializeField] private Transform wheelTransform; //Transform de la rueda, con posición en su centro
    [SerializeField] private GameObject optionPrefab; //Prefab de las opciones puestas en la rueda
    [SerializeField] private Button addButton; //Botón de añadir opción
    [SerializeField] private Button skipButton; //Botón de saltar animación
    [SerializeField] private Button spinButton; //Botón de girar ruleta
    [SerializeField] private GameObject resultPanel; //Panel con el resultado
    private WheelListManager listManager; //Manager de la lista de opciones desplegada en la UI
    private Toggle removeWinnerToggle; //Toggle que configura la elminación de la opción ganadora automáticamente
    [Header("Spin settings:")]
    [SerializeField] private float spinDuration = 5; //Duración del giro
    [SerializeField] private Vector2 spinDegreesMinMax = new Vector2(1080, 3240); //Mínimo y máximo de giro para el lanzamiento
    [SerializeField] private int maxOptions = 12; //Número máximo de opciones
    private List<WheelOption> options = new(); //Lista con todas las opciones
    private float endRotation = 0; //Rotación final de la ruleta, calculada cuando esta se lanza

    private void Start()
    {
        listManager = FindFirstObjectByType<WheelListManager>(FindObjectsInactive.Include);
        removeWinnerToggle = GetComponentInChildren<Toggle>(true);
        for(int i = 0; i < 3; i++) AddOption(); //Tres opciones iniciales por defecto
    }

    private void OnEnable()
    {
        SetMenuState(false);
    }

    public void AddOption() //Se instancia una opción
    {
        WheelOption option = Instantiate(optionPrefab, wheelTransform).GetComponent<WheelOption>();
        option.Initialize(); //Se inicializa la opción
        options.Add(option); //Se añade a la lista
        UpdateWheel(); //Se actualiza la UI de la ruleta
    }

    public void RemoveOption(WheelOption option) //Borra una opción
    {
        options.Remove(option); //La elimina de la lista
        Destroy(option.gameObject); //Destroye el objeto
        UpdateWheel(false); //Actualiza la UI de la ruleta
    }

    public void SpinWheel() //Gira la ruleta, cuando se pulsa el botón de girar
    {
        addButton.interactable = false; //Se bloquea el botón de girar
        foreach (var listOption in listManager.Options)
                listOption.GetComponentInChildren<Button>().interactable = false; //Se bloquean los botones de eliminar opciones
        StartCoroutine(Spin(Random.Range(spinDegreesMinMax[0], spinDegreesMinMax[1]), spinDuration)); //Comienza la corrutina de giro
        wheelTransform.GetComponent<CanvasGroup>().alpha = 1; //La rueda se vuelve completamente visible
    }

    public void SkipAnimation() //Finaliza la animación de girar, cuando se pulsa el botón de skip
    {
        StopAllCoroutines(); //Se para la animación de giro
        wheelTransform.rotation = Quaternion.Euler(0f, 0f, endRotation); //Se pone a la ruleta en la posición final
        ShowResult(); //Se muestran los restultados
    }

    public void SetMenuState(bool inResults) //Establece el estado del menú entre resultados o default
    {
        addButton.interactable = options.Count < maxOptions; //El botón de añadir se activa si el máximo de opciones no se ha alcanzado
        spinButton.interactable = options.Count > 0; //El botón de girar se activa si hay opciones disponibles
        wheelTransform.GetComponent<CanvasGroup>().alpha = (inResults ? 0.5f : 1); //Si está en resultados, la ruleta se vuelve semitransparente
        resultPanel.SetActive(inResults); //Se activa o desactiva el panel con el resultado
        skipButton.gameObject.SetActive(false); //Se desactiva el botón de skip en todos los casos (se activa sólo al pulsar el botón de girar)
        foreach (var listOption in listManager.Options)
            if (options.Count > 1) listOption.GetComponentInChildren<Button>(true).interactable = true; //Se activan los botones de eliminar opción siempre que haya más de una
    }

    private void UpdateWheel(bool updateDefaultNames = true) //Actualiza la ruleta con las opciones actuales
    {
        int numDefaultOptions = 1;
        for (int i = 0; i < options.Count; i++)
        {
            options[i].SetPosition(options.Count, i); //Se configura la posición de cada opción dinámicamente
            //Si el nombre es un nombre por defecto se actualiza para que ninguna opción tenga el mismo número
            //Sólo se actualizan al añadir opciones, pues al eliminarlas puede dar lugar a confusiones (auto remove activo y sale dos veces de seguido el mismo número...)
            if (options[i].Text.Contains("Option") && updateDefaultNames) options[i].Text = "Option " + numDefaultOptions++;
            listManager.AddOrUpdateOption(options[i]); //Se actualizan las opciones en la UI de la lista
        }
        SetMenuState(false); //Establece el estado en default (es decir, si desde los resultados se añade o elimina un elemento se pasa a estado default directamente
    }

    private void ShowResult() //Calcula y enseña el resultado
    {
        //El resultado se calcula comprobando la alineación de cada opción con el vector up (-1 -> desalineado; 1 -> alineado)
        float maxResult = -1; 
        float currentResult;
        WheelOption bestOption = null;
        foreach(WheelOption option in options)
        {
            currentResult = Vector3.Dot(option.GetResultDirection(), Vector3.up); //El resultado es el más alineado con el Vector up
            if (currentResult > maxResult)
            {
                bestOption = option;
                maxResult = currentResult;
            }
        }
        if (removeWinnerToggle.isOn && options.Count > 1) listManager.RemoveOption(bestOption); //Se elimina la opción ganadora en caso de que esté configurado
        resultPanel.GetComponentInChildren<TextMeshProUGUI>().text = bestOption.Text; //Se pone el ganador en el result panel
        SetMenuState(true); //Se activa el estado "Resultados"
    }

    private IEnumerator Spin(float totalDegrees, float time) //Corrutina encargada de hacer girar la rueda
    {
        float elapsed = 0f; //Tiempo que lleva girando
        float startRotation = wheelTransform.eulerAngles.z; //Rotación inicial
        endRotation = startRotation + totalDegrees; //Rotación final

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / time;
            float easedT = 1f - Mathf.Pow(1f - t, 3); //La rueda cada vez gira más lento
            float currentZ = Mathf.Lerp(startRotation, endRotation, easedT);
            wheelTransform.rotation = Quaternion.Euler(0f, 0f, currentZ);
            yield return null;
        }
        ShowResult(); //Enseña el resultado
    }
}
