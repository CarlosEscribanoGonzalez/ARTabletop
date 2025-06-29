using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class LayoutManager : MonoBehaviour
{
    [SerializeField] private Transform layoutInPortrait; //Objeto con un layout group, padre del contenido cuando se está en portrait
    [SerializeField] private Transform layoutInLandscape; //Objeto con un layout group, padre del contenido cuando se está en landscape
    [SerializeField] private List<Transform> content = new(); //Todo el contenido a desplegar en los layout groups
    [SerializeField] private float portraitSizeMult = 1; //Multiplicador de tamaño del contenido cuando está en portrait
    [SerializeField] private float landscapeSizeMult = 1; //Multiplicador de tamaño del contenido cuando está en landscape
    [SerializeField] private GameObject objToHidePortrait; //Objeto que se oculta en portrait
    [SerializeField] private GameObject objToHideLandscape; //Objeto que se oculta en landscape
    private Canvas canvas;
    private ScreenOrientation orientation; //Orientación almacenada de la pantalla
    public System.Action OnLayoutUpdated;

    void OnEnable() //Se registra la orientación y se actualiza el layout según ella
    {
        canvas = GetComponentInParent<Canvas>();
        if (!canvas.enabled) return;
        orientation = Screen.orientation;
        StartCoroutine(ResetScrollViews());
        UpdateLayout();
    }

    void Update() //Cuando el dispositivo gira se actualizan orientation y el layout
    {
        if (!canvas.enabled) return;
        if(orientation != Screen.orientation)
        {
            orientation = Screen.orientation;
            UpdateLayout();
        }
    }

    public void AddContent(Transform newContent)
    {
        content.Add(newContent);
        newContent.SetSiblingIndex(content.Count - 1);
    }

    public void RemoveContent(Transform contentToRemove)
    {
        content.Remove(contentToRemove);
    }

    public Transform GetCurrentLayoutTransform()
    {
        if (orientation == ScreenOrientation.LandscapeLeft || orientation == ScreenOrientation.LandscapeRight) return layoutInLandscape;
        else return layoutInPortrait;
    }

    private void UpdateLayout() //Actualiza el layout dependiendo de la orientación de la pantalla
    {
        if (orientation == ScreenOrientation.Portrait || orientation == ScreenOrientation.PortraitUpsideDown)
            ChangeLayout(layoutInPortrait);
        else ChangeLayout(layoutInLandscape);
        OnLayoutUpdated?.Invoke();
        StartCoroutine(ResetScrollViews());
    }

    private void ChangeLayout(Transform newLayout) //Cambia el contenido de padre para que se visualice bien dependiendo de la orientación
    {
        //Se resetea la escala de los layoutGroup para que su cambio de tamaño no afecte al contenido
        layoutInPortrait.localScale = Vector3.one;
        layoutInLandscape.localScale = Vector3.one;
        for(int i = 0; i < content.Count; i++)
        {
            content[i].parent = newLayout; //Se cambia el contenido de layout group
            content[i].SetSiblingIndex(i);
        }
        //Se aplican de nuevo las escalas
        if (layoutInPortrait != layoutInLandscape)
        {
            layoutInPortrait.localScale *= portraitSizeMult;
            layoutInLandscape.localScale *= landscapeSizeMult;
        }
        else
        {
            if (orientation == ScreenOrientation.Portrait || orientation == ScreenOrientation.PortraitUpsideDown)
                layoutInPortrait.localScale *= portraitSizeMult;
            else layoutInLandscape.localScale *= landscapeSizeMult;
        }
        if(objToHidePortrait != null) objToHidePortrait.SetActive(newLayout == layoutInLandscape);
        if(objToHideLandscape != null) objToHideLandscape.SetActive(newLayout == layoutInPortrait);
    }

    IEnumerator ResetScrollViews()
    {
        yield return null;
        foreach (var scroll in GetComponentsInChildren<ScrollRect>(true))
        {
            if (scroll.horizontal) yield return null;
            float prevElasticity = scroll.elasticity;
            scroll.elasticity = 0;
            scroll.verticalNormalizedPosition = 1;
            scroll.horizontalNormalizedPosition = 0;
            yield return null;
            scroll.elasticity = prevElasticity;
        }
    }
}
