using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
// 대리자는 너에게 내 기술을 전수한다.
// 대리를 뛸 수 있다는 건 => 능력이 아주 좋다. => 가르쳐준 건 모두 한번에 사용한다.
public delegate void MouseDownEvent(Vector2 screenPosition, Vector3 WorldPosition);
public delegate void MouseUpEvent(Vector2 screenPosition, Vector3 WorldPosition);
public delegate void MouseMoveEvent(Vector2 screenPosition, Vector3 WorldPosition);
public delegate void MouseHold(Vector2 screenPosition, Vector3 WorldPosition);
public delegate void Esc(bool value);
public delegate void Sikc(bool value);



[RequireComponent(typeof(PlayerInput))]

public class InputManager : ManagerBase
{
    // 나만 명령할 수 있어야 한다.
    // 기존 대리자는 누구나 등록하고 시전할 수 있다.
    // event 대리자는 누구나 등록하고 나만이 시전할 수 있다.
    public static event MouseDownEvent OnMouseLeftDown;
    public static event MouseDownEvent OnMouseRightDown;
    public static event MouseUpEvent OnMouseLeftUp;
    public static event MouseUpEvent OnMouseRightUp;
    public static event MouseMoveEvent OnMouseMove;
    public static event MouseHold OnHold;
    public static event Esc OnEsc;
    public static event Sikc OnSpace;


    //특정한 클래스는 특정 컨포넌트와 함께 사용해야 한다.
    //트정 클래스가 다른 클래스를 Dependence 의존하는 경우
    //다른 클래스가 필요해요! Require
    //대상 변수나 클래스 위쪽에다가 [이렇게] 내용을 넣는 것을 Attribute : 속성
    PlayerInput targetInput;
    Dictionary<string, InputAction> actionDictionary = new();
    List<RaycastResult> cursorHitList = new();

    Vector2 cursorScreenPosition;
    Vector3 cursorWorldPosition;

    public bool is2D = true;

    protected override IEnumerator OnConnected(GameManager newManager)
    {

        

        targetInput = GetComponent<PlayerInput>();
        // 전의 방식은 유저가 키 변경이 불가능 하다. (소울 원더러)
        // Forward가 뭔지 알아야 한다. => Forward의 버튼을 알 수 있음
        // On~~을 사용하지 않는 것은 하는 이름의 함수를 스크립트에서 찾아서 실시간으로 실행할 수 있는 기능을 불러와야 한다.
        // 이 방식은 유니티가 아니라 내가 직접 꽂아줄 것이다.
        LoadAllActions();
        InitializeAllActions();

        GameManager.OnUpdateEventManager -= UpdateEvent;
        GameManager.OnUpdateEventManager += UpdateEvent;
        yield return null;
    }

    protected override void OnDisconnected()
    {
        GameManager.OnUpdateEventManager -= UpdateEvent;
    }

    public void UpdateEvent(float deltaTime)// 마우스가 올라가면 계속 업데이트 된다.
    {
        RefreshGameObjectUnderCursor();
    }

    void RefreshGameObjectUnderCursor()
    {
        cursorHitList.Clear();
        if (is2D)
        {
            GameManager.Instance.Camera.GetRaycastResult2D(cursorScreenPosition, cursorHitList);
        }
        else
        {
            GameManager.Instance.Camera.GetRaycastResult3D(cursorScreenPosition, cursorHitList);
        }

    }

    public GameObject GetGameObjectUnderCursor()
    {
        if(cursorHitList.Count == 0) return null;

        return cursorHitList[0].gameObject;
    }

    void LoadAllActions()
    {
        foreach (InputAction currentAction in targetInput.actions)
        {
            actionDictionary.TryAdd(currentAction.name, currentAction);

        }
    }

    void InitializeAllActions() // 이니셜 라이즈 올 액션 (모든 액션을 만들기 위한 하나의 함수)
    {
        if(actionDictionary == null || actionDictionary.Count == 0) return;

        InitializeAction("CursorPositionChanged", (context) => CursorPositionChanged(GetVector2Value(context)));
        InitializeAction("MouseLeftButtonDown",  (context) => OnMouseLeftDown?.Invoke(cursorScreenPosition, cursorWorldPosition)); //람다를 이용한 이름 없는 함수
        InitializeAction("MouseLeftButtonUP",    (context) => OnMouseLeftUp?.Invoke(cursorScreenPosition, cursorWorldPosition));
        InitializeAction("MouseRightButtonDown", (context) => OnMouseRightDown?.Invoke(cursorScreenPosition, cursorWorldPosition));
        InitializeAction("MouseRightButtonUP",   (context) => OnMouseRightUp?.Invoke(cursorScreenPosition, cursorWorldPosition));
        InitializeAction("MouseHold", (context) => OnHold?.Invoke(cursorScreenPosition, cursorWorldPosition));
        InitializeAction("Esc", (context) => OnEsc?.Invoke(true));
        InitializeAction("Sikc", (context) => OnSpace?.Invoke(true));
    }
      
    void InitializeAction(string actionName, Action<InputAction.CallbackContext> actionMethod) // 이니셜 라이즈 액션 (각 액션을 만들기 위한 하나의 함수)
    {
        if (actionDictionary == null || actionDictionary.Count == 0) return;

        if (actionDictionary.TryGetValue(actionName, out InputAction cursorPositionChanged))
        {
            cursorPositionChanged.performed += actionMethod;
        }
    }

    Vector2 GetVector2Value(InputAction.CallbackContext context)
    {
        if(context.valueType != typeof(Vector2)) return Vector2.zero;
        return context.ReadValue<Vector2>();
    }
    void CursorPositionChanged(Vector2 screenPosition) // 커서 포지션 채인지드 실시간 마우스 위치를 카메라 기준 감지 
    {



        // 마우스의 화면상 실제 픽셀 위치 (좌표값 기본 위치)
        // 카메라를 기준으로 세상을 본다.
        Vector3 worldPosition;

        if (is2D)
        {
            worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
            worldPosition.z = 0;
        }
        else
        {
            worldPosition = Vector3.zero;
        }
        cursorScreenPosition = screenPosition;
        cursorWorldPosition = worldPosition;

        OnMouseMove?.Invoke(screenPosition, worldPosition);
    }

    void MouseButtonAction(InputAction.CallbackContext context)
    {

    }
 
}

