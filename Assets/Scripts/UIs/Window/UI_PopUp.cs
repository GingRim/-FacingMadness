using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UI_PopUp : UIBase, ISystemMessagePossible, IConfirmable
{
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI contextText;
    [SerializeField] TextMeshProUGUI confirmText;
    [SerializeField] Button confirmButton;
    Action ConfirmAction;

    public override void Registration(UIManager manager)
    {
        base.Registration(manager);
        confirmButton.onClick.AddListener(Confirm); //만들 때 추가하고

    }

    public override void Unregistration(UIManager manager)
    {
        base.Unregistration(manager);
        confirmButton.onClick.RemoveListener(Confirm); // 제거할 때 빼기
        ConfirmAction = null;
    }

    public void Confirm()
    {
        ConfirmAction?.Invoke();
    }

    public void SetConfirmAction(Action newAction)
    {
        ConfirmAction -= newAction;
        ConfirmAction += newAction;
        confirmButton.onClick.AddListener(Confirm); // 두번 발동하면 한번 발동해도 두번 동시에 발동된다.
    }

    public void SetSystemMessage(string title, string context, string confirm)
    {
        titleText?.SetText(title);
        contextText?.SetText(context);
        confirmText?.SetText(confirm);
    }
}
