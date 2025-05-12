using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using System.Collections;

public class WheelOptionList : MonoBehaviour
{
    [SerializeField] private GameObject optionPrefab;
    [SerializeField] private Transform contentTransform;
    private Dictionary<WheelOption, GameObject> optionsDictionary = new();
    private WheelManager wheelManager;
    public List<GameObject> Options { get { return optionsDictionary.Values.ToList(); } }

    private void Awake()
    {
        wheelManager = FindFirstObjectByType<WheelManager>();
    }

    public void AddOrUpdateOption(WheelOption option)
    {
        if (!optionsDictionary.ContainsKey(option))
        {
            GameObject element = Instantiate(optionPrefab, contentTransform);
            optionsDictionary.Add(option, element);
            element.GetComponentInChildren<TMP_InputField>().onValueChanged.AddListener((text) => option.Text = text);
            element.GetComponentInChildren<Button>().onClick.AddListener(() => RemoveOption(option));
        }
        optionsDictionary[option].GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(option.Text);
    }
    
    public void RemoveOption(WheelOption option)
    {
        wheelManager.RemoveOption(option);
        Destroy(optionsDictionary[option]);
        if (contentTransform.childCount <= 2) StartCoroutine(CheckRemoveButtonInteractable()); //El objeto no se destruye hasta el final del frame
        optionsDictionary.Remove(option);
    }

    IEnumerator CheckRemoveButtonInteractable()
    {
        yield return new WaitForEndOfFrame();
        contentTransform.GetComponentInChildren<Button>().interactable = false;
    }
}
