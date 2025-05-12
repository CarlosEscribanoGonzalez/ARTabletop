using UnityEngine;

public class LayoutManager : MonoBehaviour
{
    [SerializeField] private Transform layoutInPortrait;
    [SerializeField] private Transform layoutInLandscape;
    [SerializeField] private Transform[] content;
    [SerializeField] private float portraitSizeMult;
    [SerializeField] private float landscapeSizeMult;
    private ScreenOrientation orientation;
    
    void OnEnable()
    {
        orientation = Screen.orientation;
        UpdateLayout();
    }

    void Update()
    {
        if(orientation != Screen.orientation)
        {
            orientation = Screen.orientation;
            UpdateLayout();
        }
    }

    private void UpdateLayout()
    {
        if (orientation == ScreenOrientation.Portrait || orientation == ScreenOrientation.PortraitUpsideDown)
            ChangeLayout(layoutInPortrait);
        else ChangeLayout(layoutInLandscape);
    }

    private void ChangeLayout(Transform newLayout)
    {
        layoutInPortrait.localScale = Vector3.one;
        layoutInLandscape.localScale = Vector3.one;
        foreach (Transform t in content)
        {
            t.parent = newLayout;
            t.SetAsLastSibling();
        }
        layoutInPortrait.localScale *= portraitSizeMult;
        layoutInLandscape.localScale *= landscapeSizeMult;
    }
}
