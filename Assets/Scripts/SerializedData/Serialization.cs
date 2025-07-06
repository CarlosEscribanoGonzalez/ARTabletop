using System;
using System.Collections.Generic;
using UnityEngine;

namespace Serialization
{
    [Serializable]
    public class SpecialCardInfoSerializable
    {
        public string name;
        public List<CardInfoSerializable> cardsInfo = new();
        public string defaultImageName;
    }

    [Serializable]
    public class CardInfoSerializable
    {
        public string spriteFileName;
        public string text;
        public float sizeMult;
    }

    [Serializable]
    public class GameInfoSerializable
    {
        public string author;
        public string lastEditor;
        public string rules;

        public string gameName;
        public string gameImageName;

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