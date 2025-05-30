using System;
using UnityEngine;

[Serializable]
public class CardInfo
{
    public Sprite sprite;
    [TextArea] public string text;
    public float sizeMult = 1;
}
