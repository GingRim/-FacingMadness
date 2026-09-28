using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAIModule : CharacterModule
{
    [InspectorName("몬스터 AI 설정")]
    [Tooltip("이 몬스터가 사용할 행동·대응·우선순위·조건을 저장한 AI 설정입니다. 비어 있으면 기존 기본 공격과 지능 방어를 사용합니다.")]
    [SerializeField] private MonsterAIProfile profile;

    private readonly Dictionary<MonsterAIAction, int> turnUses = new();
    private readonly Dictionary<MonsterAIAction, int> battleUses = new();
    private readonly Dictionary<MonsterAIAction, int> lastUsedRound = new();
    private readonly Dictionary<MonsterAIReaction, int> reactionRoundUses = new();

    private int reactionUseRound = -1;
    private int actionsUsedThisTurn;
    private bool turnInProgress;
    private BattleManager currentBattle;

    public sealed override Type RegistrationType => typeof(MonsterAIModule);

    public void SetProfile(MonsterAIProfile newProfile)
    {
        profile = newProfile;
        battleUses.Clear();
        lastUsedRound.Clear();
        reactionRoundUses.Clear();
    }

    public void ExecuteTurn(BattleManager battle)
    {
        if (Owner == null || battle == null)
        {
            battle?.EndTurn();
            return;
        }

        currentBattle = battle;
        actionsUsedThisTurn = 0;
        turnUses.Clear();
        turnInProgress = true;

        if (profile == null || profile.Actions == null || profile.Actions.Count == 0)
        {
            ExecuteFallbackAttack(battle);
            return;
        }

        ContinueTurn();
    }

    public bool TryChooseReaction(
        BattleManager battle,
        CharacterBase attacker,
        bool canCounter,
        out ActionType reactionType)
    {
        reactionType = ActionType.None;

        if (Owner == null)
            return false;

        if (profile == null || profile.Reactions == null || profile.Reactions.Count == 0)
            return TryFallbackGuard(out reactionType);

        if (reactionUseRound != battle.Round)
        {
            reactionUseRound = battle.Round;
            reactionRoundUses.Clear();
        }

        List<MonsterAIReaction> candidates = new();
        int highestPriority = int.MinValue;

        foreach (MonsterAIReaction reaction in profile.Reactions)
        {
            if (!CanUseReaction(reaction, attacker, battle, canCounter))
                continue;

            if (reaction.Priority > highestPriority)
            {
                candidates.Clear();
                highestPriority = reaction.Priority;
            }

            if (reaction.Priority == highestPriority)
                candidates.Add(reaction);
        }

        MonsterAIReaction selected = ChooseWeighted(candidates, item => item.Weight);

        if (selected == null)
            return false;

        ActionPointModule actionPoint = Owner.GetModule<ActionPointModule>();

        if (selected.ActionPointCost > 0 &&
            (actionPoint == null || !actionPoint.TryUse(selected.ActionPointCost)))
        {
            return false;
        }

        reactionRoundUses[selected] = GetCount(reactionRoundUses, selected) + 1;
        reactionType = selected.ReactionType;

        BattleManager.ClaimBattleLog(
            $"{Owner.DisplayName}의 대응: {GetReactionName(reactionType)}");

        return reactionType != ActionType.None;
    }

    private void ContinueTurn()
    {
        if (!turnInProgress || currentBattle == null)
            return;

        if (profile == null || actionsUsedThisTurn >= profile.MaxActionsPerTurn)
        {
            FinishTurn();
            return;
        }

        MonsterAIAction action = ChooseAction(currentBattle);

        if (action == null)
        {
            FinishTurn();
            return;
        }

        CharacterBase target = FindTarget(currentBattle, action.TargetType);

        if (action.ActionType != MonsterAIActionType.EndTurn && target == null)
        {
            FinishTurn();
            return;
        }

        ActionPointModule actionPoint = Owner.GetModule<ActionPointModule>();

        if (action.ActionPointCost > 0 &&
            (actionPoint == null || !actionPoint.TryUse(action.ActionPointCost)))
        {
            FinishTurn();
            return;
        }

        RecordActionUse(action, currentBattle.Round);
        actionsUsedThisTurn++;

        bool waitsForAttack = ExecuteAction(action, target, currentBattle);

        if (!turnInProgress || waitsForAttack)
            return;

        ContinueTurn();
    }

    private MonsterAIAction ChooseAction(BattleManager battle)
    {
        List<MonsterAIAction> candidates = new();
        int highestPriority = int.MinValue;

        foreach (MonsterAIAction action in profile.Actions)
        {
            if (action == null)
                continue;

            CharacterBase target = FindTarget(battle, action.TargetType);

            if (!CanUseAction(action, target, battle))
                continue;

            if (action.Priority > highestPriority)
            {
                candidates.Clear();
                highestPriority = action.Priority;
            }

            if (action.Priority == highestPriority)
                candidates.Add(action);
        }

        return ChooseWeighted(candidates, item => item.Weight);
    }

    private bool CanUseAction(
        MonsterAIAction action,
        CharacterBase target,
        BattleManager battle)
    {
        if (action == null)
            return false;

        if (action.ActionType != MonsterAIActionType.EndTurn && target == null)
            return false;

        if (action.MaxUsesPerTurn > 0 &&
            GetCount(turnUses, action) >= action.MaxUsesPerTurn)
            return false;

        if (action.MaxUsesPerBattle > 0 &&
            GetCount(battleUses, action) >= action.MaxUsesPerBattle)
            return false;

        if (action.CooldownRounds > 0 &&
            lastUsedRound.TryGetValue(action, out int lastRound) &&
            battle.Round - lastRound <= action.CooldownRounds)
            return false;

        ActionPointModule actionPoint = Owner.GetModule<ActionPointModule>();

        if (action.ActionPointCost > 0 &&
            (actionPoint == null || !actionPoint.CanUse(action.ActionPointCost)))
            return false;

        return AreConditionsMet(action.Conditions, Owner, target, battle);
    }

    private bool CanUseReaction(
        MonsterAIReaction reaction,
        CharacterBase attacker,
        BattleManager battle,
        bool canCounter)
    {
        if (reaction == null || reaction.ReactionType == ActionType.None)
            return false;

        if (reaction.ReactionType == ActionType.Counterattack && !canCounter)
            return false;

        if (reaction.MaxUsesPerRound > 0 &&
            GetCount(reactionRoundUses, reaction) >= reaction.MaxUsesPerRound)
            return false;

        ActionPointModule actionPoint = Owner.GetModule<ActionPointModule>();

        if (reaction.ActionPointCost > 0 &&
            (actionPoint == null || !actionPoint.CanUse(reaction.ActionPointCost)))
            return false;

        if (!AreConditionsMet(reaction.Conditions, Owner, attacker, battle))
            return false;

        if (UnityEngine.Random.Range(1, 101) > reaction.ChancePercent)
            return false;

        if (!reaction.RequiresStatRoll)
            return true;

        StatModules stats = Owner.GetModule<StatModules>();
        int statValue = stats != null ? stats.GetStat(reaction.RollUnderStat) : 0;

        return UnityEngine.Random.Range(1, 101) <= statValue;
    }

    private bool ExecuteAction(
        MonsterAIAction action,
        CharacterBase target,
        BattleManager battle)
    {
        BattleManager.ClaimBattleLog($"{Owner.DisplayName}: {action.DisplayName}");

        switch (action.ActionType)
        {
            case MonsterAIActionType.Attack:
                ExecuteAttack(action, target, battle);
                return true;

            case MonsterAIActionType.HealSelf:
                RestoreSelf(RollActionValue(action));
                return false;

            case MonsterAIActionType.GainTemporaryArmor:
                AddArmor(RollActionValue(action));
                return false;

            case MonsterAIActionType.ApplyStatusToSelf:
                ApplyStatus(Owner, action.StatusType, RollActionValue(action));
                return false;

            case MonsterAIActionType.ApplyStatusToEnemy:
                ApplyStatus(target, action.StatusType, RollActionValue(action));
                return false;

            case MonsterAIActionType.EndTurn:
                FinishTurn();
                return false;

            default:
                return false;
        }
    }

    private void ExecuteAttack(
        MonsterAIAction action,
        CharacterBase target,
        BattleManager battle)
    {
        int diceValue = RollDice(action.DiceCount, action.DiceType);
        int modifier = GetStatModifier(action.ModifierStat);
        int criticalDamage = 0;
        bool critical = false;

        if (action.AllowCritical)
        {
            StatModules stats = Owner.GetModule<StatModules>();
            int agility = stats != null ? stats.GetStat(StatType.Agility) : 0;
            critical = UnityEngine.Random.Range(1, 101) <= agility;

            if (critical)
                criticalDamage = RollDice(action.DiceCount, action.DiceType);
        }

        int damage = Mathf.Max(
            0,
            action.FixedValue + diceValue + modifier + criticalDamage);

        DamageStruct damageInfo = new DamageStruct
        {
            from = Owner.gameObject,
            instigator = Owner.Controller,
            diceValue = diceValue,
            damageAmount = damage,
            critical = critical,
            highCritical = false,
            damageType = action.DamageType,
            canCounter = action.CanCounter,
            reactionType = ActionType.None
        };

        battle.RequestAttack(Owner, target, damageInfo, false, ContinueTurn);
    }

    private void ExecuteFallbackAttack(BattleManager battle)
    {
        ActionPointModule actionPoint = Owner.GetModule<ActionPointModule>();

        if (actionPoint != null && !actionPoint.TryUse(1))
        {
            FinishTurn();
            return;
        }

        CharacterBase target = FindTarget(battle, MonsterAITargetType.FirstEnemy);

        if (target == null)
        {
            FinishTurn();
            return;
        }

        int attackDice = Dice.RollD10();
        int strengthModifier = GetStatModifier(StatType.Strength);
        StatModules stats = Owner.GetModule<StatModules>();
        int agility = stats != null ? stats.GetStat(StatType.Agility) : 0;
        bool critical = UnityEngine.Random.Range(1, 101) <= agility;
        int criticalDamage = critical ? Dice.RollD10() : 0;

        DamageStruct damageInfo = new DamageStruct
        {
            from = Owner.gameObject,
            instigator = Owner.Controller,
            diceValue = attackDice,
            damageAmount = Mathf.Max(0, attackDice + strengthModifier + criticalDamage),
            critical = critical,
            highCritical = false,
            damageType = DamageType.Hand_to_hand_combat,
            canCounter = true,
            reactionType = ActionType.None
        };

        battle.RequestAttack(Owner, target, damageInfo, true);
        turnInProgress = false;
    }

    private bool TryFallbackGuard(out ActionType reactionType)
    {
        reactionType = ActionType.None;
        StatModules stats = Owner.GetModule<StatModules>();

        if (stats == null)
            return false;

        int intelligence = stats.GetStat(StatType.Intelligence);

        if (UnityEngine.Random.Range(1, 101) > intelligence)
            return false;

        ActionPointModule actionPoint = Owner.GetModule<ActionPointModule>();

        if (actionPoint != null && !actionPoint.TryUse(1))
            return false;

        reactionType = ActionType.Guard;
        BattleManager.ClaimBattleLog($"{Owner.DisplayName}은 방어했다.");
        return true;
    }

    private CharacterBase FindTarget(BattleManager battle, MonsterAITargetType targetType)
    {
        if (targetType == MonsterAITargetType.Self)
            return Owner;

        List<CharacterBase> enemies = battle.GetEnemiesOf(Owner);

        if (enemies == null || enemies.Count == 0)
            return null;

        if (targetType == MonsterAITargetType.RandomEnemy)
            return enemies[UnityEngine.Random.Range(0, enemies.Count)];

        if (targetType == MonsterAITargetType.LowestHealthEnemy)
        {
            CharacterBase selected = null;
            int lowestHealth = int.MaxValue;

            foreach (CharacterBase enemy in enemies)
            {
                HitpointModules hp = enemy.GetModule<HitpointModules>();
                int current = hp != null ? hp.Current : int.MaxValue;

                if (current < lowestHealth)
                {
                    lowestHealth = current;
                    selected = enemy;
                }
            }

            return selected ?? enemies[0];
        }

        return enemies[0];
    }

    private static bool AreConditionsMet(
        IReadOnlyList<MonsterAICondition> conditions,
        CharacterBase self,
        CharacterBase target,
        BattleManager battle)
    {
        if (conditions == null)
            return true;

        foreach (MonsterAICondition condition in conditions)
        {
            if (condition != null && !condition.IsMet(self, target, battle))
                return false;
        }

        return true;
    }

    private void RecordActionUse(MonsterAIAction action, int round)
    {
        turnUses[action] = GetCount(turnUses, action) + 1;
        battleUses[action] = GetCount(battleUses, action) + 1;
        lastUsedRound[action] = round;
    }

    private int RollActionValue(MonsterAIAction action)
    {
        return Mathf.Max(
            0,
            action.FixedValue +
            RollDice(action.DiceCount, action.DiceType) +
            GetStatModifier(action.ModifierStat));
    }

    private int GetStatModifier(StatType statType)
    {
        if (statType == StatType.None)
            return 0;

        StatModules stats = Owner.GetModule<StatModules>();
        return stats != null ? stats.GetModifier(statType) : 0;
    }

    private static int RollDice(int count, MonsterAIDiceType diceType)
    {
        int sides = GetDiceSides(diceType);

        if (count <= 0 || sides <= 0)
            return 0;

        int result = 0;

        for (int i = 0; i < count; i++)
            result += UnityEngine.Random.Range(1, sides + 1);

        return result;
    }

    private static int GetDiceSides(MonsterAIDiceType diceType)
    {
        switch (diceType)
        {
            case MonsterAIDiceType.D4: return 4;
            case MonsterAIDiceType.D6: return 6;
            case MonsterAIDiceType.D8: return 8;
            case MonsterAIDiceType.D10: return 10;
            case MonsterAIDiceType.D12: return 12;
            case MonsterAIDiceType.D20: return 20;
            default: return 0;
        }
    }

    private void RestoreSelf(int amount)
    {
        CombatModule combat = Owner.GetModule<CombatModule>();

        if (combat == null || amount <= 0)
            return;

        RestoreStruct restore = new RestoreStruct
        {
            from = Owner.gameObject,
            instigator = Owner.Controller,
            restoreAmount = amount
        };

        combat.OnRestore(restore);
    }

    private void AddArmor(int amount)
    {
        ArmorModule armor = Owner.GetModule<ArmorModule>();
        armor?.AddTemporaryArmor(amount);
    }

    private static void ApplyStatus(
        CharacterBase target,
        StatusEffectType statusType,
        int amount)
    {
        StatusEffectModule status = target != null
            ? target.GetModule<StatusEffectModule>()
            : null;

        status?.AddStatus(statusType, amount);
    }

    private void FinishTurn()
    {
        if (!turnInProgress)
            return;

        turnInProgress = false;
        currentBattle?.EndTurn();
    }

    private static T ChooseWeighted<T>(List<T> candidates, Func<T, int> getWeight)
        where T : class
    {
        if (candidates == null || candidates.Count == 0)
            return null;

        int totalWeight = 0;

        foreach (T candidate in candidates)
            totalWeight += Mathf.Max(1, getWeight(candidate));

        int roll = UnityEngine.Random.Range(0, totalWeight);

        foreach (T candidate in candidates)
        {
            roll -= Mathf.Max(1, getWeight(candidate));

            if (roll < 0)
                return candidate;
        }

        return candidates[candidates.Count - 1];
    }

    private static int GetCount<T>(Dictionary<T, int> source, T key)
    {
        return source.TryGetValue(key, out int count) ? count : 0;
    }

    private static string GetReactionName(ActionType reactionType)
    {
        switch (reactionType)
        {
            case ActionType.Guard: return "방어";
            case ActionType.Evade: return "회피";
            case ActionType.Counterattack: return "반격";
            default: return "대응 없음";
        }
    }
}
