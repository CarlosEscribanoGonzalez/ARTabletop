using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpecialCardInfo
{
    public string name;
    public List<CardInfo> cardsInfo = new();
    public Sprite defaultImage;
}