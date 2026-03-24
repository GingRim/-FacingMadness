using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;
// 대리자는 너에게 내 기술을 전수한다.
// 대리를 뛸 수 있다는 건 => 능력이 아주 좋다. => 가르쳐준 건 모두 한번에 사용한다.
public delegate void MouseDownEvent(Vector3 position);
public delegate void MouseUpEvent(Vector3 position);
public delegate void MouseMoveEvent(Vector2 screenPosition, Vector3 WorldPosition);
[RequireComponent(typeof(PlayerInput))]

public class InputManager : ManagerBase
{
    // 나만 명령할 수 있어야 한다.
    // 기존 대리자는 누구나 등록하고 시전할 수 있다.
    // event 대리자는 누구나 등록하고 나만이 시전할 수 있다.
    public static event MouseDownEvent OnMouseLeftDown;
    public static event MouseUpEvent OnMouseLeftUp;
    public static event MouseDownEvent OnMouseRightDown;
    public static event MouseUpEvent OnMouseRightUp;
    public static event MouseMoveEvent OnMouseMove;


    //특정한 클래스는 특정 컨포넌트와 함께 사용해야 한다.
    //트정 클래스가 다른 클래스를 Dependence 의존하는 경우
    //다른 클래스가 필요해요! Require
    //대상 변수나 클래스 위쪽에다가 [이렇게] 내용을 넣는 것을 Attribute : 속성
    PlayerInput targetInput;
    Dictionary<string, InputAction> actionDictionary = new();

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
        yield return null;
    }

    protected override void OnDisconnected()
    {

    }

    void LoadAllActions()
    {
        foreach (InputAction currentAction in targetInput.actions)
        {
            actionDictionary.TryAdd(currentAction.name, currentAction);

        }
    }

    void InitializeAllActions()
    {
        if(actionDictionary == null || actionDictionary.Count == 0) return;

        if(actionDictionary.TryGetValue("CursorPositionChanged", out InputAction cursorPositionChanged))
        {
            cursorPositionChanged.performed += CursorPositionChanged;
        }

        if(actionDictionary.TryGetValue("MouseLeftButtonDown", out InputAction mouseLeftButtonDown))
        {
            mouseLeftButtonDown.performed += MouseLeftButtonDown;
        }
    }

    void MouseLeftButtonDown(InputAction.CallbackContext context)
    {
        Vector2 screenPosition = context.ReadValue<Vector2>();

        Vector3 worldPosition;
    }

    void CursorPositionChanged(InputAction.CallbackContext context)
    {
        // 마우스의 화면상 실제 픽셀 위치 (좌표값 기본 위치)
        Vector2 screenPosition = context.ReadValue<Vector2>();
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
        OnMouseMove?.Invoke(screenPosition, worldPosition);
    }
}

