using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Serialization;

public class GameDeleter : MonoBehaviour
{
    public static void DeleteGameFiles(GameInfo gameToDelete) //Borra los datos de un juego dado su SO
    {
        string json = File.ReadAllText(gameToDelete.ConvertToJson());
        GameInfoSerializable serializedGameInfo = JsonUtility.FromJson<GameInfoSerializable>(json);
        DeleteGameInfo(File.ReadAllText(gameToDelete.ConvertToJson())); //Borra su json
        foreach (string textureName in GetAllUsedTextures(serializedGameInfo)) //Mira cada foto de las presentes en el juego a borrar
        {
            TryDeleteSingleImage(textureName);
        }
        foreach(string modelName in GetAllUsedModels(serializedGameInfo))
        {
            TryDeleteSingleModel(modelName);
        }
        LoadingScreenManager.ToggleLoadingScreen(false);
    }

    public static void TryDeleteSingleImage(string textureName)
    {
        if (textureName.Contains("DefaultImage")) return;
        bool contained = false;
        foreach (GameInfo game in GameOptionsManager.CustomGames) //Mira si dicha foto está presente en el resto de juegos o no
        {
            string json = File.ReadAllText(game.ConvertToJson());
            GameInfoSerializable serializedGameInfo = JsonUtility.FromJson<GameInfoSerializable>(json); 
            List<string> otherGameTextures = GetAllUsedTextures(serializedGameInfo);
            if (otherGameTextures.Contains(textureName)) //Si la foto está presente en otro juego se pasa a la siguiente foto y no se elimina
            {
                contained = true;
                Debug.Log("Imagen " + textureName + " no ha podido ser eliminada porque es usada por " + game.gameName);
                break;
            }
        }
        if (!contained) DeleteImage(textureName); //Las imágenes no usadas en otros juegos son eliminadas
    }

    public static void TryDeleteSingleModel(string modelName)
    {
        if (modelName.Contains("DefaultPiece")) return;
        bool contained = false;
        foreach (GameInfo game in GameOptionsManager.CustomGames) 
        {
            string json = File.ReadAllText(game.ConvertToJson());
            GameInfoSerializable serializedGameInfo = JsonUtility.FromJson<GameInfoSerializable>(json); 
            List<string> otherGameModels = GetAllUsedModels(serializedGameInfo);
            if (otherGameModels.Contains(modelName)) 
            {
                contained = true;
                Debug.Log("Modelo " + modelName + " no ha podido ser eliminado porque es usado por " + game.gameName);
                break;
            }
        }
        if (!contained) DeleteModel(modelName);
    }

    private static List<string> GetAllUsedTextures(GameInfoSerializable game) //Devuelve el nombre de todas las texturas usadas por un juego
    {
        List<string> textureNameList = new();
        AddPathToList(textureNameList, game.gameImageFileName);
        foreach (var c in game.cardsInfo) AddPathToList(textureNameList, c.spriteFileName);
        AddPathToList(textureNameList, game.defaultSpriteFileName);
        foreach (var b in game.boardImagesNames) AddPathToList(textureNameList, b);
        foreach (var sc in game.specialCardsInfo)
        {
            AddPathToList(textureNameList, sc.defaultSpriteFileName);
            foreach (var c in sc.cardsInfo) AddPathToList(textureNameList, c.spriteFileName);
        }
        return textureNameList;
    }

    private static List<string> GetAllUsedModels(GameInfoSerializable game)
    {
        List<string> modelNames = new();
        AddPathToList(modelNames, game.defaultPieceName);
        foreach (var piece in game.piecesNames) AddPathToList(modelNames, piece);
        foreach (var board in game.boardModelsNames) AddPathToList(modelNames, board);
        return modelNames;
    }

    private static void AddPathToList(List<string> list, string path)
    {
        if (string.IsNullOrEmpty(path)) return;
        if (!list.Contains(path)) list.Add(path);
    }

    private static void DeleteGameInfo(string jsonContent) //Borra el json de un juego
    {
        string gameId = IDCreator.GetCustomJsonID(jsonContent); //Obtiene su ID diferenciador (es decir, su path único)
        string path = Path.Combine(Application.persistentDataPath, gameId);
        if (File.Exists(path)) //Si encuentra el path borra el juego
        {
            Debug.Log($"Juego eliminado de {path}");
            File.Delete(path);
        }
        else
        {
            FeedbackManager.Instance.DisplayMessage("Unexpected error: game to delete couldn't be found.");
            Debug.LogError($"No se encontró el juego a borrar en {path}");
        }
    }

    private static void DeleteImage(string name) //Dado el nombre de una textura busca su path y la borra
    {
        string path = Path.Combine(Application.persistentDataPath, name);
        if (!File.Exists(path)) Debug.LogError($"Imagen a borrar en {path} no encontrada.");
        else
        {
            Debug.Log("Imagen en " + path + " eliminada");
            File.Delete(path);
        }
    }

    private static void DeleteModel(string name)
    {
        string path = Path.Combine(Application.persistentDataPath, name);
        if (!File.Exists(path)) Debug.LogError($"Modelo a borrar en {path} no encontrado.");
        else
        {
            Debug.Log("Modelo en " + path + " eliminado");
            File.Delete(path);
        }
    }
}
