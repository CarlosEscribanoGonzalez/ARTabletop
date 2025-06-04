using UnityEngine;
using System.Collections.Generic;
using Serialization;
using System.Linq;
using System.IO;
using Siccity.GLTFUtility;

[CreateAssetMenu(menuName = "ScriptableObjects/GameInfo")]
public class GameInfo : ScriptableObject
{
    [Header("Game info: ")]
    public string gameName;
    public Sprite gameImage;

    [Header("General settings: ")]
    public bool autoShuffle = true;
    public bool gameHasDice = true;
    public bool gameHasWheel = true;
    public bool gameHasCoins = true;

    [Header("Cards: ")]
    public List<CardInfo> cardsInfo = new();
    public Sprite defaultSprite;

    [Header("Pieces: ")]
    public int numPieces;
    public GameObject defaultPiece;
    public List<GameObject> pieces = new();

    [Header("Boards: ")]
    public List<GameObject> boards3D = new();
    public List<Sprite> boards2D = new();

    [Header("SpecialCards")]
    public List<SpecialCardInfo> specialCardsInfo = new();

    public string ConvertToJson() //Convierte el SO a JSON y devuelve su path
    {
        string path = Path.Combine(Application.persistentDataPath, GetOwnJsonID()); //Accede al path a partir del CustomID del juego
        if (File.Exists(path)) return path; //Si ya existe el path lo devuelve
        //Si no existe lo crea:
        var gameInfoSerializable = new GameInfoSerializable //Primero lo convierte a informaci�n serializable
        {
            //General settings:
            gameName = this.gameName,
            gameImageFileName = this.gameImage != null ? this.gameImage.texture.name : null,
            //RNG section:
            autoShuffle = this.autoShuffle,
            gameHasDice = this.gameHasDice,
            gameHasWheel = this.gameHasWheel,
            gameHasCoins = this.gameHasCoins,
            //Cards:
            cardsInfo = this.cardsInfo.Select(card => new CardInfoSerializable
            {
                spriteFileName = card.sprite != null ? card.sprite.texture.name : null,
                text = card.text,
                size = card.sizeMult
            }).ToList(),
            defaultSpriteFileName = this.defaultSprite != null ? this.defaultSprite.texture.name : null,
            //Pieces:
            numPieces = this.numPieces,
            defaultPieceName = this.defaultPiece.name,
            piecesNames = this.pieces.Select(piece => piece.name).ToList(),
            /*/Boards:
            boardImagesNames = this.boards2D.Select(board => board.texture.name).ToList(),
            boardModelsNames = this.boards3D.Select(board => board.name).ToList(),
            //Special cards:
            specialCardsInfo = this.specialCardsInfo.Select(card => new SpecialCardInfoSerializable
            {
                name = card.name,
                cardsInfo = card.cardsInfo.Select(c => new CardInfoSerializable
                {
                    spriteFileName = c.sprite != null ? c.sprite.texture.name : null,
                    text = c.text,
                    size = c.sizeMult
                }).ToList(),
                defaultSpriteFileName = card.defaultSpecialSprite != null ? card.defaultSpecialSprite.texture.name : null
            }).ToList()*/
        };
        File.WriteAllText(path, JsonUtility.ToJson(gameInfoSerializable, true)); //Escribe la informaci�n en el path y lo devuelve
        return path;
    }

