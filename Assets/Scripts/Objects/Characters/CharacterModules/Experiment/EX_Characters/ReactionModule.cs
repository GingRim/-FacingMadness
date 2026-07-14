using UnityEngine;

public class ReactionModule : CharacterModule
{
    public sealed override System.Type RegistrationType
        => typeof(ReactionModule);

    /// <summary>
    /// 대응을 하나라도 사용할 수 있는지 확인.
    /// 대응 팝업을 띄울지 판단할 때 사용.
    /// </summary>
    public bool CanUseAnyReaction()
    {
        if (Owner == null)
            return false;

        CostModule cost = Owner.GetModule<CostModule>();

        if (cost == null)
            return false;

        return cost.CanUse(CostType.Reaction, 1);
    }


    /// <summary>
    /// 특정 대응 행동을 사용할 수 있는지 확인.
    /// </summary>
    public bool CanUse(ActionType actionType)
    {
        if (Owner == null)
            return false;

        if (!IsReactionAction(actionType))
            return false;

        CostModule cost = Owner.GetModule<CostModule>();

        if (cost == null)
            return false;

        return cost.CanUse(CostType.Reaction, 1);
    }


    /// <summary>
    /// 대응 사용.
    /// 여기서는 피해 계산을 하지 않고,
    /// 선택한 대응 타입만 DamageStruct에 기록한다.
    /// </summary>
    public bool TryUse(ActionType actionType, in DamageStruct damageInfo, out DamageStruct result)
    {
        result = damageInfo;

        // 취소는 코스트를 쓰지 않는다.
        if (actionType == ActionType.None)
        {
            result.reactionType = ActionType.None;
            return true;
        }

        if (!CanUse(actionType))
            return false;

        CostModule cost =
            Owner.GetModule<CostModule>();

        if (cost == null)
            return false;

        Debug.Log(
            $"{Owner.name}: 대응 코스트 사용 전 / {cost.GetCurrent(CostType.Reaction)}"
        );

        if (!cost.Use(CostType.Reaction, 1))
            return false;

        Debug.Log(
            $"{Owner.name}: 대응 코스트 사용 후 / {cost.GetCurrent(CostType.Reaction)}"
        );

        result.reactionType = actionType;

        Debug.Log($"{Owner.name}: 대응 선택 / {actionType}");

        return true;
    }

    private bool IsReactionAction(ActionType actionType)
    {
        switch (actionType)
        {
            case ActionType.Guard:
            case ActionType.Evade:
            case ActionType.Counterattack:
                return true;

            default:
                return false;
        }
    }
}
