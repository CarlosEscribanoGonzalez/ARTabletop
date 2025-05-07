using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WheelOption : MonoBehaviour
{
    private Image optionImage;
    private TextMeshProUGUI optionText;
    public string Text { get { return optionText.text; } set { optionText.text = value; } }

    public void Initialize()
    {
        optionImage = GetComponentInChildren<Image>(true);
        optionText = GetComponentInChildren<TextMeshProUGUI>(true);
        optionImage.color = new Color(Random.Range(0.5f, 1), Random.Range(0.5f, 1), Random.Range(0.5f, 1));
    }

    public void SetPosition(int numOptions, int index)
    {
        transform.rotation = Quaternion.identity;
        optionText.transform.localRotation = Quaternion.identity;
        optionImage.fillAmount = 1f / numOptions;
        transform.Rotate(0, 0, 360f/numOptions * index);
        optionText.transform.Rotate(0, 0, -360f/numOptions/2);
    }

    public Vector3 GetResultDirection()
    {
        return -optionText.transform.right;
    }
}
