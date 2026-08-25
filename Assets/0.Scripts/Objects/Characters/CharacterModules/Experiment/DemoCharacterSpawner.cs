using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 데모 캐릭터 생성 테스트
/// 프리셋 캐릭터를 실제 캐릭터로 생성한다.
/// </summary>
public class DemoCharacterSpawner : MonoBehaviour
{
    [SerializeField]
    private CharacterFactory factory;

    [Header("데모 캐릭터")]
    [SerializeField]
    private CharacterPresetData[] presets;

    [Header("생성 위치")]
    [SerializeField]
    private Transform[] spawnPoints;

    private readonly List<CharacterBase> spawnedCharacters = new();

    /// <summary>
    /// 현재 생성되어 있는 플레이어 캐릭터 목록이다.
    /// </summary>
    public IReadOnlyList<CharacterBase> SpawnedCharacters =>
        spawnedCharacters;

    /// <summary>
    /// 모든 데모 캐릭터 생성이 완료되었을 때 발생한다.
    /// </summary>
    public event Action<IReadOnlyList<CharacterBase>>
        OnCharactersSpawned;

    /// <summary>
    /// 게임 시작 시 데모 플레이어 캐릭터를 생성한다.
    /// </summary>
    private void Start()
    {
        SpawnDemoCharacters();
    }

    /// <summary>
    /// 등록된 프리셋을 순서대로 생성하고 생성 결과를 보관한다.
    /// </summary>
    private void SpawnDemoCharacters()
    {
        spawnedCharacters.Clear();

        if (factory == null)
        {
            Debug.LogWarning(
                "DemoCharacterSpawner: CharacterFactory가 없습니다.");

            return;
        }

        if (presets == null || presets.Length == 0)
        {
            Debug.LogWarning(
                "DemoCharacterSpawner: 생성할 프리셋이 없습니다.");

            return;
        }

        for (int i = 0; i < presets.Length; i++)
        {
            CharacterPresetData preset = presets[i];

            if (preset == null)
                continue;

            Vector3 spawnPosition = GetSpawnPosition(i);

            CharacterBase character =
                factory.CreatePlayerCharacter(
                    preset.ToBuildData(),
                    spawnPosition);

            if (character == null)
                continue;

            spawnedCharacters.Add(character);
        }

        OnCharactersSpawned?.Invoke(spawnedCharacters);

        Debug.Log(
            $"데모 플레이어 생성 완료: {spawnedCharacters.Count}명");
    }

    /// <summary>
    /// 캐릭터 순서에 대응하는 생성 위치를 반환한다.
    /// 지정된 위치가 없으면 원점 위치를 반환한다.
    /// </summary>
    /// <param name="index">생성할 캐릭터의 순서</param>
    /// <returns>캐릭터를 생성할 월드 위치</returns>
    private Vector3 GetSpawnPosition(int index)
    {
        if (spawnPoints == null ||
            index < 0 ||
            index >= spawnPoints.Length ||
            spawnPoints[index] == null)
        {
            return Vector3.zero;
        }

        return spawnPoints[index].position;
    }
}
