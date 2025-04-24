using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Card : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;
    [SerializeField] private SpriteRenderer spriteRend;
    [SerializeField] private GameObject buttonCanvas;
    private float originalScale;
    private SpecialCardGameManager specialCardManager;

    private void Awake()
    {
        originalScale = spriteRend.transform.localScale.x;
        AdjustSpriteSize();
    }

    private void Update()
    {
        if (!buttonCanvas.activeSelf) return;
        buttonCanvas.transform.forward = buttonCanvas.transform.position - Camera.main.transform.position;
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

    public void SetSpecial(SpecialCardGameManager manager)
    {
        specialCardManager = manager;
        buttonCanvas.SetActive(true);
    }

    public void ChangeContent()
    {
        specialCardManager.UpdateCard();
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
