using UnityEngine;
using Serialization;
using System.Collections.Generic;

public class GameOptionsManager : MonoBehaviour
{
    [SerializeField] private List<GameInfo> games = new();
    [SerializeField] private GameObject gameOptionPrefab;

    void Start()
    {
        foreach (var info in games) 
        {
            GameOption game = Instantiate(gameOptionPrefab, this.transform).GetComponent<GameOption>();
            game.Info = info;
        }
#if !UNITY_EDITOR
        TryReadNewGame(); //Intenta leer la información de un nuevo juego si la app se ha abierto con enlace de compartir juego
#endif
    }

    private void TryReadNewGame()
    {
        Debug.Log("Intentando leer nuevo juego...");
        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject intent = activity.Call<AndroidJavaObject>("getIntent");

            string action = intent.Call<string>("getAction");

            if (action == "android.intent.action.VIEW")
            {
                AndroidJavaObject uri = intent.Call<AndroidJavaObject>("getData");
                string scheme = uri.Call<string>("getScheme");
                string jsonContent = "";

                if (scheme == "file")
                {
                    string path = uri.Call<string>("getPath");
                    jsonContent = System.IO.File.ReadAllText(path);
                }
                else if (scheme == "content")
                {
                    AndroidJavaObject contentResolver = activity.Call<AndroidJavaObject>("getContentResolver");
                    AndroidJavaObject inputStream = contentResolver.Call<AndroidJavaObject>("openInputStream", uri);

                    AndroidJavaObject inputStreamReader = new AndroidJavaObject("java.io.InputStreamReader", inputStream);
                    AndroidJavaObject bufferedReader = new AndroidJavaObject("java.io.BufferedReader", inputStreamReader);

                    string line;
                    while ((line = bufferedReader.Call<string>("readLine")) != null)
                    {
                        jsonContent += line;
                    }

                    bufferedReader.Call("close");
                }

                if (!string.IsNullOrEmpty(jsonContent))
                {
                    AddNewGame(jsonContent);
                }
                else
                {
                    Debug.LogError("El archivo está vacío o no se pudo leer.");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("Intento de añadir juego fallido: " + e.Message);
        }
    }

    private void AddNewGame(string jsonInfo)
    {
        GameInfo newGameInfo = ScriptableObject.CreateInstance<GameInfo>();
        
        //Faltarían todos los archivos, de momento para hacer la prueba sólo se pasan ajustes generales y cartas
        GameInfoSerializable deserialized = JsonUtility.FromJson<GameInfoSerializable>(jsonInfo);
        newGameInfo.gameName = deserialized.gameName;
        newGameInfo.autoShuffle = deserialized.autoShuffle;
        newGameInfo.extendedTracking = deserialized.extendedTracking;
        newGameInfo.gameHasDice = deserialized.gameHasDice;
        newGameInfo.gameHasWheel = deserialized.gameHasWheel;
        newGameInfo.gameHasCoins = deserialized.gameHasCoins;
        newGameInfo.cardsInfo = new List<CardInfo>();
        foreach (var card in deserialized.cardsInfo)
        {
            CardInfo cardInfo = new CardInfo();
            cardInfo.text = card.text;
            cardInfo.sizeMult = card.size;
            newGameInfo.cardsInfo.Add(cardInfo);
        }

        GameOption game = Instantiate(gameOptionPrefab, this.transform).GetComponent<GameOption>();
        game.Info = newGameInfo;
    }
}
