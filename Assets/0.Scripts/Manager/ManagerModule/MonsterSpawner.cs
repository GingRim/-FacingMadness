using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private Transform monsterParent;

    [Header("전투 플레이어 위치")]
    [SerializeField] private Transform[] playerSpawnPoints;

    [Header("전투 몬스터 위치")]
    [SerializeField] private Transform[] monsterSpawnPoints;

    public CharacterBase SpawnMonster(MonsterData data, int playerLevel, Vector3 position)
    {
        if (data == null)
        {
            Debug.LogError("몬스터 생성 실패: MonsterData 없음");
            return null;
        }

        if (data.monsterPrefab == null)
        {
            Debug.LogError($"몬스터 생성 실패: {data.monsterName} prefab 없음");
            return null;
        }

        CharacterBase monster = Instantiate(data.monsterPrefab, position, Quaternion.identity, monsterParent);

        if (monster == null)
        {
            Debug.LogError($"몬스터 생성 실패: {data.monsterName}");
            return null;
        }

        // 프리팹의 기본 이름과 아이콘 대신 MonsterData의 정보를 사용합니다.
        monster.SetDisplayName(data.monsterName);
        monster.SetIcon(data.Icon);

        if (monster.GetComponentInChildren<ActionPointModule>(true) == null)
        {
            monster.gameObject.AddComponent<ActionPointModule>();
        }

        if (monster.GetComponentInChildren<DamageResistanceModule>(true) == null)
        {
            monster.gameObject.AddComponent<DamageResistanceModule>();
        }

        // 중요: 몬스터는 Possessed를 안 하므로 여기서 직접 모듈 등록
        monster.AddAllModuleFromObject(monster.gameObject);

        int difficultyModifier = data.difficultyModifier;
        int monsterLevel = Mathf.Max(1, playerLevel + difficultyModifier);

        ApplyMonsterData(monster, data, monsterLevel);

        Debug.Log($"몬스터 생성: {monster.DisplayName} / LV {monsterLevel}");

        return monster;

    }

    public List<CharacterBase> SpawnMonsters(List<MonsterData> monsterDatas, int playerLevel)
    {
        List<CharacterBase> monsters = new();

        if (monsterDatas == null)
            return monsters;

        for (int i = 0; i < monsterDatas.Count; i++)
        {
            MonsterData data = monsterDatas[i];

            Transform spawnPoint = GetSpawnPoint(monsterSpawnPoints, i);

            Vector3 position = spawnPoint != null
                ? spawnPoint.position
                : new Vector3(i * 2f, 0f, 0f);

            CharacterBase monster =
                SpawnMonster(data, playerLevel, position);

            if (monster != null)
            {
                if (spawnPoint != null)
                {
                    monster.transform.SetPositionAndRotation(
                        spawnPoint.position,
                        spawnPoint.rotation);
                }

                monsters.Add(monster);
            }
        }

        return monsters;
    }

    /// <summary>
    /// 이미 생성된 플레이어를 전투 화면의 플레이어 스폰 지점에 배치합니다.
    /// 스폰 지점이 비어 있는 플레이어의 현재 위치는 유지합니다.
    /// </summary>
    public void PositionPlayers(IReadOnlyList<CharacterBase> players)
    {
        if (players == null)
            return;

        for (int i = 0; i < players.Count; i++)
        {
            CharacterBase player = players[i];
            Transform spawnPoint = GetSpawnPoint(playerSpawnPoints, i);

            if (player == null || spawnPoint == null)
                continue;

            player.transform.SetPositionAndRotation(
                spawnPoint.position,
                spawnPoint.rotation);
        }
    }

    private static Transform GetSpawnPoint(
        Transform[] spawnPoints,
        int index)
    {
        if (spawnPoints == null ||
            index < 0 ||
            index >= spawnPoints.Length)
        {
            return null;
        }

        return spawnPoints[index];
    }

    private void ApplyMonsterData(CharacterBase monster, MonsterData data, int monsterLevel)
    {
        ApplyLevel(monster, monsterLevel);
        ApplyStats(monster, data);
        ApplyArmor(monster, data);
        ApplyResistance(monster, data);
        ApplyAI(monster, data);
        RefreshDerivedValues(monster);
    }

    private void ApplyAI(CharacterBase monster, MonsterData data)
    {
        MonsterAIModule ai = monster.GetModule<MonsterAIModule>();

        if (ai != null)
        {
            ai.SetProfile(data.aiProfile);
        }
    }

    private void ApplyLevel(CharacterBase monster, int monsterLevel)
    {
        LVModules lv = monster.GetModule<LVModules>();

        if (lv == null)
        {
            Debug.LogWarning($"{monster.name}: LVModules 없음");
            return;
        }

        lv.SetLevel(monsterLevel);
    }

    private void ApplyStats(CharacterBase monster, MonsterData data)
    {
        StatModules stat = monster.GetModule<StatModules>();

        if (stat == null)
        {
            Debug.LogWarning($"{monster.name}: StatModules 없음");
            return;
        }

        stat.SetStat(StatType.Strength, data.strength);
        stat.SetStat(StatType.Agility, data.agility);
        stat.SetStat(StatType.Health, data.health);
        stat.SetStat(StatType.Intelligence, data.intelligence);
        stat.SetStat(StatType.Will, data.will);
    }

    private void ApplyArmor(CharacterBase monster, MonsterData data)
    {
        ArmorModule armor = monster.GetModule<ArmorModule>();

        if (armor == null)
            return;

        armor.SetBaseArmor(data.baseArmor);
    }

    private void ApplyResistance(CharacterBase monster, MonsterData data)
    {
        DamageResistanceModule resistance =
            monster.GetModule<DamageResistanceModule>();

        if (resistance == null || data == null)
            return;

        resistance.Configure(
            data.bluntResistance,
            data.slashResistance,
            data.pierceResistance,
            data.fireResistance);
    }

    private void RefreshDerivedValues(CharacterBase monster)
    {
        if (monster == null)
            return;

        DerivedStatModule derived = monster.GetModule<DerivedStatModule>();

        if (derived == null)
        {
            Debug.LogWarning($"{monster.name}: DerivedStatModule 없음");
            return;
        }

        HitpointModules hp = monster.GetModule<HitpointModules>();

        if (hp != null)
        {
            hp.InitializeHP(derived.GetMaxHP());

            Debug.Log($"{monster.name} HP 초기화: {hp.Current}/{hp.Max}");
        }

        SanityModule sanity = monster.GetModule<SanityModule>();

        if (sanity != null)
        {
            sanity.SetMaxSanity(derived.GetMaxSanity());
            sanity.FillSanity();

            Debug.Log($"{monster.name} 정신력 초기화: {sanity.CurrentSanity}/{sanity.MaxSanity}");
        }
    }
}
