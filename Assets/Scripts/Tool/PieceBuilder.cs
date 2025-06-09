using UnityEngine;
using UnityEngine.UI;
using Siccity.GLTFUtility;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class PieceBuilder : ABuilder<GameObject>
{
    [SerializeField] private Button setAsDefaultButton;
    [SerializeField] private GameObject defaultPiece;
    [SerializeField] private PiecePreview defaultPreview;
    private Dictionary<string, string> importedPaths = new();
    public GameObject DefaultPiece => defaultPiece;
    public int TotalPieces => Content.Count;

    private void Awake()
    {
        for (int i = 0; i < contentDropdown.value + 1; i++) Content.Add(null);
        preview.UpdateValues(defaultPiece);
        defaultPreview.UpdateValues(defaultPiece);
    }

    public override void UpdateIndex(int dir)
    {
        base.UpdateIndex(dir);
        setAsDefaultButton.interactable = (Content[index] != null && !Content[index].name.Equals(defaultPiece.name));
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
                    defaultPreview.UpdateValues(defaultPiece);
                }
                else Content[index] = piecePrefab;
                preview.UpdateValues(Content[index] ?? defaultPiece);
                setAsDefaultButton.interactable = (Content[index] != null && !Content[index].name.Equals(defaultPiece.name));
                if (!importedPaths.ContainsKey(path)) importedPaths.Add(path, piecePrefab.name);
            }
            LoadingScreenManager.ToggleLoadingScreen(false);
        }, new string[] { "application/octet-stream" });
    }

    public void SetAsDefault()
    {
        Content[index] = null;
        preview.UpdateValues(defaultPiece);
        setAsDefaultButton.interactable = false;
    }

    public List<GameObject> GetFinalPieces()
    {
        List<GameObject> notNullPieces = new();
        foreach (var p in Content) if (p != null) notNullPieces.Add(p);
        return notNullPieces;
    }

    public override GameObject GetDefaultContent()
    {
        return DefaultPiece;
    }

    public override void DownloadInfo()
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
