using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private Transform monsterParent;

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

        monster.AddAllModuleFromObject(monster.gameObject);

        // 중요: 몬스터는 Possessed를 안 하므로 여기서 직접 모듈 등록
        monster.AddAllModuleFromObject(monster.gameObject);

        int difficultyModifier = 0;
        int monsterLevel = Mathf.Max(1, playerLevel + difficultyModifier);

        ApplyMonsterData(monster, data, monsterLevel);

        Debug.Log($"몬스터 생성: {monster.name} / LV {monsterLevel}");

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

            Vector3 position = new Vector3(i * 2f, 0f, 0f);

            CharacterBase monster =
                SpawnMonster(data, playerLevel, position);

            if (monster != null)
            {
                monsters.Add(monster);
            }
        }

        return monsters;
    }

    private void ApplyMonsterData(CharacterBase monster, MonsterData data, int monsterLevel)
    {
        ApplyLevel(monster, monsterLevel);
        ApplyStats(monster, data);
        ApplyArmor(monster, data);
        RefreshDerivedValues(monster);
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
