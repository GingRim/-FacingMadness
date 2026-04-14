using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum UIType
{
    None, Loading, Title, Movable, Menu, Info, Battle, Reward, Pause, Creation, Quit,
    _Length
   
}

public delegate void PopUpEvent(string title, string context, string confirm);


public class UIManager : ManagerBase
{
    public static event PopUpEvent OnPopUp;

    readonly KeyValuePair<UIType, string>[] globalScreenArray =
    {
        new (UIType.Title, "TitleScreen"),
        new (UIType.Battle, "BattleScreen"),
        new (UIType.Menu, "MenuScreen"),
        new (UIType.Creation, "CharacterCreationScreen"),
        new (UIType.Pause, "PauseWindow"),
        new (UIType.Quit, "QuitConfir"),
    };


    Canvas _mainCanvas;
    public Canvas MainCanvas => _mainCanvas;

    UIBase _movableScreen;
    RectTransform switcherTransform;
    RectTransform createdTransform;
    GraphicRaycaster _raycaster;
    public GraphicRaycaster Raycaster => _raycaster;

    Dictionary<UIType, UIBase>uiDictuionary = new();

    Rect _uiBoundaru;
    public static Rect UIBoundary => GameManager.Instance?.UI?._uiBoundaru ?? Rect.zero;

    UIType _currentScreenType = UIType.None;
    public static UIType CurrentScreen => GameManager.Instance?.UI?._currentScreenType ?? UIType.None;

    float _uiScale = 1.0f;
    public static float UIScale => GameManager.Instance?.UI._uiScale ?? 1.0f;

    public RectTransform CreateFullScreen(string wantName)
    {
        GameObject instance = new GameObject(wantName);
        RectTransform result = instance.AddComponent<RectTransform>();
        //메인 캔버스에 넣고
        result.SetParent(MainCanvas.transform);
        //맨 위로 올려주기!
        result.SetAsFirstSibling();
        //anchor를 stretch - stretch로 만들고 여백을 0,0,0,0
        result.anchorMin = Vector3.zero;
        result.anchorMax = Vector3.one;
        //여백을 0,0,0,0
        result.offsetMin = Vector3.zero;
        result.offsetMax = Vector3.zero;
        // 크기를 1로
        result.localScale = Vector3.one;

        return result;
    }
    public IEnumerator Initialize(GameManager newManager)
    {
        //GameObject.FindGameObjecWithTag("MainCanvas")
        SetMainCanvas(GetComponentInChildren<Canvas>());
        SetUI(UIType.Loading, GetComponentInChildren<UI_LoadingSceen>());
        yield return null;
    }
    protected override IEnumerator OnConnected(GameManager newManager)
    {
        createdTransform = CreateFullScreen("CreatedUI");
        _movableScreen = CreateUI(UIType.Movable, "MovableScreen", MainCanvas?.transform);

        switcherTransform = CreateFullScreen("ScreenSwitcher");

        foreach(var currentPair in globalScreenArray)
        {
            UIBase created = CreateUI(currentPair.Key, currentPair.Value, switcherTransform);
            
            if(created is IOpenable asOpenable) asOpenable.Close();

        }
        RectTransform changerTransform = CreateFullScreen("ScreenChangers");
        changerTransform.SetAsLastSibling();

        GameObject instance = ObjectManager.CreateObject("ScreenChanger", changerTransform);
        if(instance.TryGetComponent(out UI_ScreenChanger asChanger))
        {
            asChanger.ChangeStart();
            yield return new WaitForSeconds(3);
            asChanger.ChangeEnd();
        }
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
            if(MainCanvas.transform is RectTransform mainRectTransForm)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(mainRectTransForm);
                _uiScale = mainRectTransForm.localScale.x;
                _uiBoundaru = mainRectTransForm.rect;
            }
        }
        else
        {
            _raycaster = null;
        }
    }

    protected UIBase CreateUI(UIType wantType, string wantName, Transform parent)
    {
        GameObject instance = ObjectManager.CreateObject(wantName, parent);
        
        UIBase result = instance?.GetComponent<UIBase>();
        
        return SetUI(wantType, result);
    }

    protected UIBase CreateUI(UIType wantType, string wantName)
    {
        UIBase result = CreateUI(wantType, wantName, createdTransform ?? MainCanvas?.transform);

        if(result?.GetComponent<UI_DraggableWindow>())
        {
            _movableScreen?.SetChild(result.gameObject);
        }

        return result;
    }


    public static UIBase ClaimCreateUI(UIType wantType, string wantName) => GameManager.Instance?.UI?.CreateUI(wantType, wantName);
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

    protected UIBase OpenScreen(UIType wantType)
    {
        CloseUI(CurrentScreen);
        _currentScreenType = wantType;
        return OpenUI(wantType);
    }
    public static UIBase OpenScreenM2(UIType wantType) => GameManager.Instance?.UI?.OpenScreen(wantType);


    public static void ClaimPopUp(string title, string context, string conFirm)
    {
        OnPopUp?.Invoke(title, context, conFirm);
    }

    public static void  ClaimErrorMessage(string context)
    {
        OnPopUp?.Invoke("Error", context, "Confirm");
    }

}
