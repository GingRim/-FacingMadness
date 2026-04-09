using UnityEngine;

public class UI_RewardWindow : UIBase, IOpenable
{
    public bool IsOpen => gameObject.activeSelf;
    public void Close() => gameObject.SetActive(false);
    public void Open() => gameObject.SetActive(true);
    public void Toggle() => gameObject.SetActive(!IsOpen);
}
