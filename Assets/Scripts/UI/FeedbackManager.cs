using UnityEngine;
using TMPro;
using System.Collections;

public class FeedbackManager : MonoBehaviour
{
    [SerializeField] private float fadeOutSpeed = 1;
    [SerializeField] private float stayTime = 5; //Tiempo que se queda enseñando el mensaje
    private TextMeshProUGUI textMesh;
    private CanvasGroup canvasGroup;
    private WaitForSeconds waitTime;
    public static FeedbackManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
        DontDestroyOnLoad(this.gameObject);
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        canvasGroup = GetComponent<CanvasGroup>();
        waitTime = new(stayTime);
    }

    public void DisplayMessage(string text, Color? textColor = null)
    {
        textColor = textColor ?? Color.red; //Color no es const, así que tengo que hacerlo así
        textMesh.text = text;
        textMesh.color = (Color)textColor;
        canvasGroup.alpha = 1;
        StopAllCoroutines();
        StartCoroutine(FadeOutCoroutine());
    }

    IEnumerator FadeOutCoroutine()
    {
        yield return waitTime;
        while(canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= fadeOutSpeed * Time.deltaTime;
            yield return null;
        }
    }
}
