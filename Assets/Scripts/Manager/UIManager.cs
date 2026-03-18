using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UIType
{
    None, LOading, Title, 
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
        Debug.Log(MainCanvas);
        yield return null;
    }

    protected override void OnDisconnected()
    {

    }

    public UIBase SetUI(UIType WantType, UIBase WantUI)
    {
        if(WantUI == null) return null;

        if(uiDictuionary.TryGetValue(WantType, out UIBase origin)) return origin;

        uiDictuionary.Add(WantType, WantUI);
        return WantUI;
    }

    public UIBase GetUI(UIType wantType)
    {
        if(uiDictuionary.TryGetValue(wantType, out UIBase result))return result;
        else return null;
    }

    public UIBase OpenUI(UIType WantType)
    {
        UIBase result = GetUI(WantType);
        return result;
    }

    public UIBase CloseUI(UIType WantType)
    {
        UIBase result = GetUI(WantType);
        if(result is IOpenable opener) opener.Close();
        return result;
    }

    public UIBase ToggleUI(UIType WantType)
    {
        UIBase result = GetUI(WantType);
        if(result is IOpenable opener) opener.Toggle();
        return result;
    }
}
