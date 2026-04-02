using System;
using UnityEngine;

public class UI_MovableScreen : UIBase
{
     Vector3 popupPosition = Vector3.zero;
    Vector3 popupShift = new(20.0f, -20.0f);

    public virtual void Registration(UIManager manager)
    {
        base.Registration(manager);
        UIManager.OnPopUp -= PopUp;
        UIManager.OnPopUp += PopUp;
    }

    public virtual void Unregistration(UIManager manager)
    {
        base.Unregistration(manager);
        UIManager.OnPopUp -= PopUp;
    }

    protected override GameObject OnSetChild(GameObject newchild)
    {
        //새로운 자식한테 UIManager한테 가서 등록 받아오라고 한다.
        UIManager.SetUIM2(newchild); 
        return base.OnSetChild(newchild);
    }

    protected override void OnUnsetChild(GameObject oldChild)
    {
        UIManager.UnsetUIM2(oldChild);
        base.OnUnsetChild(oldChild);
    }

    private void PopUp(string title, string context, string confirm)
    {
        GameObject newChild = SetChild(ObjectManager.CreateObject("PopUp"));
        if (newChild)
        {
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
                    UnsetChild(newChild);// 자식에서 제외
                    ObjectManager.DestroyObject(newChild);// 파괴한다.
                });
            }
            newChild.transform.localPosition = popupPosition;
            popupPosition += popupShift;

        }
    }
}
