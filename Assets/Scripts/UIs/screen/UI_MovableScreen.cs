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
        //InputManager.OnCancel += (value) => UIManager.ToggleUIM2(UIType.Menu);
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
        if (currentDragTarget) // ���� �������� �ϴ� ģ������
        { // �����̶�� �̾߱� �ϱ�!
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
        
    }

    protected override GameObject OnSetChild(GameObject newChild)
    {
        //���ο� �ڽ����� UIManager���� ���� ��� �޾ƿ���� �Ѵ�.
        UIManager.SetUIM2(newChild);

        if (newChild)
        {
            UI_DraggableWindow asDraggable = newChild.GetComponentInChildren<UI_DraggableWindow>();

            if(asDraggable)
            {
                // ���� �� ������ �� �ִٴ� �� �˰ھ�!
                // �� ģ���� �������� ���� �� �� SetDragTarget�Լ��� �����ų �� �ְ�
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

            //�� ģ���� �ý��� �޽����� ���� �� �ִ� ��?
            //IS�ý��� �޽��� ���� üũ�� �ϰ�
            //�޽����� �����ֱ⸸ �ϸ� ��!

            if (newChild.TryGetComponent(out ISystemMessagePossible target))
            {
                target.SetSystemMessage(title, context, confirm);
            }
            if(newChild.TryGetComponent(out IConfirmable confirmTarget))
            {
                confirmTarget.SetConfirmAction(() => // �˾�â�� ������.
                {
                    if(newUI) popupList.Remove(newUI);// �ʴ� �˾��� �ƴϰ�
                    UnsetChild(newChild);// �ڽĿ��� ����
                    ObjectManager.DestroyObject(newChild);// �ı��Ѵ�.
                });
            }

        }
    }

    public Vector3 GetNextPopipPosition()
    {
        //�׷��� �˾� �������� ��� ����ұ�?
        //���� ������ �ִ� �˾� ����Ʈ �߿��� ���� ������ �Ʒ��� �ִ� �༮�� ���ϱ�!
        //�ƹ��� ������? Vector3.zero
        Vector3 bestScore = Vector3.zero;
       
        if(popupList.Count == 0) return bestScore;
        
        foreach (UIBase currentPopup in popupList)
        {
            Vector3 currentScore = currentPopup.transform.localPosition;
            //1.    X�� �ϵ�����
            if (bestScore.x < currentScore.x) bestScore.x = currentScore.x;
            //2.    Y�� �ϵ����� 
            if (bestScore.y > currentScore.y) bestScore.y = currentScore.y;
        }

        return bestScore + popupShift;
    }

}
