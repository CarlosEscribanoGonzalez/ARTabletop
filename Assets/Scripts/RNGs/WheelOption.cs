using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WheelOption : MonoBehaviour
{
    private Image optionImage; //Componente Image de la opci�n. Interesante para cambiarle el color
    private TextMeshProUGUI optionText; //Texto de la opci�n
    public string Text { get { return optionText.text; } set { optionText.text = value; } }

    public void Initialize()
    {
        optionImage = GetComponentInChildren<Image>(true);
        optionText = GetComponentInChildren<TextMeshProUGUI>(true);
        //Cada opci�n tiene un color aleatorio:
        optionImage.color = new Color(Random.Range(0.5f, 1), Random.Range(0.5f, 1), Random.Range(0.5f, 1)); 
        //En teor�a dos opciones podr�an tener el mismo color, pero es altamente improbable
    }

    public void SetPosition(int numOptions, int index) //Calcula la fracci�n de espacio y posici�n que ocupa cada opci�n
    {
        transform.rotation = Quaternion.identity; //Resetea la rotaci�n de la opci�n
        optionText.transform.localRotation = Quaternion.identity; //Resetea la rotaci�n del texto
        optionImage.fillAmount = 1f / numOptions; //Cada opci�n ocupa una fracci�n del n�mero de opciones que hay en la ruleta
        transform.Rotate(0, 0, 360f/numOptions * index); //Se coloca cada opci�n en su lugar para completar la ruleta
        optionText.transform.Rotate(0, 0, -360f/numOptions/2); //Se ajusta el texto para que est� en el centro visible de la opci�n
    }

    public Vector3 GetResultDirection() //Devuelve un vector que apunta desde el centro de la ruleta hasta el exterior, pasando por el centro de la opci�n
    {
        return -optionText.transform.right; //Como el texto est� en el centro, simplemente se devuelve su right
    }
}
