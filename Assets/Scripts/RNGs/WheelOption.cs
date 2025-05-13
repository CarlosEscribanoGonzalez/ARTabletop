using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WheelOption : MonoBehaviour
{
    private Image optionImage; //Componente Image de la opción. Interesante para cambiarle el color
    private TextMeshProUGUI optionText; //Texto de la opción
    public string Text { get { return optionText.text; } set { optionText.text = value; } }

    public void Initialize()
    {
        optionImage = GetComponentInChildren<Image>(true);
        optionText = GetComponentInChildren<TextMeshProUGUI>(true);
        //Cada opción tiene un color aleatorio:
        optionImage.color = new Color(Random.Range(0.5f, 1), Random.Range(0.5f, 1), Random.Range(0.5f, 1)); 
        //En teoría dos opciones podrían tener el mismo color, pero es altamente improbable
    }

    public void SetPosition(int numOptions, int index) //Calcula la fracción de espacio y posición que ocupa cada opción
    {
        transform.rotation = Quaternion.identity; //Resetea la rotación de la opción
        optionText.transform.localRotation = Quaternion.identity; //Resetea la rotación del texto
        optionImage.fillAmount = 1f / numOptions; //Cada opción ocupa una fracción del número de opciones que hay en la ruleta
        transform.Rotate(0, 0, 360f/numOptions * index); //Se coloca cada opción en su lugar para completar la ruleta
        optionText.transform.Rotate(0, 0, -360f/numOptions/2); //Se ajusta el texto para que esté en el centro visible de la opción
    }

    public Vector3 GetResultDirection() //Devuelve un vector que apunta desde el centro de la ruleta hasta el exterior, pasando por el centro de la opción
    {
        return -optionText.transform.right; //Como el texto está en el centro, simplemente se devuelve su right
    }
}
