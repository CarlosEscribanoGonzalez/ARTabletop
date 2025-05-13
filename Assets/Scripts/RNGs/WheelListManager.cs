using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using System.Collections;

public class WheelListManager : MonoBehaviour
{
    [SerializeField] private GameObject optionPrefab; //Prefab de los elementos de la lista
    [SerializeField] private Transform contentTransform; //Transform padre de los elementos de la lista (en el scroll rect)
    private Dictionary<WheelOption, GameObject> optionsDictionary = new(); //Diccionario de las opciones de la ruleta y su entrada correspondiente en la lista
    private WheelManager wheelManager;
    public List<GameObject> Options { get { return optionsDictionary.Values.ToList(); } } //Devuelve las opciones presentes en la lista

    private void Awake()
    {
        wheelManager = FindFirstObjectByType<WheelManager>();
    }

    public void AddOrUpdateOption(WheelOption option) //Si la opci�n no est� a�adida la a�ade, si no la actualiza
    {
        if (!optionsDictionary.ContainsKey(option)) //A�ade una nueva opci�n a la lista
        {
            GameObject element = Instantiate(optionPrefab, contentTransform); //Instancia el elemento
            optionsDictionary.Add(option, element); //A�ade la entrada al diccionario
            //Asocia funciones a los interactuables del elemento:
            element.GetComponentInChildren<TMP_InputField>().onValueChanged.AddListener((text) => option.Text = text);
            element.GetComponentInChildren<Button>().onClick.AddListener(() => RemoveOption(option));
        }
        //En todos los casos, actualiza el input field en caso de que su valor haya cambiado (en WheelManager)
        optionsDictionary[option].GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(option.Text);
    }
    
    public void RemoveOption(WheelOption option) //Borra una opci�n dada
    {
        wheelManager.RemoveOption(option); //Se borra la opci�n de la ruleta
        Destroy(optionsDictionary[option]); //Se elimina el elemento de la lista
        if (contentTransform.childCount <= 2) //Se consideran 2 y no 1 porque el objeto no es destruido hasta el final del frame
            StartCoroutine(DisableRemoveLastElement()); //Se controla que si s�lo queda un elemento en la lista su bot�n de eliminar no funcione
        optionsDictionary.Remove(option); //Se elimina la inserci�n del diccionario
    }

    IEnumerator DisableRemoveLastElement() 
    {
        yield return new WaitForEndOfFrame(); //Espera a que pase un frame y se efect�e la eliminaci�n del elemento destruido en RemoveOption
        contentTransform.GetComponentInChildren<Button>().interactable = false; //Deshabilita el bot�n de eliminar en el �nico elemento de la lista restante
    }
}
