using System.Collections;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    void OnEnable()
    {
        StartCoroutine(InstantiateObject());
    }

    IEnumerator InstantiateObject()
    {
        yield return new WaitForSeconds(10);
        if (!GameConfigurator.EssentialInfo.isDefault)
        {
            GameInfo fullInfo = GameInfo.GetFullInfo(GameConfigurator.EssentialInfo);
            GameObject.Instantiate(fullInfo.defaultPiece, this.transform);
        }
    }
}
