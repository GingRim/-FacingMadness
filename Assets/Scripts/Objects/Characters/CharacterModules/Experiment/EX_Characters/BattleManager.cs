using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class BattleManager : ManagerBase
{
    private readonly List<CharacterBase> participants = new();
    private readonly List<CharacterBase> turnOrder = new();

    private int round;
    private int currentTurnIndex;
    private UI_Hand handUI;


    public CharacterBase CurrentCharacter { get; private set; }
    public BattleTurnState State { get; private set; }

    protected override IEnumerator OnConnected(GameManager newManager)
    {
        State = BattleTurnState.None;
        round = 0;
        currentTurnIndex = 0;

        handUI = Object.FindFirstObjectByType<UI_Hand>(FindObjectsInactive.Include);

        if (handUI == null)
        {
            Debug.LogWarning("BattleManager: UI_Hand를 찾지 못했습니다.");
        }


        yield return null;
    }

    protected override void OnDisconnected()
    {
        participants.Clear();
        turnOrder.Clear();

        CurrentCharacter = null;
        State = BattleTurnState.None;
    }

    private void OnRoundStart(CharacterBase character)
    {
        if (!IsPlayer(character))
            return;

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
        Debug.Log($"OnTurnStart 호출: {character.name}");

        if (!IsPlayer(character))
        {
            return;
        }


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
        participants.Clear();
        turnOrder.Clear();

        if (characters != null)
        {
            foreach (CharacterBase character in characters)
            {
                if (character == null)
                    continue;

                character.AddAllModuleFromObject(character.gameObject);

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

        StartRound();
    }

    private void StartRound()
    {
        round++;

        State = BattleTurnState.RoundStart;

        Debug.Log($"라운드 시작: {round}");

        foreach (CharacterBase character in participants)
        {
            if (character == null)
                continue;

            OnRoundStart(character);
        }

        BuildTurnOrder();

        currentTurnIndex = 0;

        StartTurn();
    }

    private void StartTurn()
    {
        if (turnOrder.Count <= 0)
        {
            EndRound();
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
            EndTurn();
            return;
        }


        State = BattleTurnState.TurnStart;

        Debug.Log($"턴 시작: {CurrentCharacter.name}");

        OnTurnStart(CurrentCharacter);

        Debug.Log(
            $"턴 시작 대상: {CurrentCharacter.name} / " +
            $"Controller={(CurrentCharacter.Controller != null ? CurrentCharacter.Controller.GetType().Name : "null")}"
        );

        // 몬스터 턴이면 AI 실행
        if (IsMonster(CurrentCharacter))
        {
            MonsterAIModule ai =
                CurrentCharacter.GetModule<MonsterAIModule>();

            if (ai == null)
            {
                Debug.LogWarning($"{CurrentCharacter.name}: MonsterAIModule 없음. 턴 종료");
                EndTurn();
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

        ControllerBase controller = character.GetComponent<ControllerBase>();

        return controller != null;
    }

    private bool IsMonster(CharacterBase character)
    {
        return character != null && character.Controller == null;
    }

    private void DrawCard(CharacterBase character)
    {
        StatusEffectModule status = character.GetModule<StatusEffectModule>();

        if (status != null && status.ConsumeDrawBlock())
        {
            Debug.Log($"{character.name}: 드로우 제한으로 라운드 시작 드로우 취소");
            return;
        }

        DeckModule deck = character.GetModule<DeckModule>();

        if (deck == null)
        {
            Debug.LogWarning($"{character.name}: DeckModule 없음");
            return;
        }

        CardData drawCard = deck.Draw();

        if (drawCard == null)
        {
            Debug.LogWarning($"{character.name}: 드로우 실패");
            return;
        }

        Debug.Log($"{character.name} 드로우: {drawCard.cardName}");
    }

    private void ResetCost(CharacterBase character)
    {
        CostModule cost = character.GetModule<CostModule>();

        if (cost == null)
        {
            Debug.LogWarning($"{character.name}: CostModule 없음");
            return;
        }

        cost.RefillAll();

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
        DerivedStatModule derived =
            character.GetModule<DerivedStatModule>();

        if (derived == null)
        {
            Debug.LogWarning($"{character.name}: DerivedStatModule 없음");
            return;
        }

        DeckModule deck =
            character.GetModule<DeckModule>();

        if (deck == null)
        {
            Debug.LogWarning($"{character.name}: DeckModule 없음");
            return;
        }

        StatusEffectModule status =
            character.GetModule<StatusEffectModule>();

        if (status != null && status.ConsumeDrawBlock())
        {
            Debug.Log($"{character.name}: 드로우 제한으로 턴 시작 드로우 취소");
            return;
        }

        int drawCount = 1 + derived.GetDrawBonus();


        for (int i = 0; i < drawCount; i++)
        {
            CardData drawCard = deck.Draw();

            if (drawCard == null)
            {
                break;
            }

        }

        RefreshHandUI(deck);

    }

    private void RefreshHandUI(DeckModule deck)
    {
        if (deck == null)
            return;

        if (handUI == null)
        {
            handUI = Object.FindFirstObjectByType<UI_Hand>(FindObjectsInactive.Include);
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

        ControllerBase controller =
            character.GetComponent<ControllerBase>();

        if (controller != null && character.Controller == null)
        {
            controller.Possess(character);

            Debug.Log($"{character.name}: Controller Possess 실행");
        }

        Debug.Log(
            $"전투 참가자 준비: {character.name} / " +
            $"Controller={(character.Controller != null ? character.Controller.GetType().Name : "null")}");
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

        if (hp == null)
            return true;

        return !hp.IsEmpty;
    }

    internal void EndCurrentTurn()
    {
        if (State == BattleTurnState.BattleEnd)
            return;

        EndTurn();
    }
}
