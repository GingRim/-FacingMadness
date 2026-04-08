using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UI_MovableScreen : UI_ScreenBase
{
    [SerializeField] List<UIBase> popupList = new();
    Vector3 popupPosition = Vector3.zero;
    Vector3 popupShift = new(20.0f, -20.0f);
    UI_DraggableWindow currentDragTarget = null;

    public override void Registration(UIManager manager)
    {
        base.Registration(manager);
        InputManager.OnCancel += (value) => UIManager.ToggleUIM2(UIType.Menu);
        InputManager.OnMouseMove -= MouseMove;
        InputManager.OnMouseMove += MouseMove;
        InputManager.OnMouseLeftButton -= MouseLeft;
        InputManager.OnMouseLeftButton += MouseLeft;
        UIManager.OnPopUp -= PopUp;
        UIManager.OnPopUp += PopUp;
    }

    private void MouseLeftUp(Vector2 screenPosition, Vector3 WorldPosition)
    {
        currentDragTarget = null;
    }

    private void MouseLeftDown(Vector2 screenPosition, Vector3 WorldPosition)
    {

    }

    void SetDragTarget(UI_DraggableWindow dragTarget, Vector2 startPosition)
    {
        currentDragTarget = dragTarget;
        if (currentDragTarget)
        {
            currentDragTarget.SetMouseStartPosition(startPosition);
        }
    }

    private void MouseMove(Vector2 screenPosition, Vector3 WorldPosition)
    {
        if (currentDragTarget) // 지금 움지여야 하는 친구한테
        { // 움직이라고 이야기 하기!
            currentDragTarget.SetMouseCurrentPosition(screenPosition);
        }
    }

    public override void Unregistration(UIManager manager)
    {
        base.Unregistration(manager);
        InputManager.OnMouseMove -= MouseMove;
        InputManager.OnMouseLeftButton -= MouseLeft;
       

        UIManager.OnPopUp -= PopUp;
    }

    private void MouseLeft(bool value, Vector2 screenPosition, Vector3 WorldPosition)
    {
        throw new NotImplementedException();
    }

    protected override GameObject OnSetChild(GameObject newChild)
    {
        //새로운 자식한테 UIManager한테 가서 등록 받아오라고 한다.
        UIManager.SetUIM2(newChild);

        if (newChild)
        {
            UI_DraggableWindow asDraggable = newChild.GetComponentInChildren<UI_DraggableWindow>();

            if(asDraggable)
            {
                // 좋아 너 움직일 수 있다는 것 알겠어!
                // 이 친구가 움직임을 원할 때 내 SetDragTarget함수를 실행시킬 수 있게
                asDraggable.OnDragStart -= SetDragTarget;
                asDraggable.OnDragStart += SetDragTarget;
                

            }
        }

        return base.OnSetChild(newChild);
    }

    protected override void OnUnsetChild(GameObject oldChild)
    {
        UIManager.UnsetUIM2(oldChild);
        if (oldChild)
        {
            UI_DraggableWindow asDraggable = oldChild.GetComponentInChildren<UI_DraggableWindow>();
            if(asDraggable)
            {
                asDraggable.OnDragStart -= SetDragTarget;
            }
        }
        base.OnUnsetChild(oldChild);
    }

    private void PopUp(string title, string context, string confirm)
    {
        GameObject newChild = SetChild(ObjectManager.CreateObject("PopUp"));
        if (newChild)
        {
            newChild.transform.localPosition = GetNextPopipPosition();

            if(newChild.TryGetComponent(out UIBase newUI))
            {
                if(!popupList.Contains(newUI)) popupList.Add(newUI);
            }

            //이 친구가 시스템 메시지를 받을 수 있는 가?
            //IS시스템 메시지 인지 체크를 하고
            //메시지를 보내주기만 하면 끝!

            if (newChild.TryGetComponent(out ISystemMessagePossible target))
            {
                target.SetSystemMessage(title, context, confirm);
            }
            if(newChild.TryGetComponent(out IConfirmable confirmTarget))
            {
                confirmTarget.SetConfirmAction(() => // 팝업창을 누른다.
                {
                    if(newUI) popupList.Remove(newUI);// 너는 팝업도 아니고
                    UnsetChild(newChild);// 자식에서 제외
                    ObjectManager.DestroyObject(newChild);// 파괴한다.
                });
            }

        }
    }

    public Vector3 GetNextPopipPosition()
    {
        //그러면 팝업 포지션은 어떻게 계산할까?
        //지금 가지고 있는 팝업 리스트 중에서 가장 오른쪽 아래에 있는 녀석을 구하기!
        //아무도 없으면? Vector3.zero
        Vector3 bestScore = Vector3.zero;
       
        if(popupList.Count == 0) return bestScore;
        
        foreach (UIBase currentPopup in popupList)
        {
            Vector3 currentScore = currentPopup.transform.localPosition;
            //1.    X축 일등인지
            if (bestScore.x < currentScore.x) bestScore.x = currentScore.x;
            //2.    Y축 일등인지 
            if (bestScore.y > currentScore.y) bestScore.y = currentScore.y;
        }

        return bestScore + popupShift;
    }

}
