using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum UIType
{
    None, Loading, Title, Movable, Menu, Info, Battle, Reward, Pause, Creation, Quit, SavePopUp, InComplete, MonsturHoverInfo,
    CommentaryHoverInfp, _Length

}

public enum ScreenChangeType
{
    None, ScreenChanger, SlideChanger,
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
        new (UIType.Creation, "CharacterCreationScreen"),
        new (UIType.Menu, "MenuScreen"),
    };


    Canvas _mainCanvas;
    public Canvas MainCanvas => _mainCanvas;

    UIBase _movableScreen;
    RectTransform switcherTransform;
    RectTransform createdTransform;
    RectTransform changerTransform;
    GraphicRaycaster _raycaster;
    public GraphicRaycaster Raycaster => _raycaster;

    Dictionary<UIType, UIBase> uiDictuionary = new();

    Dictionary<ScreenChangeType, UI_ScreenChanger> screenChangerDictionary = new();

    Rect _uiBoundaru;
    public static Rect UIBoundary => GameManager.Instance?.UI?._uiBoundaru ?? Rect.zero;

    UIType _currentScreenType = UIType.None;
    public static UIType CurrentScreen => GameManager.Instance?.UI?._currentScreenType ?? UIType.None;

    UI_ScreenChanger currentScreenChnger;

    float _uiScale = 1.0f;

    public static float UIScale => GameManager.Instance?.UI._uiScale ?? 1.0f;

    public RectTransform CreateFullScreen(string wantName)
    {
        GameObject instance = new GameObject(wantName);
        RectTransform result = instance.AddComponent<RectTransform>();
        //���� ĵ������ �ְ�
        result.SetParent(MainCanvas.transform);
        //�� ���� �÷��ֱ�!
        result.SetAsFirstSibling();
        //anchor�� stretch - stretch�� ����� ������ 0,0,0,0
        result.anchorMin = Vector3.zero;
        result.anchorMax = Vector3.one;
        //������ 0,0,0,0
        result.offsetMin = Vector3.zero;
        result.offsetMax = Vector3.zero;
        // ũ�⸦ 1��
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

        foreach (var currentPair in globalScreenArray)
        {
            UIBase created = CreateUI(currentPair.Key, currentPair.Value, switcherTransform);

            if (created is IOpenable asOpenable) asOpenable.Close();

        }

        changerTransform = CreateFullScreen("ScreenChangers");
        changerTransform.SetAsLastSibling();

        for (ScreenChangeType currentChanger = (ScreenChangeType)1; currentChanger < ScreenChangeType._Length; currentChanger++)
        {   //enmr���� ����
            GameObject instance = ObjectManager.CreateObject(currentChanger.ToString(), changerTransform);
            //���� ��󿡰Լ� ��ũ�� ü���� ����� ��������!
            if (instance?.TryGetComponent(out UI_ScreenChanger asChanger) ?? false)
            {
                //�����͛����� ��ųʸ��� �߰��ϱ�!
                screenChangerDictionary.Add(currentChanger, asChanger);
            }

            //��� ���ô�.
            instance?.SetActive(false);
        }

        yield return null;
    }

    protected override void OnDisconnected()
    {
        UnSetAllUI(); // �� �� ����!
    }

    protected void SetMainCanvas(Canvas newCanvas)
    {
        _mainCanvas = newCanvas;
        if (_mainCanvas)
        {
            _raycaster = _mainCanvas.GetComponentInChildren<GraphicRaycaster>();
            if (MainCanvas.transform is RectTransform mainRectTransForm)
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

        if (result?.GetComponent<UI_DraggableWindow>())
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
        if (WantUI == null) return null;

        if (uiDictuionary.TryGetValue(WantType, out UIBase origin)) return origin;

        uiDictuionary.Add(WantType, WantUI);
        return SetUI(WantUI);

    }
    public static UIBase SetUIM2(UIBase WantUI) => GameManager.Instance?.UI?.SetUI(WantUI);
    public static void SetUIM2(GameObject wantObject) => SetUIM2(wantObject?.GetComponent<UIBase>());

    protected void UnsetUI(UIType wantType)// ��� �������� �μ��� �̸��� �˰� �ִ� ���
    {
        //�� ������ ã�ƾ� �Ѵ�.
        //��� �������� �̸��� �˰� �ִ� ���� �̵��Ͻÿ�.
        if (uiDictuionary.TryGetValue(wantType, out UIBase found))
        {
            UnsetUI(found);// ó��
            uiDictuionary.Remove(wantType); // �ذ�
        }
    }
    protected void UnsetUI(UIBase wantUI)// ��� �������� �̸��� �˰� �ִ� ���
    {
        if (!wantUI) return;

        wantUI.Unregistration(this);
    }
    public static void UnsetUIM2(UIBase wantUI) => GameManager.Instance?.UI?.UnsetUI(wantUI);
    public static void UnsetUIM2(GameObject wantObject) => UnsetUIM2(wantObject?.GetComponent<UIBase>());

    protected void UnSetAllUI()
    {
        foreach (UIBase ui in uiDictuionary.Values)
        {
            UnsetUI(ui);// �ϳ��ϳ� ������
        }
        //�� �������ϱ� ���� ��� �ļ�
        uiDictuionary.Clear();
    }

    protected UIBase GetUI(UIType wantType)
    {
        if (uiDictuionary.TryGetValue(wantType, out UIBase result)) return result;
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
        if (result is IOpenable opener) opener.Close();
        return result;
    }
    public static UIBase CloseUIM2(UIType wantType) => GameManager.Instance?.UI?.CloseUI(wantType);

    protected UIBase ToggleUI(UIType WantType)
    {
        UIBase result = GetUI(WantType);
        if (result is IOpenable opener) opener.Toggle();
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

    protected void OpenScreen(UIType wantScreen, ScreenChangeType changeType)
    {
        ScrrnChangeEffectM2(changeType, () => OpenScreenM2(wantScreen));
    }
    public static void OpenScreenM2(UIType wantScreen, ScreenChangeType changeType) => GameManager.Instance?.UI?.OpenScreen(wantScreen, changeType);
    protected void ScrrnChangeEffectStart(ScreenChangeType wantTyoe, System.Action endFunction = null)
    {
        if (currentScreenChnger) return;

        if(screenChangerDictionary.TryGetValue(wantTyoe, out UI_ScreenChanger result))
        {
            if (!result)
            {
                endFunction?.Invoke();
                return;
            }
            //�Ҵ�
            result.gameObject.SetActive(true);
            //�ִϸ��̼ǵ� �ض�~ �׸��� ������ �̰� ����!
            result?.ChangeStart(endFunction);
            currentScreenChnger = result;
        }
        else
        {
            endFunction?.Invoke();
        }
    } 
    public static void ScrrnChangeEffectStartM2(ScreenChangeType wantTyoe, System.Action endPunction = null) => GameManager.Instance?.UI?.ScrrnChangeEffectStart(wantTyoe, endPunction);
    public static void ScrrnChangeEffectM2(ScreenChangeType wantTyoe, System.Action endPunction = null) => GameManager.Instance?.UI?.ScrrnChangeEffectStart(wantTyoe, endPunction + ScrrnChangeEffectEndM2);
    protected void ScrrnChangeEffectEnd() 
    { 
        if(currentScreenChnger == null) return;
        GameObject targetObject = currentScreenChnger.gameObject;
        currentScreenChnger.ChangeEnd(() => targetObject.SetActive(false));
        currentScreenChnger = null;
    }
    public static void ScrrnChangeEffectEndM2() => GameManager.Instance?.UI?.ScrrnChangeEffectEnd();


    public static void ClaimPopUp(string title, string context, string conFirm)
    {
        OnPopUp?.Invoke(title, context, conFirm);
    }

    public static void  ClaimErrorMessage(string context)
    {
        OnPopUp?.Invoke("Error", context, "Confirm");
    }

}
