using UnityEngine;

public class GuardModule : ActionModule
{
    public override ActionType ActionType => ActionType.Guard; // 이게 가드야 선언

    public override bool CanUse(ControllerBase user, in DamageStruct damageInfo) //유저 코스트 확인용
    {
        // 임시: 나중에 대응 코스트 확인으로 교체(코스트 있다면 true 없다면 flos)
        return true;
    }

    public override DamageStruct Execute(ControllerBase user, in DamageStruct damageInfo) // 대응에 대한 결과
    {
        DamageStruct result = damageInfo;

        result.damageAmount /= 2;

        return result;
    }
}
