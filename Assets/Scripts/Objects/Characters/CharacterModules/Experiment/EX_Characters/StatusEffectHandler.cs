using UnityEngine;

public abstract class StatusEffectHandler
{
    protected StatusEffectModule owner;

    public abstract StatusEffectType Type { get; }
    public virtual int MaxStack => 1;

    public void Initialize(StatusEffectModule owner)
    {
        this.owner = owner;
    }

    public virtual void OnAdd(int value) { }
    public virtual void OnTurnEnd() { }
    public virtual void OnRoundEnd() { }
    public virtual void OnBattleEnd() { }

    public virtual int GetInitiativeBonus(int stack)
    {
        return 0;
    }

    public virtual int ReduceDamage(int damage, DamageType damageType, int stack)
    {
        return damage;
    }
}
