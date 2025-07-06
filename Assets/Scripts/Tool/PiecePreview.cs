using UnityEngine;

public class PiecePreview : APreview<GameObject>
{
    [SerializeField] private Transform previewTransform;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float verticalSpeed;
    [SerializeField] private float verticalOffset;
    [SerializeField] private float targetSize = 6.75f;
    private GameObject currentPiece;
    private float targetY;
    private float startY;

    void Start()
    {
        startY = previewTransform.localPosition.y;
        targetY = startY + verticalOffset;
    }

    void FixedUpdate()
    {
        previewTransform.Rotate(0, rotationSpeed * Time.fixedDeltaTime, 0);
        float newY = Mathf.MoveTowards(previewTransform.localPosition.y, targetY, verticalSpeed * Time.fixedDeltaTime);
        previewTransform.localPosition = new Vector3(previewTransform.localPosition.x, newY, previewTransform.localPosition.z);
        if (Mathf.Abs(previewTransform.localPosition.y - targetY) < 0.05f)
        {
            verticalOffset *= -1;
            targetY = startY + verticalOffset;
        }
    }

    private GameObject piecePrefab;
    public override void UpdateValues(GameObject piecePrefab)
    {
        Destroy(currentPiece);
        previewTransform.localScale = Vector3.one;
        currentPiece = GameObject.Instantiate(piecePrefab, previewTransform);
        currentPiece.SetActive(true);
        this.piecePrefab = piecePrefab;
        AdjustSize();
    }

    protected override void AdjustSize()
    {
        float scaleFactor = ContentScaler.ScaleModel(piecePrefab, targetSize);
        previewTransform.localScale = Vector3.one * scaleFactor;
    }
}
