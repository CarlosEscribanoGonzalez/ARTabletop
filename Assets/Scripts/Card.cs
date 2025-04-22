using UnityEngine;
using TMPro;

public class Card : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;
    [SerializeField] private SpriteRenderer spriteRend;
    private float originalScale;

    private void Awake()
    {
        originalScale = spriteRend.transform.localScale.x;
        AdjustSpriteSize();
    }

    public void SetText(string t)
    {
        text.text = t;
    }

    public void SetSprite(Sprite s)
    {
        spriteRend.sprite = s;
        AdjustSpriteSize();
    }

    Vector2 desiredTextureSize = new Vector2(640, 896);
    float scaleMult = 0;
    private void AdjustSpriteSize()
    {
        if(spriteRend.sprite.texture.width > spriteRend.sprite.texture.height) 
            scaleMult = desiredTextureSize.x / spriteRend.sprite.texture.width;
        else scaleMult = desiredTextureSize.y / spriteRend.sprite.texture.height;
        spriteRend.transform.localScale = new Vector3(originalScale * scaleMult, originalScale * scaleMult, originalScale * scaleMult);
    }
}
