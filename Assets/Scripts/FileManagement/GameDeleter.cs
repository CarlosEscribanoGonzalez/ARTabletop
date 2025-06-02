using Serialization;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameDeleter : MonoBehaviour
{
    private static GameOptionsManager gameOptionsManager;

    private void Awake()
    {
        gameOptionsManager = FindFirstObjectByType<GameOptionsManager>();
    }

    public static void DeleteGameFiles(GameInfo gameToDelete) //Borra los datos de un juego dado su SO
    {
        LoadingScreenManager.ToggleLoadingScreen(true); //Feedback al usuario, para que no cierre la app mientras
        DeleteGameInfo(gameToDelete); //Borra su json
        foreach (string textureName in GetAllUsedTextures(gameToDelete)) //Mira cada foto de las presentes en el juego a borrar
        {
            TryDeleteSingleImage(textureName);
        }
        LoadingScreenManager.ToggleLoadingScreen(false);
    }

    public static void TryDeleteSingleImage(string textureName)
    {
        bool contained = false;
        foreach (GameInfo game in gameOptionsManager.CustomGames) //Mira si dicha foto está presente en el resto de juegos o no
        {
            List<string> otherGameTextures = GetAllUsedTextures(game);
            if (otherGameTextures.Contains(textureName)) //Si la foto está presente en otro juego se pasa a la siguiente foto y no se elimina
            {
                contained = true;
                Debug.Log("Imagen " + textureName + " no ha podido ser eliminada porque es usada por " + game.gameName);
                break;
            }
        }
        if (!contained) DeleteImage(textureName); //Las imágenes no usadas en otros juegos son eliminadas
    }

    private static List<string> GetAllUsedTextures(GameInfo game) //Devuelve el nombre de todas las texturas usadas por un juego
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

    private static void AddTextureToList(List<string> textureList, Sprite sprite) //Si la lista no contiene el elemento lo añade
    {
        if (sprite == null || textureList.Contains(sprite.texture.name)) return;
        textureList.Add(sprite.texture.name);
    }

    private static void DeleteGameInfo(GameInfo game) //Borra el json de un juego
    {
        string gameId = game.GetCustomID(); //Obtiene su ID diferenciador (es decir, su path único)
        string path = Path.Combine(Application.persistentDataPath, gameId);
        if (File.Exists(path)) //Si encuentra el path borra el juego
        {
            Debug.Log($"Juego eliminado de {path}");
            File.Delete(path);
        }
        else Debug.LogError($"No se encontró el juego a borrar en {path}");
    }

    private static void DeleteImage(string name) //Dado el nombre de una textura busca su path y la borra
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
