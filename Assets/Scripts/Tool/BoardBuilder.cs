using Siccity.GLTFUtility;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class BoardBuilder : ABuilder<GameObject>
{
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button add2DButton;
    [SerializeField] private Button add3DButton;
    [SerializeField] private GameObject noBoardsText;
    private Dictionary<string, string> importedPaths = new();

    public void DeleteBoard()
    {
        Content.RemoveAt(index);
        if (index >= Content.Count) index = Content.Count - 1;
        preview.UpdateValues(Content[index]);
        UpdateUI();
    }

    public void PickImage()
    {
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                Texture2D texture = NativeGallery.LoadImageAtPath(path, 1024, false);
                if (texture == null)
                {
                    Debug.LogError("No se pudo cargar la imagen.");
                    return;
                }
                texture.name = Path.GetFileName(path);
                Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                GameObject newContent = new();
                SpriteRenderer rend = newContent.AddComponent<SpriteRenderer>();
                rend.sprite = newSprite;
                Content.Add(newContent);
                this.index = Content.Count - 1;
                preview.UpdateValues(Content[index]);
                UpdateUI();
            }
        }, "Select an image");
    }

    public void PickModel()
    {
        LoadingScreenManager.ToggleLoadingScreen(true);
        GameObject boardPrefab;
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
                boardPrefab = Instantiate(piece);
                DestroyImmediate(piece);
                boardPrefab.hideFlags = HideFlags.HideAndDontSave;
                boardPrefab.SetActive(false);
                boardPrefab.name = IDCreator.GetCustomModelID(Path.GetFileNameWithoutExtension(path), boardPrefab);
                Content.Add(boardPrefab);
                this.index = Content.Count - 1;
                preview.UpdateValues(Content[index]);
                UpdateUI();
                if (!importedPaths.ContainsKey(path)) importedPaths.Add(path, boardPrefab.name);
            }
            LoadingScreenManager.ToggleLoadingScreen(false);
        }, new string[] { "application/octet-stream" });
    }

    public override GameObject GetDefaultContent()
    {
        return null;
    }

    public override void DownloadInfo()
    {
        SpriteRenderer rend;
        foreach(var board in Content)
        {
            rend = board.GetComponentInChildren<SpriteRenderer>();
            if (rend != null) ContentDownloader.DownloadSprite(rend.sprite);
        }
        foreach (var path in importedPaths.Keys)
        {
            if (ModelIsUsed(importedPaths[path]))
            {
                ContentDownloader.DownloadModel(path, importedPaths[path]);
            }
        }
    }

    private void UpdateUI()
    {
        deleteButton.interactable = Content.Count > 0;
        add3DButton.interactable = Content.Count < 5;
        add2DButton.interactable = Content.Count < 5;
        noBoardsText.SetActive(Content.Count == 0);
        indexText.text = (index + 1).ToString();
        contentDropdown.value = Content.Count;
        CheckArrowsVisibility();
    }

    private bool ModelIsUsed(string modelName)
    {
        foreach (var model in Content)
        {
            if (modelName == model.name) return true;
        }
        return false;
    }
}
