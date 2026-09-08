using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 캐릭터 생성 화면과 CharacterFactory를 연결하고
/// 생성된 플레이어 목록을 필드 흐름에 제공합니다.
/// </summary>
public class DemoCharacterSpawner : MonoBehaviour
{
    [SerializeField]
    private CharacterFactory factory;

    [Header("생성 위치")]
    [SerializeField]
    private Transform[] spawnPoints;

    private readonly List<CharacterBase> spawnedCharacters = new();

    /// <summary>
    /// 현재 생성되어 있는 플레이어 캐릭터 목록이다.
    /// </summary>
    public IReadOnlyList<CharacterBase> SpawnedCharacters => spawnedCharacters;

    /// <summary>
    /// 모든 데모 캐릭터 생성이 완료되었을 때 발생한다.
    /// </summary>
    public event Action<IReadOnlyList<CharacterBase>> OnCharactersSpawned;

    /// <summary>
    /// 캐릭터 생성 화면을 통해 실제 플레이어가 준비되었는지 확인합니다.
    /// 더 이상 EX 프리셋을 자동 생성하지 않습니다.
    /// </summary>
    public bool EnsureCharactersSpawned()
    {
        return spawnedCharacters.Count > 0;
    }

    /// <summary>
    /// 캐릭터 생성 화면에서 확정한 데이터를 변경하지 않고
    /// CharacterFactory에 전달하여 플레이어 한 명을 생성합니다.
    /// </summary>
    public CharacterBase SpawnPlayerCharacter(CharacterBuildData selectedData)
    {
        if (selectedData == null || factory == null)
        {
            Debug.LogWarning(
                "DemoCharacterSpawner: 생성 데이터 또는 CharacterFactory가 없습니다.");
            return null;
        }

        RemoveMissingCharacters();

        if (spawnedCharacters.Count > 0)
        {
            Debug.LogWarning(
                "이미 생성된 플레이어가 있어 중복 생성하지 않습니다.");
            return spawnedCharacters[0];
        }

        CharacterBase character =
            factory.CreatePlayerCharacter(
                selectedData,
                GetSpawnPosition(0));

        if (character == null)
            return null;

        spawnedCharacters.Add(character);
        OnCharactersSpawned?.Invoke(spawnedCharacters);

        Debug.Log(
            $"생성 화면 플레이어 생성 완료: {character.DisplayName}");

        return character;
    }

    private void RemoveMissingCharacters()
    {
        for (int i = spawnedCharacters.Count - 1; i >= 0; i--)
        {
            if (spawnedCharacters[i] == null)
                spawnedCharacters.RemoveAt(i);
        }
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
