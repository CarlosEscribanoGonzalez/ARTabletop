using System.Collections;
using UnityEngine;
using static NativeFilePicker;
using static NativeGallery;

public class ContentLoader : MonoBehaviour
{
    private bool inCooldown = false;
    private WaitForSeconds cooldownTime = new WaitForSeconds(0.3f);
    public static ContentLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
    }

    public void PickImage(MediaPickCallback callback)
    {
        if (inCooldown) return;
        GetImageFromGallery(callback);
        StartCoroutine(CooldownCoroutine());
    }

    public void PickModel(FilePickedCallback callback)
    {
        if (inCooldown) return;
        PickFile(callback, new string[] { "application/octet-stream" });
        StartCoroutine(CooldownCoroutine());
    }

    IEnumerator CooldownCoroutine()
    {
        inCooldown = true;
        yield return cooldownTime;
        inCooldown = false;
    }
}
