using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
// �븮�ڴ� �ʿ��� �� ����� �����Ѵ�.
// �븮�� �� �� �ִٴ� �� => �ɷ��� ���� ����. => �������� �� ��� �ѹ��� ����Ѵ�.
public delegate void MouseButtonEvent(bool value, Vector2 screenPosition, Vector3 WorldPosition);
public delegate void MouseMoveEvent(Vector2 screenPosition, Vector3 WorldPosition);
public delegate void MouseHold(Vector2 screenPosition, Vector3 WorldPosition);
public delegate void ButtonEvent(bool value);
public delegate void VectorEvent(Vector2 value);
public delegate void AxisEvent(float value);



[RequireComponent(typeof(PlayerInput))]

public class InputManager : ManagerBase
{
    // ���� ����� �� �־�� �Ѵ�.
    // ���� �븮�ڴ� ������ ����ϰ� ������ �� �ִ�.
    // event �븮�ڴ� ������ ����ϰ� ������ ������ �� �ִ�.
    public static event MouseButtonEvent OnMouseLeftButton; // ���� Ŭ��
    public static event MouseButtonEvent OnMouseRightButton;// ������ Ŭ��
    public static event MouseMoveEvent   OnMouseMove;       // ���콺 �̵�
    public static event ButtonEvent      OnCancel;          // 
    public static event ButtonEvent      OnShowStatus;      //
    public static event VectorEvent      OnMove;            //
    public static event ButtonEvent      OnPause;
    //Ư���� Ŭ������ Ư�� ������Ʈ�� �Բ� ����ؾ� �Ѵ�.
    //Ʈ�� Ŭ������ �ٸ� Ŭ������ Dependence �����ϴ� ���
    //�ٸ� Ŭ������ �ʿ��ؿ�! Require
    //��� ������ Ŭ���� ���ʿ��ٰ� [�̷���] ������ �ִ� ���� Attribute : �Ӽ�
    PlayerInput targetInput;
    Dictionary<string, InputAction> actionDictionary = new();
    List<RaycastResult> cursorHitList = new();

    Vector2 cursorScreenPosition;
    Vector3 cursorWorldPosition;

    public bool is2D = true;

    protected override IEnumerator OnConnected(GameManager newManager)
    {

        

        targetInput = GetComponent<PlayerInput>();
        // ���� ����� ������ Ű ������ �Ұ��� �ϴ�. (�ҿ� ������)
        // Forward�� ���� �˾ƾ� �Ѵ�. => Forward�� ��ư�� �� �� ����
        // On~~�� ������� �ʴ� ���� �ϴ� �̸��� �Լ��� ��ũ��Ʈ���� ã�Ƽ� �ǽð����� ������ �� �ִ� ����� �ҷ��;� �Ѵ�.
        // �� ����� ����Ƽ�� �ƴ϶� ���� ���� �Ⱦ��� ���̴�.
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

    public void UpdateEvent(float deltaTime)// ���콺�� �ö󰡸� ��� ������Ʈ �ȴ�.
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

    void InitializeAllActions() // �̴ϼ� ������ �� �׼� (��� �׼��� ����� ���� �ϳ��� �Լ�)
    {
        if(actionDictionary == null || actionDictionary.Count == 0) return;

        InitializeAction("CursorPositionChanged", (context) => CursorPositionChanged(GetVector2Value(context)));
        InitializeAction("Move", (context) => OnMove?.Invoke(GetVector2Value(context)));

        InitializeAction("MouseLeftButtonDown",  (context) => OnMouseLeftButton?.Invoke( true, cursorScreenPosition, cursorWorldPosition)); //���ٸ� �̿��� �̸� ���� �Լ�
        InitializeAction("MouseLeftButtonUP",    (context) => OnMouseLeftButton?.Invoke( true, cursorScreenPosition, cursorWorldPosition));
        InitializeAction("MouseRightButtonDown", (context) => OnMouseRightButton?.Invoke(true, cursorScreenPosition, cursorWorldPosition));
        InitializeAction("MouseRightButtonUP",   (context) => OnMouseRightButton?.Invoke(true, cursorScreenPosition, cursorWorldPosition));
       
        InitializeAction("Cancel", (context) => OnCancel?.Invoke(true));
        InitializeAction("showStatusButtonDown", (context) => OnShowStatus?.Invoke(true));
        InitializeAction("showStatusButtonUp", (context) => OnShowStatus?.Invoke(true));

        InitializeAction("Pause", (context) => OnPause?.Invoke(true));
     
    }
      
    void InitializeAction(string actionName, Action<InputAction.CallbackContext> actionMethod) // �̴ϼ� ������ �׼� (�� �׼��� ����� ���� �ϳ��� �Լ�)
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


    void CursorPositionChanged(Vector2 screenPosition) // Ŀ�� ������ ä������ �ǽð� ���콺 ��ġ�� ī�޶� ���� ���� 
    {



        // ���콺�� ȭ��� ���� �ȼ� ��ġ (��ǥ�� �⺻ ��ġ)
        // ī�޶� �������� ������ ����.
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

