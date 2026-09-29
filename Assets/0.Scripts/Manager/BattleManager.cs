using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : ManagerBase
{
    private readonly List<CharacterBase> participants = new();
    private readonly List<CharacterBase> turnOrder = new();

    private int round;
    private int currentTurnIndex;
    private UI_Hand handUI;

    private CharacterBase pendingAttacker;
    private CharacterBase pendingDefender;
    private DamageStruct pendingDamageInfo;

    // 같은 공격자의 한 턴 동안 처음 선택한 대응을 후속 공격에 재사용합니다.
    private CharacterBase reactionChainAttacker;
    private CharacterBase reactionChainDefender;
    private ActionType reactionChainType;
    private bool hasReactionChainChoice;

    private bool waitingReaction;
    private bool endTurnAfterReaction;
    private bool isBattleActive;
    private bool startedFromActiveField;
    private Action pendingAttackResolved;

    private UI_ReactionSelect reactionSelectUI;

    public static event Action<string> OnBattleLog;
    public static event Action OnBattleLogClear;
    public static event Action<int> OnRoundChanged;
    public static event Action OnBattleStarted;
    public static event Action<bool> OnBattleEnded;

    public int Round => round;
    public int DisplayRound => round;

    public CharacterBase CurrentCharacter { get; private set; }
    public BattleTurnState State { get; private set; }

    protected override IEnumerator OnConnected(GameManager newManager)
    {
        State = BattleTurnState.None;
        round = 0;
        currentTurnIndex = 0;

        handUI = FindFirstObjectByType<UI_Hand>(FindObjectsInactive.Include);

        if (handUI == null)
        {
            Debug.LogWarning("BattleManager: UI_Hand를 찾지 못했습니다.");
        }


        yield return null;
    }

    protected override void OnDisconnected()
    {
        ClearReactionChainChoice();

        participants.Clear();
        turnOrder.Clear();

        CurrentCharacter = null;
        State = BattleTurnState.None;
    }

    private void OnRoundStart(CharacterBase character)
    {
        ResetCost(character);
    }

    private void OnRoundEnd(CharacterBase character)
    {
        ArmorModule armor = character.GetModule<ArmorModule>();

        if (armor != null)
        {
            armor.ClearTemporaryArmor();
        }

        StatusEffectModule status = character.GetModule<StatusEffectModule>();

        if (status != null)
        {
            status.OnRoundEnd();
        }
    }

    private void OnTurnStart(CharacterBase character)
    {
        if (waitingReaction || State == BattleTurnState.WaitingReaction)
        {
            Debug.Log("StartTurn 중단: 대응 선택 대기 중");
            return;
        }

        Debug.Log($"OnTurnStart 호출: {character.name}");

        if (!IsPlayer(character))
        {
            return;
        }


        ProcessHandTurnDurability(character);

        DrawCardsByIntelligence(character);
    }

    private void OnTurnEnd(CharacterBase character)
    {
        StatusEffectModule status = character.GetModule<StatusEffectModule>();

        if (status != null)
        {
            status.OnTurnEnd();
        }
    }

    public void StartBattle(List<CharacterBase> characters)
    {
        isBattleActive = true;
        ClearReactionChainChoice();

        FieldManager fieldManager =
            GameManager.Instance != null
                ? GameManager.Instance.Field
                : null;

        startedFromActiveField =
            fieldManager != null && fieldManager.IsFieldActive;

        BindBattleUI();
        participants.Clear();
        turnOrder.Clear();



        if (characters != null)
        {
            foreach (CharacterBase character in characters)
            {
                if (character == null)
                    continue;

                character.AddAllModuleFromObject(character.gameObject);

                PrepareParticipant(character);
                participants.Add(character);


                Debug.Log(
                    $"전투 참가자 등록: {character.name} / " +
                    $"Controller={(character.Controller != null ? character.Controller.GetType().Name : "null")}");
            }
        }

        round = 0;
        currentTurnIndex = 0;

        State = BattleTurnState.BattleStart;

        Debug.Log("전투 시작");

        OnBattleStarted?.Invoke();

        StartRound();
    }

    private void StartRound()
    {
        if (!isBattleActive)
            return;

        if (State == BattleTurnState.BattleEnd)
            return;

        round++;

        State = BattleTurnState.RoundStart;

        Debug.Log($"라운드 시작: {round}");

        OnRoundChanged?.Invoke(round);

        foreach (CharacterBase character in participants)
        {
            if (character == null)
                continue;

            if (!IsAlive(character))
                continue;

            OnRoundStart(character);
        }

        BuildTurnOrder();

        currentTurnIndex = 0;

        StartTurn();
    }

    private void StartTurn()
    {
        if (waitingReaction || State == BattleTurnState.WaitingReaction)
        {
            Debug.Log("StartTurn 중단: 대응 선택 대기 중");
            return;
        }

        if (currentTurnIndex >= turnOrder.Count)
        {
            EndRound();
            return;
        }

        CurrentCharacter = turnOrder[currentTurnIndex];

        if (CurrentCharacter == null)
        {
            EndCurrentTurn();
            return;
        }

        State = BattleTurnState.TurnStart;

        while (currentTurnIndex < turnOrder.Count)
        {
            CharacterBase candidate =
                turnOrder[currentTurnIndex];

            if (IsAlive(candidate))
                break;

            Debug.Log(
                $"{candidate?.name}: 생명력 0으로 턴 제외");

            currentTurnIndex++;
        }

        if (currentTurnIndex >= turnOrder.Count)
        {
            EndRound();
            return;
        }

        CurrentCharacter = turnOrder[currentTurnIndex];

        // 새 캐릭터의 턴이 시작되면 이전 공격자의 연속 대응 선택은 종료됩니다.
        ClearReactionChainChoice();
        WriteBattleLog($"{CurrentCharacter.name}의 턴입니다.");

        OnTurnStart(CurrentCharacter);

        Debug.Log(
            $"턴 시작 대상: {CurrentCharacter.name} / " +
            $"Controller={(CurrentCharacter.Controller != null ? CurrentCharacter.Controller.GetType().Name : "null")}"
        );

        // 몬스터 턴이면 AI 실행
        if (IsMonster(CurrentCharacter))
        {
            MonsterAIModule ai = CurrentCharacter.GetModule<MonsterAIModule>();

            if (ai == null)
            {
                Debug.LogWarning($"{CurrentCharacter.name}: MonsterAIModule 없음. 턴 종료");
                EndCurrentTurn();
                return;
            }

            ai.ExecuteTurn(this);
            return;
        }


        // 플레이어 턴이면 입력 대기
        State = BattleTurnState.WaitingAction;
    }

    public void EndTurn()
    {
        if (!isBattleActive)
            return;

        if (State == BattleTurnState.BattleEnd)
            return;

        if (waitingReaction || State == BattleTurnState.WaitingReaction)
        {
            Debug.Log("EndTurn 중단: 대응 선택 대기 중");
            return;
        }

        if (CurrentCharacter != null)
        {
            Debug.Log($"턴 종료: {CurrentCharacter.name}");

            OnTurnEnd(CurrentCharacter);
        }

        currentTurnIndex++;

        State = BattleTurnState.TurnEnd;

        StartTurn();
    }

    private void EndRound()
    {
        State = BattleTurnState.RoundEnd;

        Debug.Log($"라운드 종료: {round}");

        foreach (CharacterBase character in participants)
        {
            if (character == null)
                continue;

            OnRoundEnd(character);
        }

        StartRound();
    }

    private bool IsPlayer(CharacterBase character)
    {
        if (character == null)
            return false;

        // 컨트롤러가 캐릭터와 다른 오브젝트에 있어도
        // 실제 Possess 결과를 기준으로 플레이어를 판별한다.
        return character.Controller is PlauerController;
    }

    private bool IsMonster(CharacterBase character)
    {
        return character != null && character.Controller == null;
    }

    private void DrawCard(CharacterBase character)
    {
        if (character == null)
            return;

        StatusEffectModule status = character.GetModule<StatusEffectModule>();

        if (status != null && status.ConsumeDrawBlock())
        {
            Debug.Log($"{character.name}: " + "드로우 제한으로 드로우 취소");

            return;
        }

        DeckModule deck = character.GetModule<DeckModule>();

        if (deck == null)
        {
            Debug.LogWarning($"{character.name}: DeckModule 없음");

            return;
        }

        CardInstance drawCard = deck.DrawInstance();

        if (drawCard == null || drawCard.Data == null)
        {
            Debug.LogWarning($"{character.name}: 드로우 실패");

            return;
        }

        Debug.Log($"{character.name} 드로우: " + $"{drawCard.CardName}");
    }


    private void ResetCost(CharacterBase character)
    {
        ActionPointModule actionPoint = character.GetModule<ActionPointModule>();
        DerivedStatModule derived = character.GetModule<DerivedStatModule>();
        LVModules level = character.GetModule<LVModules>();

        if (actionPoint == null || derived == null || level == null)
        {
            Debug.LogWarning($"{character.name}: 행동력 계산 모듈 없음");
            return;
        }

        // 필드에서 전투로 진입한 플레이어는 첫 전투 라운드에
        // 필드에서 남아 있던 행동력을 그대로 사용합니다.
        // 필드를 거치지 않은 직접 전투는 설정된 행동력이 없으므로 정상 초기화합니다.
        if (round == 1 &&
            IsPlayer(character) &&
            startedFromActiveField &&
            actionPoint.IsConfigured)
        {
            return;
        }

        actionPoint.PrepareTurn(derived.GetAgilityModifier(), level.Level);

    }

    private void BuildTurnOrder()
    {
        turnOrder.Clear();

        foreach (CharacterBase character in participants)
        {
            if (character == null)
                continue;

            turnOrder.Add(character);
        }

        turnOrder.Sort((a, b) =>
        {
            int aInitiative = GetInitiative(a);
            int bInitiative = GetInitiative(b);

            return bInitiative.CompareTo(aInitiative);
        });


        foreach (CharacterBase character in turnOrder)
        {
            Debug.Log($"{character.name} / 우선권 {GetInitiative(character)}");
        }
    }

    private int GetInitiative(CharacterBase character)
    {
        if (character == null)
            return 0;

        DerivedStatModule derived = character.GetModule<DerivedStatModule>();

        LVModules lv = character.GetModule<LVModules>();

        DeckModule deck = character.GetModule<DeckModule>();

        if (derived == null || lv == null)
            return 0;

        int handSize;

        if (deck != null)
        {
            handSize = deck.HandCount;
        }
        else
        {
            handSize = 0;
        }

        return derived.GetInitiative(lv.Level, handSize);
    }

    private void DrawCardsByIntelligence(CharacterBase character)
    {
        DerivedStatModule derived = character.GetModule<DerivedStatModule>();

        if (derived == null)
        {
            Debug.LogWarning($"{character.name}: DerivedStatModule 없음");
            return;
        }

        DeckModule deck = character.GetModule<DeckModule>();

        if (deck == null)
        {
            Debug.LogWarning($"{character.name}: DeckModule 없음");
            return;
        }

        StatusEffectModule status = character.GetModule<StatusEffectModule>();

        if (status != null && status.ConsumeDrawBlock())
        {
            Debug.Log($"{character.name}: 드로우 제한으로 턴 시작 드로우 취소");
            return;
        }

        int drawCount = 1 + derived.GetDrawBonus();


        for (int i = 0; i < drawCount; i++)
        {
            CardInstance drawCard = deck.DrawInstance();

            if (drawCard == null)
            {
                break;
            }

            Debug.Log($"{character.name} 드로우: " + $"{drawCard.CardName}");
        }
        RefreshHandUI(deck);

    }

    private void RefreshHandUI(DeckModule deck)
    {
        if (deck == null)
            return;

        if (handUI == null)
        {
            handUI = FindFirstObjectByType<UI_Hand>(FindObjectsInactive.Include);
        }

        if (handUI == null)
        {
            Debug.LogWarning("Hand UI 갱신 실패: UI_Hand 없음");
            return;
        }

        handUI.RefreshFromDeck(deck);

    }

    private void PrepareParticipant(CharacterBase character)
    {
        if (character == null)
            return;

        character.AddAllModuleFromObject(character.gameObject);

        HitpointModules hp = character.GetModule<HitpointModules>();

        if (hp != null)
        {
            hp.OnEmpty -= CheckBattleEnd;
            hp.OnEmpty += CheckBattleEnd;
        }

        SanityModule sanity = character.GetModule<SanityModule>();

        if (sanity != null)
        {
            sanity.OnEmpty -= CheckBattleEnd;
            sanity.OnEmpty += CheckBattleEnd;
        }

        ControllerBase controller =
            character.GetComponent<ControllerBase>();

        if (controller != null &&
            character.Controller == null)
        {
            controller.Possess(character);

            Debug.Log(
                $"{character.name}: Controller Possess 실행");
        }
    }

    /// <summary>
    /// 적 & 몬스터 리스트
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public List<CharacterBase> GetEnemiesOf(CharacterBase user)
    {
        List<CharacterBase> result = new();

        if (user == null)
            return result;



        bool userIsPlayer = user.Controller != null;

        foreach (CharacterBase character in participants)
        {
            if (character == null)
                continue;

            if (character == user)
                continue;

            if (!IsAlive(character))
                continue;

            bool targetIsPlayer = character.Controller != null;

            if (userIsPlayer != targetIsPlayer)
            {
                result.Add(character);
            }
        }

        return result;
    }

    /// <summary>
    /// 아군 리스트
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public List<CharacterBase> GetAlliesOf(CharacterBase user)
    {
        List<CharacterBase> result = new();

        if (user == null)
            return result;

        bool userIsPlayer = user.Controller != null;

        foreach (CharacterBase character in participants)
        {
            if (character == null)
                continue;

            if (character == user)
                continue;

            bool targetIsPlayer = character.Controller != null;

            if (userIsPlayer == targetIsPlayer)
            {
                result.Add(character);
            }
        }

        return result;
    }

    private bool IsAlive(CharacterBase character)
    {
        if (character == null)
            return false;

        HitpointModules hp = character.GetModule<HitpointModules>();
        SanityModule sanity = character.GetModule<SanityModule>();

        return (hp == null || !hp.IsEmpty) &&
               (sanity == null || !sanity.IsEmpty);
    }

    internal void EndCurrentTurn()
    {
        if (State == BattleTurnState.BattleEnd)
            return;

        EndTurn();
    }

    private bool IsPhysicalDamage(DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Hand_to_hand_combat:
            case DamageType.Long_range_combat:
                return true;

            default:
                return false;
        }
    }

    public void RequestAttack(
        CharacterBase attacker,
        CharacterBase defender,
        DamageStruct damageInfo,
        bool endTurnAfterResolve = false,
        Action onResolved = null)
    {
        if (attacker == null || defender == null)
            return;

        Debug.Log(
            $"공격 요청 / 공격자:{attacker.name} / 대상:{defender.name} / " +
            $"피해:{damageInfo.damageAmount} / 타입:{damageInfo.damageType} / 반격가능:{damageInfo.canCounter}"
        );

        pendingAttacker = attacker;
        pendingDefender = defender;
        pendingDamageInfo = damageInfo;
        endTurnAfterReaction = endTurnAfterResolve;
        pendingAttackResolved = onResolved;

        bool reusedReaction =
            TryReuseReactionChainChoice(
                attacker,
                defender,
                ref damageInfo);

        if (!reusedReaction)
        {
            ResolveMonsterReaction(attacker, defender, ref damageInfo);
        }

        pendingDamageInfo = damageInfo;

        if (!reusedReaction &&
            CanOpenReactionPopup(defender, damageInfo))
        {
            waitingReaction = true;
            State = BattleTurnState.WaitingReaction;

            OpenReactionPopup(
                defender,
                attacker,
                damageInfo
            );

            return;
        }

        int defenderHealthBefore = GetCurrentHealth(defender);

        ApplyDamageToTarget(
            defender,
            damageInfo
        );

        StopRepeatedEvadeAfterDamage(defender, defenderHealthBefore);

        if (State == BattleTurnState.BattleEnd)
        {
            pendingAttackResolved = null;
            return;
        }

        // 반격으로 공격자가 쓰러지면 남아 있는 연속 공격을 실행하지 않습니다.
        if (!IsAlive(attacker))
        {
            pendingAttackResolved = null;

            if (CurrentCharacter == attacker)
                EndTurn();

            return;
        }

        Action resolved = pendingAttackResolved;
        pendingAttackResolved = null;
        resolved?.Invoke();

        if (endTurnAfterResolve)
        {
            EndTurn();
        }
    }

    /// <summary>
    /// 공격받은 몬스터의 AI 프로필에서 대응 행동을 결정합니다.
    /// 프로필이 없으면 기존 지능 방어 판정을 사용합니다.
    /// </summary>
    private void ResolveMonsterReaction(
        CharacterBase attacker,
        CharacterBase defender,
        ref DamageStruct damageInfo)
    {
        if (defender == null ||
            defender.GetModule<MonsterAIModule>() == null)
        {
            return;
        }

        MonsterAIModule ai = defender.GetModule<MonsterAIModule>();

        if (ai == null ||
            !ai.TryChooseReaction(
                this,
                attacker,
                damageInfo.canCounter,
                out ActionType reactionType))
            return;

        damageInfo.reactionType = reactionType;
    }


    private bool CanOpenReactionPopup(CharacterBase defender, in DamageStruct damageInfo)
    {
        if (defender == null)
        {
            Debug.Log("대응 팝업 불가: defender null");
            return false;
        }

        // 공격 처리 전에 대응이 이미 결정된 경우 선택창을 다시 열지 않습니다.
        if (damageInfo.reactionType != ActionType.None)
            return false;

        // 플레이어만 대응 팝업 사용
        if (defender.Controller == null)
        {
            Debug.Log($"대응 팝업 불가: {defender.name}은 플레이어가 아님");
            return false;
        }

        // 물리 공격만 대응 가능
        if (!IsPhysicalDamage(damageInfo.damageType))
        {
            Debug.Log($"대응 팝업 불가: 물리 공격이 아님 / {damageInfo.damageType}");
            return false;
        }

        ReactionModule reaction = defender.GetModule<ReactionModule>();

        if (reaction == null)
        {
            Debug.Log($"대응 팝업 불가: {defender.name}에게 ReactionModule 없음");
            return false;
        }

        if (!reaction.CanUseAnyReaction())
        {
            Debug.Log($"대응 팝업 불가: {defender.name} 행동력 부족");
            return false;
        }

        return true;
    }

    private bool CanCounterAttack(CharacterBase defender, CharacterBase attacker, in DamageStruct damageInfo)
    {
        if (defender == null || attacker == null)
            return false;

        if (!damageInfo.canCounter)
            return false;

        ReactionModule reaction = defender.GetModule<ReactionModule>();

        if (reaction == null)
            return false;

        return reaction.CanUse(ActionType.Counterattack);
    }

    private void OpenReactionPopup(CharacterBase defender, CharacterBase attacker, in DamageStruct damageInfo)
    {
        if (reactionSelectUI == null)
            BindBattleUI();

        if (reactionSelectUI == null)
        {
            Debug.LogWarning("대응 팝업 실패: reactionSelectUI null");
            return;
        }

        bool canCounter = CanCounterAttack(defender, attacker, damageInfo);

        Debug.Log(
            $"대응 팝업 열기 / 대상:{defender.name} / 피해:{damageInfo.damageAmount} / 반격버튼:{canCounter}"
        );

        reactionSelectUI.Open(damageInfo.damageAmount, canCounter);
    }

    private void BindBattleUI()
    {
        UI_BattleScreen battleScreen =
            UIManager.GetUIM2(UIType.Battle) as UI_BattleScreen;

        if (battleScreen != null && battleScreen.HandUI != null)
            handUI = battleScreen.HandUI;

        UIBase actionPopupUI = UIManager.GetUIM2(UIType.ActionPopUp);

        if (actionPopupUI == null)
        {
            Debug.LogWarning("ActionPopUp UI를 찾을 수 없습니다.");
            return;
        }

        reactionSelectUI = actionPopupUI.GetComponentInChildren<UI_ReactionSelect>(true);

        if (reactionSelectUI == null)
        {
            Debug.LogWarning("ReactionSelectUI를 찾을 수 없습니다.");
            return;
        }

        reactionSelectUI.OnSelected -= ResolveReaction;
        reactionSelectUI.OnSelected += ResolveReaction;

        Debug.Log("ReactionSelectUI 연결 완료");
    }

    internal void ResolveReaction(ActionType actionType)
    {
        if (!waitingReaction)
        {
            Debug.LogWarning($"대응 처리 실패: 현재 대응 대기 상태가 아닙니다. / 선택:{actionType}");
            return;
        }

        waitingReaction = false;

        CharacterBase attacker = pendingAttacker;
        CharacterBase defender = pendingDefender;
        DamageStruct damageInfo = pendingDamageInfo;
        bool shouldEndTurn = endTurnAfterReaction;

        if (defender == null)
        {
            Debug.LogWarning("대응 처리 실패: defender null");
            State = BattleTurnState.WaitingAction;
            return;
        }

        ReactionModule reaction = defender.GetModule<ReactionModule>();

        if (reaction == null)
        {
            Debug.LogWarning($"{defender.name}: ReactionModule 없음. 대응 없이 피해 처리");
        }
        else
        {
            bool success =
                reaction.TryUse(
                    actionType,
                    damageInfo,
                    out damageInfo
                );

            if (!success)
            {
                Debug.LogWarning($"{defender.name}: 대응 실패 / 선택:{actionType}");
                damageInfo.reactionType = ActionType.None;
            }
            else
            {
                Debug.Log($"{defender.name}: 대응 적용 / 선택:{damageInfo.reactionType}");
            }
        }

        StoreReactionChainChoice(
            attacker,
            defender,
            damageInfo.reactionType);

        int defenderHealthBefore = GetCurrentHealth(defender);

        ApplyDamageToTarget(
            defender,
            damageInfo
        );

        StopRepeatedEvadeAfterDamage(defender, defenderHealthBefore);

        if (State == BattleTurnState.BattleEnd)
        {
            pendingAttackResolved = null;
            ClearPendingReaction();
            return;
        }

        State = BattleTurnState.WaitingAction;

        // 반격으로 공격자가 쓰러진 경우 콜백으로 다음 공격을 이어가지 않습니다.
        if (!IsAlive(attacker))
        {
            pendingAttackResolved = null;

            if (CurrentCharacter == attacker)
                EndTurn();

            return;
        }

        Action resolved = pendingAttackResolved;
        pendingAttackResolved = null;
        resolved?.Invoke();

        if (shouldEndTurn)
        {
            EndTurn();
        }
    }

    private void ClearPendingReaction()
    {
        waitingReaction = false;
        pendingAttacker = null;
        pendingDefender = null;
        endTurnAfterReaction = false;
        pendingAttackResolved = null;

        if (reactionSelectUI != null)
        {
            reactionSelectUI.Close();
        }
    }

    private bool TryReuseReactionChainChoice(
        CharacterBase attacker,
        CharacterBase defender,
        ref DamageStruct damageInfo)
    {
        if (!hasReactionChainChoice ||
            reactionChainAttacker != attacker ||
            reactionChainDefender != defender)
        {
            return false;
        }

        damageInfo.reactionType = reactionChainType;

        Debug.Log(
            $"연속 공격 대응 재사용: {defender.name} / {reactionChainType}");

        return true;
    }

    private void StoreReactionChainChoice(
        CharacterBase attacker,
        CharacterBase defender,
        ActionType reactionType)
    {
        reactionChainAttacker = attacker;
        reactionChainDefender = defender;
        reactionChainType = reactionType;
        hasReactionChainChoice = true;
    }

    private void ClearReactionChainChoice()
    {
        reactionChainAttacker = null;
        reactionChainDefender = null;
        reactionChainType = ActionType.None;
        hasReactionChainChoice = false;
    }

    private int GetCurrentHealth(CharacterBase character)
    {
        HitpointModules hitpoint =
            character != null
                ? character.GetModule<HitpointModules>()
                : null;

        return hitpoint != null ? hitpoint.Current : 0;
    }

    private void StopRepeatedEvadeAfterDamage(
        CharacterBase defender,
        int healthBefore)
    {
        if (!hasReactionChainChoice ||
            reactionChainDefender != defender ||
            reactionChainType != ActionType.Evade)
        {
            return;
        }

        if (GetCurrentHealth(defender) >= healthBefore)
            return;

        // 한 번 피해를 받으면 남은 연속 타격에는 회피 판정을 반복하지 않습니다.
        reactionChainType = ActionType.None;

        Debug.Log(
            $"{defender.name}: 회피 실패로 남은 연속 공격의 회피 종료");
    }

    private void ApplyDamageToTarget(CharacterBase defender, DamageStruct damageInfo)
    {
        if (defender == null)
            return;

        CombatModule combat = defender.GetModule<CombatModule>();

        if (combat == null)
        {
            Debug.LogWarning($"{defender.name}: CombatModule 없음. 피해 처리 불가");
            return;
        }

        HitpointModules hp = defender.GetModule<HitpointModules>();

        combat.OnHit(damageInfo);


        CheckBattleEnd();
    }

    private void CheckBattleEnd()
    {
        if (State == BattleTurnState.BattleEnd)
            return;

        HideDefeatedCharacters();

        bool playerAlive = HasAlivePlayer();

        bool monsterAlive = HasAliveMonster();

        if (!playerAlive)
        {
            EndBattle(false);
            return;
        }

        if (!monsterAlive)
        {
            EndBattle(true);
        }
    }

    private void HideDefeatedCharacters()
    {
        foreach (CharacterBase character in participants)
        {
            if (character == null)
                continue;

            if (IsAlive(character))
                continue;

            character.gameObject.SetActive(false);
        }
    }

    private void EndBattle(bool victory)
    {
        if (!isBattleActive)
            return;

        isBattleActive = false;
        State = BattleTurnState.BattleEnd;

        UnbindHitPointEvents();
        ClearPendingReaction();

        OnBattleEnded?.Invoke(victory);

        if (!victory)
        {
            FieldManager fieldManager =
                GameManager.Instance != null
                    ? GameManager.Instance.Field
                    : null;

            // 필드 세션에서 사망한 경우 필드의 확정 사망 엔딩이
            // 이미 처리되므로 일반 전투 게임 오버를 겹쳐 열지 않습니다.
            if (fieldManager == null || !fieldManager.HasEndedByDeath)
            {
                OpenGameOver();
            }

            return;
        }

        StartCoroutine(VictorySequence());
    }

    [SerializeField]
    private float victoryDelay = 1.0f;

    private IEnumerator VictorySequence()
    {
        // 마지막 공격 결과를 잠시 보여줌
        yield return new WaitForSeconds(victoryDelay);

        HideMonsters();

        // 몬스터가 사라진 화면을 잠깐 보여줌
        yield return new WaitForSeconds(0.5f);

        OpenReward();
    }

    private void HideMonsters()
    {
        foreach (CharacterBase character in participants)
        {
            if (character == null)
                continue;

            if (!IsMonster(character))
                continue;

            character.gameObject.SetActive(false);
        }
    }

    private void OpenGameOver()
    {
        UIManager.OpenUIM2(UIType.GameOver);
    }

    private void OpenReward()
    {
        UI_RewardWindow rewardUI = UIManager.OpenUIM2(UIType.Reward)
            as UI_RewardWindow;

        if (rewardUI == null)
        {
            Debug.LogWarning(
                "BattleManager: Reward UI를 찾지 못했습니다.");

            return;
        }

        //rewardUI.SetReward(/* 전투 보상 */);
    }
    private bool HasAlivePlayer()
    {
        foreach (CharacterBase character in participants)
        {
            if (character == null)
                continue;

            if (IsPlayer(character) && IsAlive(character))
                return true;
        }

        return false;
    }

    private bool HasAliveMonster()
    {
        foreach (CharacterBase character in participants)
        {
            if (character == null)
                continue;

            if (IsMonster(character) && IsAlive(character))
                return true;
        }

        return false;
    }

    private void UnbindHitPointEvents()
    {
        foreach (CharacterBase character in participants)
        {
            if (character == null)
                continue;

            HitpointModules hp =
                character.GetModule<HitpointModules>();

            if (hp != null)
            {
                hp.OnEmpty -= CheckBattleEnd;
            }

            SanityModule sanity = character.GetModule<SanityModule>();

            if (sanity != null)
                sanity.OnEmpty -= CheckBattleEnd;
        }
    }

    public static void ClaimBattleLog(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        OnBattleLog?.Invoke(message);
    }

    public static void ClaimBattleLogClear()
    {
        OnBattleLogClear?.Invoke();
    }

    public void WriteBattleLog(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        OnBattleLog?.Invoke(message);
    }

    private void ProcessHandTurnDurability(CharacterBase character)
    {
        if (character == null)
            return;

        DeckModule deck = character.GetModule<DeckModule>();

        if (deck == null)
            return;

        int removedCount = deck.ProcessHandTurnDurability();

        if (removedCount > 0)
        {
            RefreshHandUI(deck);
        }
    }

}
