using Serialization;
using UnityEngine;

public static class IDCreator 
{
    //Las im�genes tienen su propio ID desde la galer�a, es casi imposible que dos tengan el mismo,
    //as� que no hace falta hacerles una funci�n
    public static string GetCustomJsonID(string jsonContent) //Crea un ID para cada juego, altamente improbable que dos juegos tengan el mismo ID
    {
        GameInfoSerializable info = JsonUtility.FromJson<GameInfoSerializable>(jsonContent);
        string imageName = info.gameImageFileName; //Se usa el nombre de su imagen para hacer un hash
        int dif = info.specialCardsInfo.Count + info.cardsInfo.Count + info.boardImagesNames.Count; //N�mero diferenciador en caso de que tengan dos juegos el mismo nombre y la misma imagen
        return info.gameName + "_" + imageName[0].GetHashCode() + 
            imageName[imageName.Length - 1].GetHashCode() + "_" + dif + ".artabletop"; //El custom id del juego se devuelve
    }

    public static string GetCustomJsonID(GameInfo game)
    {
        string imageName = game.gameImage.texture.name;
        int dif = game.specialCardsInfo.Count + game.cardsInfo.Count + game.boards2D.Count;
        return game.gameName + "_" + imageName[0].GetHashCode() + 
            imageName[imageName.Length - 1].GetHashCode() + "_" + dif + ".artabletop";
    }

    public static string GetCustomModelID(string modelName, GameObject model)
    {
        Vector3 modelSizes = model.GetComponentInChildren<MeshFilter>().mesh.bounds.size;
        string fileName = $"{modelName}_{(int)modelSizes.x % 10}{(int)modelSizes.y % 10}{(int)modelSizes.z % 10}.glb"; //ID personalizado para modelos
        return fileName;
    }
}
