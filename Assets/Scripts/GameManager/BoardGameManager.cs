using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Linq;
using System.Collections.Generic;

public class BoardGameManager : MonoBehaviour, IGameManager
{
    //Los tableros pueden ser de tipo modelo o sprite. Los prioritarios son los de tipo tablero
    [SerializeField] private GameObject[] boardModels; //Modelos de los tableros
    [SerializeField] private Sprite[] boardSprites; //Sprites de los tableros
    [SerializeField] private XRReferenceImageLibrary imageLibrary;
    private List<XRReferenceImage> boardImagesList = new(); //Marcadores asociados a los tableros
    private int index = 0;

    private void Awake()
    {
        boardImagesList = imageLibrary.Where(img => img.name.ToLower().Contains("board")).ToList(); //Se almacenan los marcadores de tableros
    }

    ARTrackedImage trackable;
    public bool ProvideInfo(AGameUnit unit) //Se proporciona la información a los tableros escaneados
    {
        trackable = unit.GetComponentInParent<ARTrackedImage>();
        index = boardImagesList.IndexOf(trackable.referenceImage); //Se calcula el tablero a enseñar a partir del índice de su marcador
        if (index >= 0 && index < boardModels.Length) //Los primeros marcadores enseñarán los tableros de tipo modelo
        {
            unit.SetModel(boardModels[index]); //Se aplican los modelos
            return true;
        }
        else //Los siguientes marcadores aplicarán los de tipo sprite
        {
            index -= boardModels.Length; //Se ajusta el índice al array de sprites
            if (index >= 0 && index < boardSprites.Length)
            {
                unit.SetSprite(boardSprites[index]); //Se aplican los sprites
                return true;
            }
        }
        return false;
    }
}
