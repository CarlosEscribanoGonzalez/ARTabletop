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

    public override void SetInitInfo(GameInfo gameInfo)
    {
        int numBoards = gameInfo.boards2D.Count + gameInfo.boards3D.Count;
        if (numBoards == 0) return;
        foreach (var board2d in gameInfo.boards2D)
        {
            GameObject newContent = new();
            SpriteRenderer rend = newContent.AddComponent<SpriteRenderer>();
            rend.sprite = board2d;
            Content.Add(newContent);
        }
        foreach (var board3d in gameInfo.boards3D)
        {
            Content.Add(board3d);
        }
        UpdateIndex(0);
        UpdateUI();
    }

    public void DeleteBoard()
    {
        Content.RemoveAt(index);
        if (Content.Count > 0)
        {
            if (index >= Content.Count) index = Content.Count - 1;
            preview.UpdateValues(Content[index]);
        }
        else
        {
            index = -1;
            preview.UpdateValues(null);
        }
        UpdateUI();
    }

    public void LoadImage()
    {
        LoadingScreenManager.ToggleLoadingScreen(true, false, "Importing image...");
        ContentLoader.Instance.PickImage((path) =>
        {
            try
            {
                if (path != null)
                {
                    Texture2D texture = NativeGallery.LoadImageAtPath(path, 1024, false);
                    if (texture == null)
                    {
                        FeedbackManager.Instance.DisplayMessage("Unexpected error: image couldn't be loaded. Please, try again.");
                        Debug.LogError("No se pudo cargar la imagen.");
                        return;
                    }
                    if (Path.GetFileName(path).EndsWith(".png") || Path.GetFileName(path).EndsWith(".jpg"))
                        texture.name = Path.GetFileName(path);
                    else texture.name = Path.GetFileNameWithoutExtension(path) + ".jpg";
                    Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    GameObject newContent = new();
                    SpriteRenderer rend = newContent.AddComponent<SpriteRenderer>();
                    rend.sprite = newSprite;
                    Content.Add(newContent);
                    this.index = Content.Count - 1;
                    preview.UpdateValues(Content[index]);
                    UpdateUI();
                }
                LoadingScreenManager.ToggleLoadingScreen(false);
            }
            catch
            {
                LoadingScreenManager.ToggleLoadingScreen(false);
            }
        });
    }

    public void LoadModel()
    {
        LoadingScreenManager.ToggleLoadingScreen(true, false, "Importing model...");
        GameObject boardPrefab;
        ContentLoader.Instance.PickModel((path) =>
        {
            try
            {
                if (path != null)
                {
                    Debug.Log("Archivo seleccionado: " + path);
                    if (!path.EndsWith(".glb"))
                    {
                        FeedbackManager.Instance.DisplayMessage("Invalid file selected. Please choose a file with .glb extension.", Color.white);
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
            }
            catch
            {
                LoadingScreenManager.ToggleLoadingScreen(false);
            }
        });
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
