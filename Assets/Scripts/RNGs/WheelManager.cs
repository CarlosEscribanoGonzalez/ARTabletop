using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class WheelManager : MonoBehaviour
{
    [SerializeField] private Transform wheelTransform; //Transform de la rueda, con posici�n en su centro
    [SerializeField] private GameObject optionPrefab; //Prefab de las opciones puestas en la rueda
    [SerializeField] private Button addButton; //Bot�n de a�adir opci�n
    [SerializeField] private Button skipButton; //Bot�n de saltar animaci�n
    [SerializeField] private Button spinButton; //Bot�n de girar ruleta
    [SerializeField] private GameObject resultPanel; //Panel con el resultado
    private WheelListManager listManager; //Manager de la lista de opciones desplegada en la UI
    private Toggle removeWinnerToggle; //Toggle que configura la elminaci�n de la opci�n ganadora autom�ticamente
    [Header("Spin settings:")]
    [SerializeField] private float spinDuration = 5; //Duraci�n del giro
    [SerializeField] private Vector2 spinDegreesMinMax = new Vector2(1080, 3240); //M�nimo y m�ximo de giro para el lanzamiento
    [SerializeField] private int maxOptions = 12; //N�mero m�ximo de opciones
    private List<WheelOption> options = new(); //Lista con todas las opciones
    private float endRotation = 0; //Rotaci�n final de la ruleta, calculada cuando esta se lanza

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

    public void AddOption() //Se instancia una opci�n
    {
        WheelOption option = Instantiate(optionPrefab, wheelTransform).GetComponent<WheelOption>();
        option.Initialize(); //Se inicializa la opci�n
        options.Add(option); //Se a�ade a la lista
        UpdateWheel(); //Se actualiza la UI de la ruleta
    }

    public void RemoveOption(WheelOption option) //Borra una opci�n
    {
        options.Remove(option); //La elimina de la lista
        Destroy(option.gameObject); //Destroye el objeto
        UpdateWheel(false); //Actualiza la UI de la ruleta
    }

    public void SpinWheel() //Gira la ruleta, cuando se pulsa el bot�n de girar
    {
        addButton.interactable = false; //Se bloquea el bot�n de girar
        foreach (var listOption in listManager.Options)
                listOption.GetComponentInChildren<Button>().interactable = false; //Se bloquean los botones de eliminar opciones
        StartCoroutine(Spin(Random.Range(spinDegreesMinMax[0], spinDegreesMinMax[1]), spinDuration)); //Comienza la corrutina de giro
        wheelTransform.GetComponent<CanvasGroup>().alpha = 1; //La rueda se vuelve completamente visible
    }

    public void SkipAnimation() //Finaliza la animaci�n de girar, cuando se pulsa el bot�n de skip
    {
        StopAllCoroutines(); //Se para la animaci�n de giro
        wheelTransform.rotation = Quaternion.Euler(0f, 0f, endRotation); //Se pone a la ruleta en la posici�n final
        ShowResult(); //Se muestran los restultados
    }

    public void SetMenuState(bool inResults) //Establece el estado del men� entre resultados o default
    {
        addButton.interactable = options.Count < maxOptions; //El bot�n de a�adir se activa si el m�ximo de opciones no se ha alcanzado
        spinButton.interactable = options.Count > 0; //El bot�n de girar se activa si hay opciones disponibles
        wheelTransform.GetComponent<CanvasGroup>().alpha = (inResults ? 0.5f : 1); //Si est� en resultados, la ruleta se vuelve semitransparente
        resultPanel.SetActive(inResults); //Se activa o desactiva el panel con el resultado
        skipButton.gameObject.SetActive(false); //Se desactiva el bot�n de skip en todos los casos (se activa s�lo al pulsar el bot�n de girar)
        foreach (var listOption in listManager.Options)
            if (options.Count > 1) listOption.GetComponentInChildren<Button>(true).interactable = true; //Se activan los botones de eliminar opci�n siempre que haya m�s de una
    }

    private void UpdateWheel(bool updateDefaultNames = true) //Actualiza la ruleta con las opciones actuales
    {
        int numDefaultOptions = 1;
        for (int i = 0; i < options.Count; i++)
        {
            options[i].SetPosition(options.Count, i); //Se configura la posici�n de cada opci�n din�micamente
            //Si el nombre es un nombre por defecto se actualiza para que ninguna opci�n tenga el mismo n�mero
            //S�lo se actualizan al a�adir opciones, pues al eliminarlas puede dar lugar a confusiones (auto remove activo y sale dos veces de seguido el mismo n�mero...)
            if (options[i].Text.Contains("Option") && updateDefaultNames) options[i].Text = "Option " + numDefaultOptions++;
            listManager.AddOrUpdateOption(options[i]); //Se actualizan las opciones en la UI de la lista
        }
        SetMenuState(false); //Establece el estado en default (es decir, si desde los resultados se a�ade o elimina un elemento se pasa a estado default directamente
    }

    private void ShowResult() //Calcula y ense�a el resultado
    {
        //El resultado se calcula comprobando la alineaci�n de cada opci�n con el vector up (-1 -> desalineado; 1 -> alineado)
        float maxResult = -1; 
        float currentResult;
        WheelOption bestOption = null;
        foreach(WheelOption option in options)
        {
            currentResult = Vector3.Dot(option.GetResultDirection(), Vector3.up); //El resultado es el m�s alineado con el Vector up
            if (currentResult > maxResult)
            {
                bestOption = option;
                maxResult = currentResult;
            }
        }
        if (removeWinnerToggle.isOn && options.Count > 1) listManager.RemoveOption(bestOption); //Se elimina la opci�n ganadora en caso de que est� configurado
        resultPanel.GetComponentInChildren<TextMeshProUGUI>().text = bestOption.Text; //Se pone el ganador en el result panel
        SetMenuState(true); //Se activa el estado "Resultados"
    }

    private IEnumerator Spin(float totalDegrees, float time) //Corrutina encargada de hacer girar la rueda
    {
        float elapsed = 0f; //Tiempo que lleva girando
        float startRotation = wheelTransform.eulerAngles.z; //Rotaci�n inicial
        endRotation = startRotation + totalDegrees; //Rotaci�n final

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / time;
            float easedT = 1f - Mathf.Pow(1f - t, 3); //La rueda cada vez gira m�s lento
            float currentZ = Mathf.Lerp(startRotation, endRotation, easedT);
            wheelTransform.rotation = Quaternion.Euler(0f, 0f, currentZ);
            yield return null;
        }
        ShowResult(); //Ense�a el resultado
    }
}
