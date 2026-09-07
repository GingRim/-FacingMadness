using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 필드 이벤트 결과로 전투를 시작합니다.
/// 현재 필드 참가자는 유지하고 지정된 몬스터를 생성합니다.
/// 화면 전환은 TutorialFieldFlowController가 담당합니다.
/// </summary>
[CreateAssetMenu(fileName = "NewFieldBattleStartEffect", menuName = "Field/Event Effect/Battle Start")]
public class FieldBattleStartEffect : FieldEventEffect
{
    [Header("출현 몬스터")]
    [SerializeField]
    private List<MonsterData> monsterDatas = new();

    public override void Execute(FieldEventContext context)
    {
        if (context == null || context.FieldManager == null)
        {
            Debug.LogWarning(
                "필드 전투 시작 실패: " +
                "FieldEventContext 또는 FieldManager가 없습니다.");

            return;
        }

        FieldManager fieldManager = context.FieldManager;

        if (!fieldManager.IsFieldActive)
        {
            Debug.LogWarning(
                "필드 전투 시작 실패: " +
                "현재 진행 중인 필드가 없습니다.");

            return;
        }

        if (monsterDatas == null || monsterDatas.Count == 0)
        {
            Debug.LogWarning(
                "필드 전투 시작 실패: " +
                "출현 몬스터가 설정되지 않았습니다.");

            return;
        }

        BattleManager battleManager =
            GameManager.Instance != null
                ? GameManager.Instance.Battle
                : null;

        if (battleManager == null)
        {
            Debug.LogWarning(
                "필드 전투 시작 실패: " +
                "BattleManager가 없습니다.");

            return;
        }

        MonsterSpawner monsterSpawner =
            FindFirstObjectByType<MonsterSpawner>(
                FindObjectsInactive.Include);

        if (monsterSpawner == null)
        {
            Debug.LogWarning(
                "필드 전투 시작 실패: " +
                "MonsterSpawner가 없습니다.");

            return;
        }

        List<CharacterBase> battleParticipants =
            new();

        foreach (CharacterBase player in
                 fieldManager.Participants)
        {
            if (player != null &&
                !battleParticipants.Contains(player))
            {
                battleParticipants.Add(player);
            }
        }

        if (battleParticipants.Count == 0)
        {
            Debug.LogWarning(
                "필드 전투 시작 실패: " +
                "전투에 참가할 플레이어가 없습니다.");

            return;
        }

        CharacterBase levelReference =
            context.Character != null
                ? context.Character
                : battleParticipants[0];

        int playerLevel =
            GetPlayerLevel(levelReference);

        List<CharacterBase> monsters =
            monsterSpawner.SpawnMonsters(
                monsterDatas,
                playerLevel);

        if (monsters == null ||
            monsters.Count == 0)
        {
            Debug.LogWarning(
                "필드 전투 시작 실패: " +
                "생성된 몬스터가 없습니다.");

            return;
        }

        battleParticipants.AddRange(monsters);

        context.AddResultMessage(
            $"전투 발생: 몬스터 {monsters.Count}마리");

        Debug.Log(
            $"필드 이벤트 전투 시작: " +
            $"플레이어 " +
            $"{battleParticipants.Count - monsters.Count}명 / " +
            $"몬스터 {monsters.Count}명");

        battleManager.StartBattle(
            battleParticipants);
    }

    private int GetPlayerLevel(
        CharacterBase player)
    {
        if (player == null)
            return 1;

        LVModules levelModule =
            player.GetModule<LVModules>();

        return levelModule != null
            ? levelModule.Level
            : 1;
    }
}