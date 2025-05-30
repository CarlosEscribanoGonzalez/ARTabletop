using Serialization;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameDeleter : MonoBehaviour
{
    public static void DeleteGame(GameInfo gameToDelete)
    {
        LoadingScreenManager.ToggleLoadingScreen(true);
        DeleteGameInfo(gameToDelete);
        GameOptionsManager gameOptionsManager = FindFirstObjectByType<GameOptionsManager>();
        gameOptionsManager.RemoveGame(gameToDelete);
        foreach (string textureName in GetAllUsedTextures(gameToDelete))
        {
            bool contained = false;
            foreach (GameInfo game in gameOptionsManager.Games)
            {
                List<string> otherGamesTextures = GetAllUsedTextures(gameToDelete);
                if (otherGamesTextures.Contains(textureName))
                {
                    contained = true;
                    Debug.Log("Imagen " + textureName + " no ha podido ser eliminada porque es usada por " + game.gameName);
                    break;
                }
            }
            if (!contained) DeleteImage(textureName);
        }
        LoadingScreenManager.ToggleLoadingScreen(false);
    }

    private static List<string> GetAllUsedTextures(GameInfo game)
    {
        List<string> textureNameList = new();
        AddTextureToList(textureNameList, game.gameImage);
        foreach (var c in game.cardsInfo) AddTextureToList(textureNameList, c.sprite);
        AddTextureToList(textureNameList, game.defaultSprite);
        foreach (var b in game.boards2D) AddTextureToList(textureNameList, b);
        foreach (var sc in game.specialCardsInfo)
        {
            AddTextureToList(textureNameList, sc.defaultSpecialSprite);
            foreach (var c in sc.cardsInfo) AddTextureToList(textureNameList, c.sprite);
        }
        return textureNameList;
    }

    private static void AddTextureToList(List<string> textureList, Sprite sprite)
    {
        if (sprite == null || textureList.Contains(sprite.texture.name)) return;
        textureList.Add(sprite.texture.name);
    }

    private static void DeleteGameInfo(GameInfo game)
    {
        string gameId = game.GetCustomID();
        string path = Path.Combine(Application.persistentDataPath, gameId);
        if (File.Exists(path))
        {
            Debug.Log($"Juego eliminado de {path}");
            File.Delete(path);
        }
        else Debug.LogError($"No se encontró el juego a borrar en {path}");
    }

    private static void DeleteImage(string name)
    {
        string path = Path.Combine(Application.persistentDataPath, name + ".png");
        if (!File.Exists(path)) path = Path.Combine(Application.persistentDataPath, name + ".jpg");
        if (!File.Exists(path)) Debug.LogError($"Imagen a borrar en {path} no encontrada.");
        else
        {
            Debug.Log("Imagen en " + path + " eliminada");
            File.Delete(path);
        }
    }
}
