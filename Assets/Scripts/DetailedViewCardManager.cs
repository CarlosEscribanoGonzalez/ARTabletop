using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class DetailedViewCardManager : MonoBehaviour
{
    [SerializeField] private DetailedViewCard detailedCard;
    [SerializeField] private RectTransform keptCardsContent;
    [SerializeField] private GameObject keptCardPrefab;
    [SerializeField] private GameObject blockingPanel;
    private ScrollRect scrollRect;
    private Dictionary<CardInfo, GameObject> cardsKept = new();
    private Volume dof;
    private GameObject rngSection;
    public bool IsInDetailedView => detailedCard.gameObject.activeSelf;

    private void Awake()
    {
        dof = GetComponentInChildren<Volume>(true);
        rngSection = FindFirstObjectByType<RNGSection>().gameObject;
        scrollRect = GetComponentInChildren<ScrollRect>(true);
    }

    public void SetDetailedInfo(CardInfo info)
    {
        if (IsInDetailedView) KeepCard(detailedCard.Info);
        ToggleView(true);
        detailedCard.SetInfo(info);
    }

    public void ToggleView(bool activate)
    {
        detailedCard.gameObject.SetActive(activate);
        dof.enabled = activate;
        rngSection.SetActive(!activate);
        blockingPanel.SetActive(activate);
    }

    public void KeepCard(CardInfo info)
    {
        if (cardsKept.ContainsKey(info)) return;
        cardsKept.Add(info, Instantiate(keptCardPrefab, keptCardsContent));
        cardsKept[info].GetComponent<DetailedViewCard>().SetInfo(info);
        scrollRect.gameObject.SetActive(true);
    }

    public void RemoveCard(CardInfo info)
    {
        if (cardsKept.ContainsKey(info))
        {
            Destroy(cardsKept[info]);
            cardsKept.Remove(info);
            if (cardsKept.Count == 0) scrollRect.gameObject.SetActive(false);
        }
    }
}
