using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 현재 플레이어와 테스트 몬스터를 이용해 전투를 시작합니다.
/// 이 테스트 전투에서 승리하면 보상창 종료 후
/// 튜토리얼 필드를 시작하도록 예약합니다.
/// </summary>
public class EX_BattleStartButtonTest : MonoBehaviour
{
    [SerializeField]
    private List<MonsterData> monsterDatas;

    [SerializeField]
    private MonsterSpawner monsterSpawner;

    [SerializeField]
    private TutorialFieldFlowController tutorialFieldFlow;

    private List<CharacterBase> FindCurrentPlayers()
    {
        List<CharacterBase> result = new();

        ControllerBase[] controllers =
            FindObjectsByType<ControllerBase>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);

        foreach (ControllerBase controller in controllers)
        {
            if (controller == null)
                continue;

            CharacterBase character = controller.Character;

            if (character == null)
                continue;

            if (result.Contains(character))
                continue;

            result.Add(character);

            Debug.Log(
                $"플레이어 감지: {character.name} / " +
                $"Controller={controller.GetType().Name}");
        }

        return result;
    }

    private int GetPlayerLevel(
        List<CharacterBase> players)
    {
        if (players == null ||
            players.Count == 0 ||
            players[0] == null)
        {
            return 1;
        }

        LVModules levelModule =
            players[0].GetModule<LVModules>();

        return levelModule != null
            ? levelModule.Level
            : 1;
    }

    public void StartBattle()
    {
        if (monsterSpawner == null)
        {
            Debug.LogError(
                "전투 시작 실패: MonsterSpawner 없음");

            return;
        }

        if (GameManager.Instance == null ||
            GameManager.Instance.Battle == null)
        {
            Debug.LogError(
                "전투 시작 실패: BattleManager 없음");

            return;
        }

        List<CharacterBase> currentPlayers =
            FindCurrentPlayers();

        if (currentPlayers.Count == 0)
        {
            Debug.LogError(
                "전투 시작 실패: 현재 빙의 중인 플레이어가 없습니다.");

            return;
        }

        if (tutorialFieldFlow == null)
        {
            tutorialFieldFlow =
                FindFirstObjectByType<TutorialFieldFlowController>(
                    FindObjectsInactive.Include);
        }

        if (tutorialFieldFlow == null)
        {
            Debug.LogError(
                "전투 시작 실패: " +
                "TutorialFieldFlowController 없음");

            return;
        }

        List<CharacterBase> participants = new();

        foreach (CharacterBase player in currentPlayers)
        {
            if (player != null)
            {
                participants.Add(player);
            }
        }

        int playerLevel =
            GetPlayerLevel(currentPlayers);

        List<CharacterBase> spawnedMonsters =
            monsterSpawner.SpawnMonsters(
                monsterDatas,
                playerLevel);

        if (spawnedMonsters != null)
        {
            participants.AddRange(spawnedMonsters);
        }

        Debug.Log(
            $"전투 참가자 수: {participants.Count}");

        // 타이틀에서 바로 시작한 테스트 전투이므로
        // 승리 후 보상창을 닫으면 튜토리얼 필드를 시작합니다.
        if (!tutorialFieldFlow.gameObject.activeSelf)
        {
            tutorialFieldFlow.gameObject.SetActive(true);
        }

        if (!tutorialFieldFlow.isActiveAndEnabled)
        {
            Debug.LogError(
                "전투 시작 실패: " +
                "TutorialFieldFlowController가 활성 상태가 아닙니다.");

            return;
        }

        if (!tutorialFieldFlow.PrepareFieldStartAfterBattle(currentPlayers))
        {
            Debug.LogError(
                "전투 시작 실패: " +
                "필드로 전달할 플레이어가 없습니다.");

            return;
        }

        GameManager.Instance.Battle.StartBattle(
            participants);
    }
}