    public static GameInfo FromJsonToSO(string json) //Pasa la informaci�n de un JSON a un ScriptableObject
    {
        GameInfo newGameInfo = ScriptableObject.CreateInstance<GameInfo>();

        GameInfoSerializable deserialized = JsonUtility.FromJson<GameInfoSerializable>(json);
        //General settings:
        newGameInfo.gameName = deserialized.gameName;
        newGameInfo.gameImage = AssignSprite(deserialized.gameImageFileName);
        //RNG section:
        newGameInfo.autoShuffle = deserialized.autoShuffle;
        newGameInfo.gameHasDice = deserialized.gameHasDice;
        newGameInfo.gameHasWheel = deserialized.gameHasWheel;
        newGameInfo.gameHasCoins = deserialized.gameHasCoins;
        //Cards:
        newGameInfo.cardsInfo = new List<CardInfo>();
        foreach (var card in deserialized.cardsInfo)
        {
            CardInfo cardInfo = new CardInfo();
            cardInfo.text = card.text;
            cardInfo.sprite = AssignSprite(card.spriteFileName);
            cardInfo.sizeMult = card.size;
            newGameInfo.cardsInfo.Add(cardInfo);
        }
        newGameInfo.defaultSprite = AssignSprite(deserialized.defaultSpriteFileName);
        //Pieces:
        newGameInfo.numPieces = deserialized.numPieces;
        if(!string.IsNullOrEmpty(deserialized.defaultPieceName)) 
            newGameInfo.defaultPiece = AssignModel(deserialized.defaultPieceName);
        foreach (var piece in deserialized.piecesNames) newGameInfo.pieces.Add(AssignModel(piece));
        //Boards:
        foreach (var board2d in deserialized.boardImagesNames) newGameInfo.boards2D.Add(AssignSprite(board2d));
        foreach (var board3d in deserialized.boardModelsNames) newGameInfo.boards3D.Add(AssignModel(board3d));
        //Special cards:
        foreach (var scard in deserialized.specialCardsInfo)
        {
            SpecialCardInfo specialCardInfo = new SpecialCardInfo();
            specialCardInfo.name = scard.name;
            foreach (var card in scard.cardsInfo)
            {
                CardInfo cardInfo = new CardInfo();
                cardInfo.text = card.text;
                cardInfo.sprite = AssignSprite(card.spriteFileName);
                cardInfo.sizeMult = card.size;
                specialCardInfo.cardsInfo.Add(cardInfo);
            }
            specialCardInfo.defaultSpecialSprite = AssignSprite(scard.defaultSpriteFileName);
            newGameInfo.specialCardsInfo.Add(specialCardInfo);
        }
        return newGameInfo;
    }

    public static string GetCustomJsonID(string jsonContent) //Crea un ID para cada juego, altamente improbable que dos juegos tengan el mismo ID
    {
        GameInfoSerializable info = JsonUtility.FromJson<GameInfoSerializable>(jsonContent);
        string imageName = info.gameImageFileName; //Se usa el nombre de su imagen para hacer un hash
        int dif = info.specialCardsInfo.Count * info.cardsInfo.Count + info.boardImagesNames.Count; //N�mero diferenciador en caso de que tengan dos juegos el mismo nombre y la misma imagen
        return info.gameName + "_" + imageName[0].GetHashCode() + imageName[imageName.Length - 1].GetHashCode() + "_" + dif + ".artabletop"; //El custom id del juego se devuelve
    }

    private string GetOwnJsonID()
    {
        string imageName = gameImage.texture.name;
        int dif = specialCardsInfo.Count * cardsInfo.Count + boards2D.Count;
        return gameName + "_" + imageName[0].GetHashCode() + imageName[imageName.Length - 1].GetHashCode() + "_" + dif + ".artabletop";
    }

    static string path; //Path de la imagen que se busca
    static byte[] imgData; //Datos de la imagen
    static Texture2D texture; //Textura de la imagen
    private static Sprite AssignSprite(string textureName) //Devuelve un sprite a partir del nombre de su textura para formar el SO
    {
        if (textureName == string.Empty) return null;
        string[] supportedExtensions = { ".png", ".jpg", ".jpeg" };
        foreach (var ext in supportedExtensions) //Busca con todas las extensiones compatibles
        {
            path = Path.Combine(Application.persistentDataPath, textureName + ext);
            if (File.Exists(path)) //Si existe el path con esa extensi�n:
            {
                imgData = File.ReadAllBytes(path); //Se leen los datos de la imagen
                texture = new Texture2D(0, 0); //Se crea una nueva textura. El tama�o se autoajusta m�s tarde
                texture.name = textureName; //Se le da el mismo nombre a la nueva textura para que tenga el mismo path
                texture.LoadImage(imgData); //Se carga la imagen, se crea el sprite y se devuelve
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }
        Debug.LogError($"La textura {textureName} no fue encontrada en {path}");
        return null;
    }

    private static GameObject AssignModel(string modelName)
    {
        string path = Path.Combine(Application.persistentDataPath, modelName + ".glb");
        if (!File.Exists(path))
        {
            Debug.LogError("Error al asignar modelo: no existe ning�n modelo en el path: " + path);
            return null;
        }
        try
        {
            GameObject modelInstance = Importer.LoadFromFile(path); //Se crea el modelo en la escena
            modelInstance.SetActive(false); //Se desactiva, no se quiere ver ese modelo
            GameObject modelPrefab = GameObject.Instantiate(modelInstance); //Se crea una copia en memoria
            DestroyImmediate(modelInstance); //El original se borra porque no interesa
            modelPrefab.hideFlags = HideFlags.HideAndDontSave; //Se configuran sus hide flags
            modelPrefab.name = modelName; //Se le pone nombre, importante para luego construir el zip
            return modelPrefab;
        }
        catch(System.Exception e)
        {
            Debug.LogError("Error pasando de .glb a GameObject: " + e);
            return null;
        }
    }
}