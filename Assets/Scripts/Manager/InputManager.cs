using System;
using System.Collections;
using System.Collections.Generic;
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
public delegate void MouseHoverEvent(GameObject newTarget, GameObject oldTarget);
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
    public static event MouseHoverEvent  OnMouseHover;
    public static event ButtonEvent      OnCancel;          // 
    public static event ButtonEvent      OnShowStatus;      //
    public static event VectorEvent      OnMove;            //
    public static event ButtonEvent      OnPause;
    //Ư���� Ŭ������ Ư�� ������Ʈ�� �Բ� ����ؾ� �Ѵ�.
    //Ʈ�� Ŭ������ �ٸ� Ŭ������ Dependence �����ϴ� ���
    //�ٸ� Ŭ������ �ʿ��ؿ�! Require
   
    PlayerInput targetInput;
    Dictionary<string, InputAction> actionDictionary = new();
    List<RaycastResult> cursorHitList = new();

    GameObject cursorHoverObhect;
    Vector2 cursorScreenPosition;
    Vector3 cursorWorldPosition;


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
        RefreshGameObjectUnderCursor(cursorScreenPosition);
    }

    void RefreshGameObjectUnderCursor(Vector2 screenPosition)
    {
        cursorHitList.Clear();
         GameManager.Instance.Camera.GetRaycastResult(screenPosition, cursorHitList);

        // ���콺�� ȭ��� ���� �ȼ� ��ġ (��ǥ�� �⺻ ��ġ)
        // ī�޶� �������� ������ ����.
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        GameObject firstObject = null; //1등 선언
        
        if (cursorHitList.Count > 0 && cursorHitList[0].element != null)
        {
            firstObject = cursorHitList[0].gameObject;
        }
        if (GameManager.is2D)
        {
            //-32768 ~ 32767 만 가능하기 때문에
            //lauer가 1일 때에 67232 ~132767이기 때문에
            worldPosition.z = 0;
            float GetValue(RaycastResult target)
            {
                return target.sortingOrder + target.sortingLayer * 100000;
            }
            RaycastResult nearest = cursorHitList.GetMaximum<RaycastResult>(GetValue);
            firstObject = nearest.gameObject;
        }
        else
        {
            float GetDistance(RaycastResult target)
            {
                return target.distance;
            }

            RaycastResult nearest = cursorHitList.GetMinimum<RaycastResult>(GetDistance);
            firstObject = nearest.gameObject; // 오브젝트 꺼내오기 
            worldPosition = nearest.worldPosition; // 위치에 꺼내오고
        }

        GameObject lastHoverObject = cursorHoverObhect;

        cursorScreenPosition = screenPosition;
        cursorWorldPosition = worldPosition;
        cursorHoverObhect = firstObject;
        
        if (lastHoverObject != firstObject)
        {
            OnMouseHover?.Invoke(firstObject, lastHoverObject);
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
        InitializeAction("Move"                 , (context) => OnMove?.Invoke(GetVector2Value(context))
                                                , (context) => OnMove?.Invoke(Vector2.zero));

        InitializeAction("MouseLeftButton"      ,  (context) => OnMouseLeftButton?.Invoke( true, cursorScreenPosition, cursorWorldPosition) //���ٸ� �̿��� �̸� ���� �Լ�
                                                , (context) => OnMouseLeftButton?.Invoke( false, cursorScreenPosition, cursorWorldPosition));
       
        InitializeAction("MouseRightButton"     , (context) => OnMouseRightButton?.Invoke(true, cursorScreenPosition, cursorWorldPosition)
                                                , (context) => OnMouseRightButton?.Invoke(false, cursorScreenPosition, cursorWorldPosition));
      
        InitializeAction("showStatusButton"     , (context) => OnShowStatus?.Invoke(true)
                                                , (context) => OnShowStatus?.Invoke(false));

        InitializeAction("Cancel"               , (context) => OnCancel?.Invoke(true));
        InitializeAction("Pause"                , (context) => OnPause?.Invoke(true));
     
    }
    void InitializeAction(string actionName, Action<InputAction.CallbackContext> actionMethod, Action<InputAction.CallbackContext> cancelMethod = null) // �̴ϼ� ������ �׼� (�� �׼��� ����� ���� �ϳ��� �Լ�)
    {
        if (actionDictionary == null || actionDictionary.Count == 0) return;

        if (actionDictionary.TryGetValue(actionName, out InputAction currentInput))
        {   //발동할때 할 일
            if(actionMethod is not null) currentInput.performed += actionMethod;
            //취소될 때 할 일
            if (actionMethod is not null) currentInput.canceled += cancelMethod;
            //currentInput.started 키가 눌렀을 때 발동 된다. 무지성 발동
        }
    }

    Vector2 GetVector2Value(InputAction.CallbackContext context)
    {
        if(context.valueType != typeof(Vector2)) return Vector2.zero;
        return context.ReadValue<Vector2>();
    }


    void CursorPositionChanged(Vector2 screenPosition) // Ŀ�� ������ ä������ �ǽð� ���콺 ��ġ�� ī�޶� ���� ���� 
    {


        RefreshGameObjectUnderCursor(screenPosition); //세로고침 한번 때려주고!



        OnMouseMove?.Invoke(screenPosition, cursorWorldPosition);
    }

    void MouseButtonAction(InputAction.CallbackContext context)
    {

    }
 
}

