using System;
using System.Collections.Generic;
using UnityEngine;

namespace Serialization
{
    [Serializable]
    public class SpecialCardInfo
    {
        public string name;
        public List<CardInfo> cardsInfo = new();
        public Sprite defaultSpecialSprite;
    }

    [Serializable]
    public class SpecialCardInfoSerializable
    {
        public string name;
        public List<CardInfoSerializable> cardsInfo = new();
        public string defaultSpriteFileName;
    }

    [Serializable]
    public class CardInfoSerializable
    {
        public string spriteFileName;
        public string text;
        public float size;
    }

    [Serializable]
    public class GameInfoSerializable
    {
        public string gameName;
        public string gameImageFileName;

        public bool autoShuffle;
        public bool gameHasDice;
        public bool gameHasWheel;
        public bool gameHasCoins;

        public List<CardInfoSerializable> cardsInfo;
        public string defaultSpriteFileName;

        public int numPieces;
        public string defaultPieceName;
        public List<string> piecesNames;

        public List<string> boardImagesNames;
        public List<string> boardModelsNames;

        public List<SpecialCardInfoSerializable> specialCardsInfo;

    }
}