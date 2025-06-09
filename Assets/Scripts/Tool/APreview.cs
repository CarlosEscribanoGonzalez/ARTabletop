using UnityEngine;

public abstract class APreview<T> : MonoBehaviour
{
    public abstract void UpdateValues(T contentToShow);
}
