using UnityEngine;
using TMPro;

public class Card : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;
    [SerializeField] private SpriteRenderer spriteRend;

    public void SetText(string t)
    {
        text.text = t;
    }

    public void SetSprite(Sprite s)
    {
        spriteRend.sprite = s;
    }
}
