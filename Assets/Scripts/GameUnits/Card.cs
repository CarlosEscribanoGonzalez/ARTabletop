using UnityEngine;
using TMPro;
using UnityEngine.XR.ARFoundation;

public class Card : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;
    [SerializeField] private SpriteRenderer spriteRend;
    [SerializeField] private GameObject buttonCanvas;
    [SerializeField] private Sprite defaultSprite;
    private SpecialCardGameManager specialCardManager;

    private void Start()
    {
        ARTrackedImage trackedImg = GetComponentInParent<ARTrackedImage>();
        if (trackedImg.referenceImage.name.ToLower().Contains("special"))
        {
            specialCardManager = GameSettings.Instance.GetSpecialCardManager(trackedImg.referenceImage.name);
            if (specialCardManager is null || !specialCardManager.ProvideInfo(this))
            {
                GetComponentInParent<PlayableUnit>().DisplayNoInfoIndicator();
            }
            else buttonCanvas.SetActive(true);
        }
        else if (!FindFirstObjectByType<CardGameManager>().ProvideInfo(this))
            GetComponentInParent<PlayableUnit>().DisplayNoInfoIndicator();
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
        spriteRend.sprite = s ?? defaultSprite;
    }

    public void ChangeContent()
    {
        specialCardManager.UpdateCard();
    }

    Vector2 desiredTextureSize = new Vector2(640, 896);
    float scaleMult = 0;
    float prevSizeMult = 0;
    bool scaled = false;
    public void SetSize(float sizeMult, bool resetScale = false)
    {
        if (resetScale)
        {
            scaled = false;
            spriteRend.transform.localScale /= (scaleMult * prevSizeMult);
            GetComponentInChildren<SpriteMask>().transform.localScale /= prevSizeMult;
            text.transform.localScale /= prevSizeMult;
        }
        
        if (scaled) return; //Coger la escala original no funciona porque es un n�mero peque�o y en la build cuenta como 0
        scaled = true;
        prevSizeMult = sizeMult;
        GetComponentInChildren<SpriteMask>().transform.localScale *= sizeMult;
        text.transform.localScale *= sizeMult;
        if(spriteRend.sprite.texture is null)
        {
            spriteRend.transform.localScale *= sizeMult;
            scaleMult = 1;
            return;
        }
        if (spriteRend.sprite.texture.width > spriteRend.sprite.texture.height) 
            scaleMult = desiredTextureSize.x / spriteRend.sprite.texture.width;
        else scaleMult = desiredTextureSize.y / spriteRend.sprite.texture.height;
        spriteRend.transform.localScale *= scaleMult * sizeMult;
    }
}
