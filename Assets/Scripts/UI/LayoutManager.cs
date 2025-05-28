using UnityEngine;
using System.Collections.Generic;

public class LayoutManager : MonoBehaviour
{
    [SerializeField] private Transform layoutInPortrait; //Objeto con un layout group, padre del contenido cuando se está en portrait
    [SerializeField] private Transform layoutInLandscape; //Objeto con un layout group, padre del contenido cuando se está en landscape
    [SerializeField] private List<Transform> content = new(); //Todo el contenido a desplegar en los layout groups
    [SerializeField] private float portraitSizeMult = 1; //Multiplicador de tamaño del contenido cuando está en portrait
    [SerializeField] private float landscapeSizeMult = 1; //Multiplicador de tamaño del contenido cuando está en landscape
    [SerializeField] private GameObject objToHidePortrait; //Objeto que se oculta en portrait
    [SerializeField] private GameObject objToHideLandscape; //Objeto que se oculta en landscape
    private ScreenOrientation orientation; //Orientación almacenada de la pantalla
    
    void OnEnable() //Se registra la orientación y se actualiza el layout según ella
    {
        orientation = Screen.orientation; 
        UpdateLayout();
    }

    void Update() //Cuando el dispositivo gira se actualizan orientation y el layout
    {
        if(orientation != Screen.orientation)
        {
            orientation = Screen.orientation;
            UpdateLayout();
        }
    }

    public void AddContent(Transform newContent)
    {
        content.Add(newContent);
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
    }

    private void ChangeLayout(Transform newLayout) //Cambia el contenido de padre para que se visualice bien dependiendo de la orientación
    {
        //Se resetea la escala de los layoutGroup para que su cambio de tamaño no afecte al contenido
        layoutInPortrait.localScale = Vector3.one;
        layoutInLandscape.localScale = Vector3.one;
        foreach (Transform t in content) t.parent = newLayout; //Se cambia el contenido de layout group
        //Se aplican de nuevo las escalas
        layoutInPortrait.localScale *= portraitSizeMult;
        layoutInLandscape.localScale *= landscapeSizeMult;
        if(objToHidePortrait != null) objToHidePortrait.SetActive(newLayout == layoutInLandscape);
        if(objToHideLandscape != null) objToHideLandscape.SetActive(newLayout == layoutInPortrait);
    }
}
