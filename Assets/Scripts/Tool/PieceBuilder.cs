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

    protected override void Awake()
    {
        base.Awake();
        if (ToolManager.GameToEdit != null && ToolManager.GameToEdit.numPieces > 0) return;
        for (int i = 0; i < initLength; i++)
        {
            Content.Add(null);
        }
        contentDropdown.SetValueWithoutNotify(initLength - 1);
        preview.UpdateValues(defaultPiece);
        defaultPreview.UpdateValues(defaultPiece);
    }

    public override void SetInitInfo(GameInfo gameInfo)
    {
        if (gameInfo.numPieces == 0) return;
        contentDropdown.SetValueWithoutNotify(gameInfo.numPieces - 1);
        Content = gameInfo.pieces;
        for (int i = 0; i < gameInfo.numPieces; i++) if (i >= Content.Count) Content.Add(null);
        defaultPiece = gameInfo.defaultPiece;
        defaultPreview.UpdateValues(defaultPiece);
        UpdateIndex(0);
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
        ContentLoader.Instance.PickModel((path) =>
        {
            if (path != null)
            {
                Debug.Log("Archivo seleccionado: " + path);
                if (!path.EndsWith(".glb"))
                {
                    FeedbackManager.Instance.DisplayMessage("Invalid file selected. Please choose a .glb file.", Color.white);
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
