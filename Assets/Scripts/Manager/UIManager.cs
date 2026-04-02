using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum UIType
{
    None, Loading, Title, Movable,
    _Length,
   
}

public delegate void PopUpEvent(string title, string context, string confirm);


public class UIManager : ManagerBase
{
    public static event PopUpEvent OnPopUp;

    Canvas _mainCanvas;
    public Canvas MainCanvas => _mainCanvas;

    GraphicRaycaster _raycaster;
    public GraphicRaycaster Raycaster => _raycaster;

    Dictionary<UIType, UIBase>uiDictuionary = new();

    public IEnumerator Initialize(GameManager newManager)
    {
        //GameObject.FindGameObjecWithTag("MainCanvas")
        SetMainCanvas(GetComponentInChildren<Canvas>());
        SetUI(UIType.Loading, GetComponentInChildren<UI_LoadingSceen>());
        yield return null;
    }
    protected override IEnumerator OnConnected(GameManager newManager)
    {
        UIBase movableUI = CreateUI(UIType.Movable, "MovableScreen");
        yield return null;
    }

    protected override void OnDisconnected()
    {
        UnSetAllUI(); // 싹 다 나가!
    }

    protected void SetMainCanvas(Canvas newCanvas)
    {
        _mainCanvas = newCanvas;
        if (_mainCanvas)
        {
            _raycaster = _mainCanvas.GetComponentInChildren<GraphicRaycaster>();
        }
        else
        {
            _raycaster = null;
        }
    }

    protected UIBase CreateUI(UIType wantType, string wantName)
    {
        GameObject instance = ObjectManager.CreateObject(wantName, _mainCanvas.transform);
        UIBase result = instance?.GetComponent<UIBase>();
        return SetUI(wantType, result);
    }

    protected UIBase SetUI(UIBase WantUI)
    {
        WantUI?.Registration(this);
        return WantUI;
    }

    protected UIBase SetUI(UIType WantType, UIBase WantUI)
    {
        if(WantUI == null) return null;

        if(uiDictuionary.TryGetValue(WantType, out UIBase origin)) return origin;

        uiDictuionary.Add(WantType, WantUI);
        return SetUI(WantUI);
        
    }
    public static UIBase SetUIM2(UIBase WantUI) => GameManager.Instance?.UI?.SetUI(WantUI);
    public static void SetUIM2(GameObject wantObject) => SetUIM2(wantObject?.GetComponent<UIBase>());
    
    protected void UnsetUI(UIType wantType)// 담당 공무원의 부서의 이름을 알고 있는 경우
    {
        //그 직원을 찾아야 한다.
        //담당 공무원의 이름을 알고 있는 경우로 이동하시오.
        if(uiDictuionary.TryGetValue(wantType, out UIBase found))
        {
            UnsetUI(found);// 처리
            uiDictuionary.Remove(wantType); // 해고
        }
    }
    protected void UnsetUI(UIBase wantUI)// 담당 공무원의 이름을 알고 있는 경우
    {
        if(!wantUI) return;

        wantUI.Unregistration(this);
    }
    public static void UnsetUIM2(UIBase wantUI) => GameManager.Instance?.UI?.UnsetUI(wantUI);
    public static void UnsetUIM2(GameObject wantObject) => UnsetUIM2(wantObject?.GetComponent<UIBase>());

    protected void UnSetAllUI()
    {
        foreach(UIBase ui  in uiDictuionary.Values)
        {
            UnsetUI(ui);// 하나하나 나가라
        }
        //다 나갔으니까 직원 명부 파쇠
        uiDictuionary.Clear();
    }

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

    public static void ClaimPopUp(string title, string context, string conFirm)
    {
        OnPopUp?.Invoke(title, context, conFirm);
    }

    public static void ClaimErrorMessage(string context)
    {
        OnPopUp?.Invoke("Error", context, "Confirm");
    }

}
