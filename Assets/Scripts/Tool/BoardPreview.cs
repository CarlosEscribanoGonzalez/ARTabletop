using UnityEngine;

public class BoardPreview : APreview<GameObject>
{
    [SerializeField] private SpriteRenderer spriteRend;
    [SerializeField] private Transform previewTransform;
    [SerializeField] private float targetSize = 10;
    [SerializeField] private float targetTextureSize = 36.5f;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float verticalSpeed;
    [SerializeField] private float verticalOffset;
    private GameObject previewedObject;
    private float targetY;

    void Start()
    {
        targetY = previewTransform.localPosition.y + verticalOffset;
        spriteRend.gameObject.SetActive(false);
    }

    void FixedUpdate()
    {
        previewTransform.Rotate(0, rotationSpeed * Time.fixedDeltaTime, 0);
        float newY = Mathf.MoveTowards(previewTransform.localPosition.y, targetY, verticalSpeed * Time.fixedDeltaTime);
        previewTransform.localPosition = new Vector3(previewTransform.localPosition.x, newY, previewTransform.localPosition.z);
        if (Mathf.Abs(previewTransform.localPosition.y - targetY) < 0.05f) targetY *= -1;
    }

    public override void UpdateValues(GameObject contentToShow)
    {
        if(previewedObject != null) Destroy(previewedObject);
        SpriteRenderer rend = contentToShow.GetComponentInChildren<SpriteRenderer>();
        spriteRend.gameObject.SetActive(rend != null);
        if (rend == null)
        {
            previewedObject = Instantiate(contentToShow, previewTransform);
            previewedObject.SetActive(true);
            float scaleFactor = ContentScaler.ScaleModel(contentToShow, targetSize);
            previewTransform.localScale = Vector3.one * scaleFactor;
        }
        else
        {
            spriteRend.sprite = rend.sprite;
            float scaleFactor = ContentScaler.ScaleImage(spriteRend.sprite.texture, new Vector2(targetTextureSize, targetTextureSize));
            previewTransform.localScale = Vector3.one * scaleFactor;
        }
    }
}
