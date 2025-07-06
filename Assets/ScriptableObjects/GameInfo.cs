using UnityEngine;
using System.Collections.Generic;
using Serialization;
using System.Linq;
using System.IO;
using Siccity.GLTFUtility;

[CreateAssetMenu(menuName = "ScriptableObjects/GameInfo")]
public class GameInfo : ScriptableObject
{
    [Header("Meta data: ")]
    public string author;
    public string lastEditor;
    [TextArea] public string rules;

    [Header("Game info: ")]
    public string gameName;
    public Sprite gameImage;
    public bool isDefault = false;

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
        string path = Path.Combine(Application.persistentDataPath, IDCreator.GetCustomJsonID(this)); //Accede al path a partir del CustomID del juego
        if (File.Exists(path)) return path; //Si ya existe el path lo devuelve
        //Si no existe lo crea:
        var gameInfoSerializable = new GameInfoSerializable //Primero lo convierte a información serializable
        {
            author = this.author,
            lastEditor = this.lastEditor,
            rules = this.rules,
            //General settings:
            gameName = this.gameName,
            gameImageName = this.gameImage != null ? this.gameImage.texture.name : null,
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
                sizeMult = card.sizeMult
            }).ToList(),
            defaultSpriteFileName = this.defaultSprite != null ? this.defaultSprite.texture.name : null,
            //Pieces:
            numPieces = this.numPieces,
            defaultPieceName = this.defaultPiece.name,
            piecesNames = this.pieces.Select(piece => piece.name).ToList(),
            //Boards:
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
                    sizeMult = c.sizeMult
                }).ToList(),
                defaultImageName = card.defaultImage != null ? card.defaultImage.texture.name : null
            }).ToList()
        };
        File.WriteAllText(path, JsonUtility.ToJson(gameInfoSerializable, true)); //Escribe la información en el path y lo devuelve
        return path;
    }

    public static GameInfo FromJsonToSO(string json, bool provideOnlyEssential = false) //Pasa la información de un JSON a un ScriptableObject
    {
        GameInfo newGameInfo = ScriptableObject.CreateInstance<GameInfo>();

        GameInfoSerializable deserialized = JsonUtility.FromJson<GameInfoSerializable>(json);
        //General settings:
        newGameInfo.author = deserialized.author;
        newGameInfo.lastEditor = deserialized.lastEditor;
        newGameInfo.rules = deserialized.rules;
        newGameInfo.gameName = deserialized.gameName;
        newGameInfo.gameImage = AssignSprite(deserialized.gameImageName);
        newGameInfo.isDefault = false;
        if (provideOnlyEssential) return newGameInfo;
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
            cardInfo.sizeMult = card.sizeMult;
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
                cardInfo.sizeMult = card.sizeMult;
                specialCardInfo.cardsInfo.Add(cardInfo);
            }
            specialCardInfo.defaultImage = AssignSprite(scard.defaultImageName);
            newGameInfo.specialCardsInfo.Add(specialCardInfo);
        }
        return newGameInfo;
    }

    public static GameInfo GetFullInfo(GameInfo essentialInfo) //Para aquellos casos en los que sólo se tiene la información esencial
    {
        string jsonPath = essentialInfo.ConvertToJson(); //Sacamos el path al json, que al ser formado por el nombre y la imagen es accesible únicamente con la info esencial
        return FromJsonToSO(File.ReadAllText(jsonPath)); //Ahora sí obtenemos toda la info del json
    }

    static string path; //Path de la imagen que se busca
    static byte[] imgData; //Datos de la imagen
    static Texture2D texture; //Textura de la imagen
    static Dictionary<string, Sprite> createdSprites = new();
    private static Sprite AssignSprite(string textureName) //Devuelve un sprite a partir del nombre de su textura para formar el SO
    {
        if (textureName == string.Empty) return null;
        path = Path.Combine(Application.persistentDataPath, textureName);
        if (createdSprites.ContainsKey(path)) return createdSprites[path];
        if (File.Exists(path)) //Si existe el path con esa extensión:
        {
            imgData = File.ReadAllBytes(path); //Se leen los datos de la imagen
            texture = new Texture2D(0, 0); //Se crea una nueva textura. El tamaño se autoajusta más tarde
            texture.name = textureName; //Se le da el mismo nombre a la nueva textura para que tenga el mismo path
            texture.LoadImage(imgData); //Se carga la imagen, se crea el sprite y se devuelve
            Sprite s = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            createdSprites.Add(path, s);
            return s;
        }
        Debug.LogError($"La textura {textureName} no fue encontrada en {path}");
        return null;
    }

    static Dictionary<string, GameObject> createdModels = new();
    private static GameObject AssignModel(string modelName)
    {
        if (modelName.Equals("DefaultPice.glb")) return DefaultContentInstaller.Instance.DefaultPiece;
        if (!modelName.EndsWith(".glb")) modelName += ".glb";
        string path = Path.Combine(Application.persistentDataPath, modelName);
        if (createdModels.ContainsKey(path)) return createdModels[path];
        if (!File.Exists(path))
        {
            Debug.LogError("Error al asignar modelo: no existe ningún modelo en el path: " + path);
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
            createdModels.Add(path, modelPrefab);
            return modelPrefab;
        }
        catch(System.Exception e)
        {
            Debug.LogError("Error pasando de .glb a GameObject: " + e);
            return null;
        }
    }
}