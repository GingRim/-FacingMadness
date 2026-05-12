using TMPro;
using UnityEngine;

public class UI_CostHoverInfo : OpenableUIBase
{
    [SerializeField] Vector2 shiftedPosition;

    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI commentText;

    UI_Cost target;

    public override void Registration(UIManager manager)
    {
        base.Registration(manager);

        InputManager.OnMouseHover -= HoverInfoChange;
        InputManager.OnMouseHover += HoverInfoChange;

        // 마우스를 따라가게 하고 싶으면 사용
        // InputManager.OnMouseMove -= MoveToMouse;
        // InputManager.OnMouseMove += MoveToMouse;
    }

    public override void Unregistration(UIManager manager)
    {
        base.Unregistration(manager);

        InputManager.OnMouseHover -= HoverInfoChange;
       // InputManager.OnMouseMove -= MoveToMouse;
    }

    private void MoveToMouse(Vector2 screenPosition, Vector3 worldPosition)
    {
        transform.position = screenPosition + shiftedPosition;
    }

    void HoverInfoChange(GameObject newTarget, GameObject oldTarget)
    {
        UI_Cost asCost = newTarget?.GetComponent<UI_Cost>();

        if (asCost == null)
        {
            target = null;
            Close();
            return;
        }

        target = asCost;

        SetCostInfo(target.CostType);

        Open();
    }

    void SetCostInfo(CostType type)
    {
        switch (type)
        {
            case CostType.Action:
                nameText.SetText("행동");
                commentText.SetText("공격, 회복, 등 주요 행동 카드를 사용하는 코스트입니다.");
                break;

            case CostType.Auxiliary:
                nameText.SetText("보조 행동");
                commentText.SetText("버프, 행동의 보조 혹은 행동+보조 행동에 사용되는 코스트입니다.");
                break;

            case CostType.Reaction:
                nameText.SetText("대응 코스트");
                commentText.SetText("가드, 회피, 반격처럼 피격 상황에서 사용하는 코스트입니다.");
                break;

            default:
                nameText.SetText("알 수 없는 코스트");
                commentText.SetText("존재해선 않되는 기억");
                break;
        }
    }
}
