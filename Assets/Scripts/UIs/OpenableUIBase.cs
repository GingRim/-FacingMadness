using UnityEngine;

public class OpenableUIBase : UIBase, IOpenable
{
    public virtual bool IsOpen => gameObject.activeSelf;
    public virtual void Close() => gameObject.SetActive(false);
    public virtual void Open() => gameObject.SetActive(true);
    public virtual void Toggle() => gameObject.SetActive(!IsOpen);
}
