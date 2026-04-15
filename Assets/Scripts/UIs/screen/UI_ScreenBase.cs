using System;
using UnityEngine;

[Serializable]
public struct UIClaim
{
    public string prefabName;
    public UIType uiType;
    public bool isOpen;

    public UIBase Execute()
    {
        UIBase result = UIManager.GetUIM2(uiType);
        //찾은게 없다.                만들어!
        if (!result) result = UIManager.ClaimCreateUI(uiType, prefabName);
        //만든게 없다.       없어!
        if (!result) return result;

        if(result is IOpenable openTarget)
        {
            if(isOpen) openTarget.Open();
            else openTarget.Close();
        }
            
        return result;
    }
}

public class UI_ScreenBase : UIBase, IOpenable
{
    [SerializeField] UIClaim[] requiredUI;
    public bool IsOpen => gameObject.activeSelf;
    public void Close() => gameObject.SetActive(false);
    public void Open() => gameObject.SetActive(true);
    public void Toggle() => gameObject.SetActive(!IsOpen);
    public override void Registration(UIManager manager)
    {
        base.Registration(manager);

        if(requiredUI is null) return;
        foreach (UIClaim currentClaim in requiredUI)
        {
            currentClaim.Execute();
        }
    }

}
