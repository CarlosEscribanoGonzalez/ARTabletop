using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Collections;

public class DetailedViewCardManager : MonoBehaviour
{
    [SerializeField] private DetailedViewCard detailedCard; //DetailedCard encargada de mostrar la información de la carta actualmente seleccionada
    [SerializeField] private RectTransform keptCardsContent; //Contenido del scroll rect de las cartas mantenidas
    [SerializeField] private GameObject keptCardPrefab; //Prefab de las cartas mantenidas
    [SerializeField] private GameObject blockingPanel; //Panel que bloquea la interacción del usuario cuando está detailed view activa
    [SerializeField] private GameObject infoPanel; //Panel de información sobre cómo usar las detailed views, activo una vez para cada jugador
    [SerializeField] private GameObject infoButton;
    private ScrollRect scrollRect; //ScrollRect de las cartas guardadas
    private Dictionary<CardInfo, GameObject> cardsKept = new(); //Diccionario de las cartas guardadas. Usan como clave las CardInfo de los CardManagers.
    private Volume dof; //Efecto de DoF cuando se abre detailed view
    private GameObject rngSection; //Botones de dados y ruleta
    private bool isInfoUnderstood = false;
    public bool IsInDetailedView => detailedCard.gameObject.activeSelf; //Devuelve si la carta detailedCard está siendo mostrada
    

    private void Awake()
    {
        dof = GetComponentInChildren<Volume>(true);
        rngSection = FindFirstObjectByType<RNGSection>().gameObject;
        scrollRect = GetComponentInChildren<ScrollRect>(true);
        isInfoUnderstood = PlayerPrefs.GetInt("InfoUnderstood", 0) == 0 ? false : true;
    }

    public void SetDetailedInfo(CardInfo info)
    {
        if (!isInfoUnderstood)
        {
            infoPanel.SetActive(true);
            infoButton.SetActive(false);
        }
        //Si hay una carta abierta y se pulsa una de las mantenidas la que estaba anteriormente se guarda antes de que la nueva se abra
        if (IsInDetailedView) KeepCard(detailedCard.Info); 
        ToggleView(true); //Se activa detailed view
        detailedCard.SetInfo(info); //Se aplica la info de la carta seleccionada
    }

    public void ToggleView(bool activate) //Activa y desactiva la vista detallada
    {
        detailedCard.gameObject.SetActive(activate); //Toggle de detailedCard
        dof.enabled = activate; //Toggle del efecto de DoF
        rngSection.SetActive(!activate); //La UI de los dados se debe desactivar si está abierta la vista detallada
        blockingPanel.SetActive(activate); //Se activa el panel que bloquea la interacción con los elementos que estén detrás
        if(isInfoUnderstood) infoButton.SetActive(activate);
    }

    public void KeepCard(CardInfo info) //Mantiene la carta actualmente mostrada
    {
        if (cardsKept.ContainsKey(info)) return; //Si ya está guardada no hace nada
        cardsKept.Add(info, Instantiate(keptCardPrefab, keptCardsContent)); //Crea una nueva instancia de carta almacenada y su entrada en el diccionario
        StartCoroutine(SetupKeptInfo(info)); //Hay que esperar hasta el final del frame para que se cree correctamente la carta
        scrollRect.gameObject.SetActive(true); //Si se ha añadido una carta se puede garantizar que al menos hay un elemento en el scroll rect. Este se activa
    }

    public void RemoveCard(CardInfo info) //Elimina una carta del scroll rect si su contenido estaba almacenado
    {
        if (cardsKept.ContainsKey(info))
        {
            Destroy(cardsKept[info]); //Se destruye la carta mantenida
            cardsKept.Remove(info); //Se elimina la entrada del diccionario
            if (cardsKept.Count == 0) scrollRect.gameObject.SetActive(false); //Si ya no quedan elementos se oculta el scroll rect
        }
    }

    public void OnInfoUnderstood()
    {
        PlayerPrefs.SetInt("InfoUnderstood", 1);
        isInfoUnderstood = true;
        infoButton.SetActive(true);
        infoPanel.SetActive(false);
    }

    IEnumerator SetupKeptInfo(CardInfo info)
    {
        yield return new WaitForEndOfFrame();
        cardsKept[info].SetActive(true);
        cardsKept[info].GetComponent<DetailedViewCard>().SetInfo(info); //Se le aplica la información necesaria a la carta creada
    }
}
