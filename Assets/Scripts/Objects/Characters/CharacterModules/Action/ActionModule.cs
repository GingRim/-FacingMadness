using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public abstract class ActionModule : CharacterModule
{
    public abstract ActionType ActionType { get; }

    public abstract bool CanUse(ControllerBase user, in DamageStruct damageInfo);

    public abstract DamageStruct Execute(ControllerBase user, in DamageStruct damageInfo);
}
