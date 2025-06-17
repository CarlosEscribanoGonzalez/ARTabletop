#if UNITY_EDITOR
using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

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
    [SerializeField] private Color centralTextsColor = Color.white;
    [Header("Saving configuration: ")]
    [SerializeField] private string folderName = "GeneratedMarkers";
    [SerializeField] private Vector2 imageDimensions = new Vector2(1000, 1400);
    [SerializeField] private float backgroundRemoveTolerance = 0.7f;
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
        typeText.color = centralTextsColor;
        numberText.color = centralTextsColor;
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
        Texture2D texture = new Texture2D((int)imageDimensions[0], (int)imageDimensions[1], TextureFormat.RGBA32, false);
        cam.Render();
        RenderTexture.active = rt;
        texture.ReadPixels(new Rect(0, 0, (int)imageDimensions[0], (int)imageDimensions[1]), 0, 0);
        texture.Apply();
        Color[] textureWithoutBackground = EliminateBackground(texture);
        texture.SetPixels(textureWithoutBackground);
        texture.Apply();
        cam.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);
        string folderPath = Path.Combine(Application.dataPath, "Markers");
        folderPath = Path.Combine(folderPath, folderName);
        Directory.CreateDirectory(folderPath);
        string filename = $"marker_{markerType}{number}.png";
        string fullPath = Path.Combine(folderPath, filename);
        File.WriteAllBytes(fullPath, texture.EncodeToPNG());
        Debug.Log("IMAGEN CORRECTAMENTE GUARDADA EN " + fullPath);
    }

    private Color[] EliminateBackground(Texture2D texture)
    {
        Color bgColor = cam.backgroundColor;
        Color[] pixels = texture.GetPixels();
        int width = texture.width;
        int height = texture.height;
        bool[] visited = new bool[pixels.Length];
        Queue<int> queue = new Queue<int>();

        int[] startIndices = {
        0, width - 1,
        (height - 1) * width, height * width - 1
        };

        foreach (int idx in startIndices)
        {
            if (IsSimilar(pixels[idx], bgColor, backgroundRemoveTolerance))
            {
                queue.Enqueue(idx);
                visited[idx] = true;
            }
        }

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            Color currentPixel = pixels[current];

            if (IsSimilar(currentPixel, Color.black, 0.05f))
                continue;

            if (IsSimilar(currentPixel, Color.black, 1 - backgroundRemoveTolerance))
                pixels[current] = Color.black;
            else if (IsSimilar(currentPixel, bgColor, backgroundRemoveTolerance))
                pixels[current] = new Color(0, 0, 0, 0);

            int x = current % width;
            int y = current / width;
            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            for (int i = 0; i < 4; i++)
            {
                int nx = x + dx[i];
                int ny = y + dy[i];

                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                {
                    int neighborIndex = ny * width + nx;

                    if (!visited[neighborIndex])
                    {
                        Color neighborPixel = pixels[neighborIndex];

                        if (IsSimilar(neighborPixel, bgColor, backgroundRemoveTolerance))
                        {
                            queue.Enqueue(neighborIndex);
                            visited[neighborIndex] = true;
                        }
                    }
                }
            }
        }

        return pixels;
    }

    private bool IsSimilar(Color a, Color b, float tolerance)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
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
#endif