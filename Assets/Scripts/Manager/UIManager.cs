using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UIType
{
    None, Loading, Title, 
    _Length
}

public class UIManager : ManagerBase
{

    Canvas _mainCanvas;

    public Canvas MainCanvas => _mainCanvas;

    Dictionary<UIType, UIBase>uiDictuionary = new();

    protected override IEnumerator OnConnected(GameManager newManager)
    {
        _mainCanvas = GetComponentInChildren<Canvas>();
        //GameObject.FindGameObjecWithTag("MainCanvas")
        SetUI(UIType.Loading, GetComponentInChildren<UI_LoadingSceen>());
        yield return null;
    }

    protected override void OnDisconnected()
    {

    }

    protected UIBase SetUI(UIType WantType, UIBase WantUI)
    {
        if(WantUI == null) return null;

        if(uiDictuionary.TryGetValue(WantType, out UIBase origin)) return origin;

        uiDictuionary.Add(WantType, WantUI);
        return WantUI;
    }
    public static UIBase SetUIM2(UIType WantType, UIBase WantUI) => GameManager.Instance?.UI?.SetUI(WantType, WantUI);
    
    protected UIBase GetUI(UIType wantType)
    {
        if(uiDictuionary.TryGetValue(wantType, out UIBase result))return result;
        else return null;
    }
    public static UIBase GetUIM2(UIType wantType) => GameManager.Instance?.UI?.GetUI(wantType);

    protected UIBase OpenUI(UIType WantType)
    {
        UIBase result = GetUI(WantType);
        if (result is IOpenable opener) opener.Open();

        return result;
    }
    public static UIBase OpenUIM2(UIType wantType) => GameManager.Instance?.UI?.OpenUI(wantType);

    protected UIBase CloseUI(UIType WantType)
    {
        UIBase result = GetUI(WantType);
        if(result is IOpenable opener) opener.Close();
        return result;
    }
    public static UIBase CloseUIM2(UIType wantType) => GameManager.Instance?.UI?.CloseUI(wantType);

    protected UIBase ToggleUI(UIType WantType)
    {
        UIBase result = GetUI(WantType);
        if(result is IOpenable opener) opener.Toggle();
        return result;
    }
    public static UIBase ToggleUIM2(UIType wantType) => GameManager.Instance?.UI?.ToggleUI(wantType);
}
