using UnityEngine;

public class UI_LoadingSceen : UIBase, IOpenable
{
    public bool IsOpen => gameObject.activeSelf;

    public void Close() => gameObject.SetActive(false);
    
    public void Open() => gameObject.SetActive(true);
    
    public void toggle() => gameObject.SetActive(!IsOpen);
}
