using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using UnityEditor;
using System.IO;

[ExecuteInEditMode]
public class MarkerCreator : MonoBehaviour
{
    [SerializeField] private TextMeshPro typeText;
    [SerializeField] private TextMeshPro numberText;
    [SerializeField] private GameObject backgroundParent;
    [SerializeField] private GameObject textsParent;
    [Header("Generation configuration: ")]
    [SerializeField] private string markerType;
    [SerializeField] private string number;
    [SerializeField] private string abbreviation;
    [Header("Saving configuration: ")]
    [SerializeField] private string folderName = "GeneratedMarkers";
    [SerializeField] private Vector2 imageDimensions = new Vector2(1000, 1400);
    private TextMeshProUGUI[] texts;
    private SpriteRenderer[] backgroundSprites;
    private Camera cam;

    private void OnEnable()
    {
        texts = textsParent.GetComponentsInChildren<TextMeshProUGUI>();
        backgroundSprites = backgroundParent.GetComponentsInChildren<SpriteRenderer>();
        cam = Camera.main;
    }

    public void CreateMarker()
    {
        typeText.text = markerType;
        numberText.text = number;
        foreach(SpriteRenderer rend in backgroundSprites)
            rend.color = new Color(Random.value, Random.value, Random.value, 1);
        foreach(TextMeshProUGUI textMesh in texts)
        {
            textMesh.color = new Color(Random.value, Random.value, Random.value, 1);
            textMesh.fontStyle = FontStyles.Normal;
            if(Random.value > 0.5f) textMesh.fontStyle |= FontStyles.Italic;
            if(Random.value > 0.5f) textMesh.fontStyle |= FontStyles.Bold;
            textMesh.text = abbreviation + number;
            //textMesh.transform.localEulerAngles = new Vector3(Random.Range(-90, 91), Random.Range(-90, 91), Random.Range(0, 360));
            textMesh.transform.localEulerAngles = new Vector3(0, 0, Random.Range(-180, 180));
            textMesh.GetComponent<SortingGroup>().sortingOrder = Random.Range(0, 100);
        }
    }

    public void GetNextOrPrevMarker(int dir)
    {
        int num = int.Parse(number) + dir;
        if (num < 1) return;
        number = num.ToString();
        if (num < 10) number = "0" + number;
        CreateMarker();
    }

    public void Capture()
    {
        RenderTexture rt = new RenderTexture((int)imageDimensions[0], (int)imageDimensions[1], 24);
        cam.targetTexture = rt;
        Texture2D screenshot = new Texture2D((int)imageDimensions[0], (int)imageDimensions[1], TextureFormat.RGB24, false);
        cam.Render();
        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, (int)imageDimensions[0], (int)imageDimensions[1]), 0, 0);
        screenshot.Apply();
        cam.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);
        string folderPath = Path.Combine(Application.dataPath, folderName);
        Directory.CreateDirectory(folderPath);
        string filename = $"marker_{markerType}{number}.png";
        string fullPath = Path.Combine(folderPath, filename);
        File.WriteAllBytes(fullPath, screenshot.EncodeToPNG());
        Debug.Log("IMAGEN CORRECTAMENTE GUARDADA EN " + fullPath);
    }
}

[CustomEditor(typeof(MarkerCreator))]
public class MarkerCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MarkerCreator creator = (MarkerCreator)target;
        GUILayout.Space(10);
        if (GUILayout.Button("UPDATE ALL VALUES")) creator.CreateMarker();
        GUILayout.Space(10);
        if (GUILayout.Button("GET NEXT MARKER")) creator.GetNextOrPrevMarker(1);
        GUILayout.Space(10);
        if (GUILayout.Button("GET PREV MARKER")) creator.GetNextOrPrevMarker(-1);
        GUILayout.Space(10);
        if (GUILayout.Button("SAVE MARKER")) creator.Capture();
    }
}
