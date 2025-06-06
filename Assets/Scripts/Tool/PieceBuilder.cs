using UnityEngine;
using UnityEngine.UI;
using Siccity.GLTFUtility;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class PieceBuilder : MonoBehaviour
{
    [SerializeField] private Button setAsDefaultButton;
    [SerializeField] private GameObject defaultPiece;
    [SerializeField] private TMP_Dropdown numPiecesDropdown;
    [SerializeField] private TextMeshProUGUI indexText;
    [SerializeField] private PiecePreview preview;
    [SerializeField] private PiecePreview defaultPreview;
    [SerializeField] private GameObject confirmationPanel;
    private List<GameObject> pieces = new();
    private int index = 0;
    private Dictionary<string, string> importedPaths = new();
    public GameObject DefaultPiece => defaultPiece;
    public int TotalPieces => pieces.Count;

    private void Awake()
    {
        for (int i = 0; i < numPiecesDropdown.value + 1; i++) pieces.Add(null);
        preview.SetPiece(defaultPiece);
        defaultPreview.SetPiece(defaultPiece);
    }

    public void UpdateIndex(int dir)
    {
        index += dir;
        if (index >= pieces.Count) index = 0;
        else if (index < 0) index = pieces.Count - 1;
        preview.SetPiece(pieces[index] ?? defaultPiece);
        indexText.text = (index + 1).ToString();
        setAsDefaultButton.interactable = (pieces[index] != null && !pieces[index].name.Equals(defaultPiece.name));
    }

    public void PickPiece(bool isDefaultPiece)
    {
        LoadingScreenManager.ToggleLoadingScreen(true);
        GameObject piecePrefab;
        NativeFilePicker.PickFile((path) =>
        {
            if (path != null)
            {
                Debug.Log("Archivo seleccionado: " + path);
                if (!path.EndsWith(".glb"))
                {
                    Debug.LogError("No se ha podido cargar un modelo: tipo de archivo incorrecto");
                    LoadingScreenManager.ToggleLoadingScreen(false);
                    return;
                }
                GameObject piece = Importer.LoadFromFile(path);
                piecePrefab = Instantiate(piece);
                DestroyImmediate(piece);
                piecePrefab.hideFlags = HideFlags.HideAndDontSave;
                piecePrefab.SetActive(false);
                piecePrefab.name = IDCreator.GetCustomModelID(Path.GetFileNameWithoutExtension(path), piecePrefab);
                if (isDefaultPiece)
                {
                    defaultPiece = piecePrefab;
                    defaultPreview.SetPiece(defaultPiece);
                }
                else pieces[index] = piecePrefab;
                preview.SetPiece(pieces[index] ?? defaultPiece);
                setAsDefaultButton.interactable = (pieces[index] != null && !pieces[index].name.Equals(defaultPiece.name));
                if (!importedPaths.ContainsKey(path)) importedPaths.Add(path, piecePrefab.name);
            }
            LoadingScreenManager.ToggleLoadingScreen(false);
        }, new string[] { "application/octet-stream" });
    }

    public void SetAsDefault()
    {
        pieces[index] = null;
        preview.SetPiece(defaultPiece);
        setAsDefaultButton.interactable = false;
    }

    private int newLength;
    public void UpdateLength(int value)
    {
        newLength = value + 1;
        if (pieces.Count > newLength)
        {
            numPiecesDropdown.SetValueWithoutNotify(pieces.Count - 1); //Se "cancela" el cambio a la espera de la confirmación
            confirmationPanel.SetActive(true);
        }
        else if (pieces.Count < newLength) while (pieces.Count < newLength) pieces.Add(null);
    }

    public void ConfirmChange()
    {
        pieces.RemoveRange(newLength, pieces.Count - newLength);
        numPiecesDropdown.SetValueWithoutNotify(newLength - 1);
        if (index >= pieces.Count)
        {
            index = pieces.Count - 1;
            preview.SetPiece(pieces[index] ?? defaultPiece);
        }
        indexText.text = (index + 1).ToString();
    }

    public List<GameObject> GetFinalPieces()
    {
        List<GameObject> notNullPieces = new();
        foreach (var p in pieces) if (p != null) notNullPieces.Add(p);
        return notNullPieces;
    }

    public void DownloadInfo()
    {
        foreach(var path in importedPaths.Keys)
        {
            if (ModelIsUsed(importedPaths[path]))
            {
                ContentDownloader.DownloadModel(path, importedPaths[path]);
            }
        }
    }

    private bool ModelIsUsed(string modelName)
    {
        if (modelName == defaultPiece.name) return true;
        foreach(var piece in GetFinalPieces())
        {
            if (modelName == piece.name) return true;
        }
        return false;
    }
}
