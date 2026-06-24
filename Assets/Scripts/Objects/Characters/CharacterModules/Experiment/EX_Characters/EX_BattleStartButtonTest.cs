using System.Collections.Generic;
using UnityEngine;

/// <summary>

/// </summary>
public class EX_BattleStartButtonTest : MonoBehaviour
{
    [SerializeField] private List<MonsterData> monsterDatas;
    [SerializeField] private MonsterSpawner monsterSpawner;

    private List<CharacterBase> FindCurrentPlayers()
    {
        List<CharacterBase> result = new();

        ControllerBase[] controllers =
        FindObjectsByType<ControllerBase>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        foreach (ControllerBase controller in controllers)
        {
            if (controller == null)
                continue;

            CharacterBase character = controller.Character;

            if (character == null)
                continue;

            if (!result.Contains(character))
            {
                result.Add(character);

                Debug.Log(
                $"플레이어 감지: {character.name} / " +
                $"Controller={controller.GetType().Name}");
            }
        }

        return result;

    }

    private int GetPlayerLevel(List<CharacterBase> players)
    {
        if (players == null || players.Count <= 0 || players[0] == null)
            return 1;

        LVModules lv =
            players[0].GetModule<LVModules>();

        if (lv == null)
            return 1;

        return lv.Level;
    }

    public void StartBattle()
    {
        if (monsterSpawner == null)
        {
            Debug.LogError("전투 시작 실패: MonsterSpawner 없음");
            return;
        }

        List<CharacterBase> participants = new();

        List<CharacterBase> currentPlayers = FindCurrentPlayers();

        if (currentPlayers.Count <= 0)
        {
            Debug.LogError("전투 시작 실패: 현재 빙의 중인 플레이어가 없습니다.");
            return;
        }

        foreach (CharacterBase player in currentPlayers)
        {
            participants.Add(player);
        }

        int playerLevel = GetPlayerLevel(currentPlayers);

        List<CharacterBase> spawnedMonsters =
            monsterSpawner.SpawnMonsters(monsterDatas, playerLevel);

        participants.AddRange(spawnedMonsters);

        Debug.Log($"전투 참가자 수: {participants.Count}");

        GameManager.Instance.Battle.StartBattle(participants);
    }
}