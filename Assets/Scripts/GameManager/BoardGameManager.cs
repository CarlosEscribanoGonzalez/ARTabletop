using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Linq;
using System.Collections.Generic;

public class BoardGameManager : MonoBehaviour, IGameManager
{
    [SerializeField] private XRReferenceImageLibrary imageLibrary; //Librer�a de im�genes AR
    private List<XRReferenceImage> boardImagesList = new(); //Marcadores asociados a los tableros
    private int index = 0;
    //Los tableros pueden ser de tipo modelo o sprite. Los prioritarios son los de tipo tablero
    public GameObject[] BoardModels { get; set; } = null; //Modelos de los tableros
    public Sprite[] BoardSprites { get; set; } = null; //Sprites de los tableros

    private void Awake()
    {
        boardImagesList = imageLibrary.Where(img => img.name.ToLower().Contains("board")).ToList(); //Se almacenan los marcadores de tableros
    }

    ARTrackedImage trackable;
    public bool ProvideInfo(AGameUnit unit) //Se proporciona la informaci�n a los tableros escaneados
    {
        trackable = unit.GetComponentInParent<ARTrackedImage>();
        index = boardImagesList.IndexOf(trackable.referenceImage); //Se calcula el tablero a ense�ar a partir del �ndice de su marcador
        if (index >= 0 && index < BoardModels.Length) //Los primeros marcadores ense�ar�n los tableros de tipo modelo
        {
            unit.SetModel(BoardModels[index]); //Se aplican los modelos
            return true;
        }
        else //Los siguientes marcadores aplicar�n los de tipo sprite
        {
            index -= BoardModels.Length; //Se ajusta el �ndice al array de sprites
            if (index >= 0 && index < BoardSprites.Length)
            {
                unit.SetSprite(BoardSprites[index]); //Se aplican los sprites
                return true;
            }
        }
        return false;
    }
}
