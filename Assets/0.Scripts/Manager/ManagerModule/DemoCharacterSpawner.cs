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
    public IReadOnlyList<CharacterBase> SpawnedCharacters =>
        spawnedCharacters;

    /// <summary>
    /// 모든 데모 캐릭터 생성이 완료되었을 때 발생한다.
    /// </summary>
    public event Action<IReadOnlyList<CharacterBase>>
        OnCharactersSpawned;

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
        if (selectedData == null)
        {
            Debug.LogWarning(
                "DemoCharacterSpawner: 생성 데이터가 없습니다.");
            return null;
        }

        ResolveFactory();

        if (factory == null)
        {
            Debug.LogWarning(
                "DemoCharacterSpawner: CharacterFactory를 찾지 못했습니다.");
            return null;
        }

        RemoveMissingCharacters();

        if (spawnedCharacters.Count > 0)
        {
            Debug.LogWarning(
                "이미 생성된 플레이어가 있어 중복 생성하지 않습니다.");
            return spawnedCharacters[0];
        }

        Transform spawnPoint = GetSpawnPoint(0);

        CharacterBase character =
            factory.CreatePlayerCharacter(
                selectedData,
                spawnPoint != null
                    ? spawnPoint.position
                    : Vector3.zero);

        if (character == null)
            return null;

        // 생성 과정의 다른 초기화가 Transform을 변경하더라도
        // 최종 생성 위치는 인스펙터에서 지정한 스폰 지점으로 확정합니다.
        if (spawnPoint != null)
        {
            character.transform.SetPositionAndRotation(
                spawnPoint.position,
                spawnPoint.rotation);
        }

        spawnedCharacters.Add(character);
        OnCharactersSpawned?.Invoke(spawnedCharacters);

        Debug.Log(
            $"생성 화면 플레이어 생성 완료: {character.DisplayName}");

        return character;
    }

    /// <summary>
    /// 인스펙터 연결이 비어 있으면 같은 오브젝트와 현재 장면에서
    /// CharacterFactory를 찾아 연결합니다.
    /// </summary>
    private void ResolveFactory()
    {
        if (factory != null)
            return;

        factory = GetComponent<CharacterFactory>();

        if (factory != null)
            return;

        factory = FindFirstObjectByType<CharacterFactory>(
            FindObjectsInactive.Include);
    }

    private void RemoveMissingCharacters()
    {
        for (int i = spawnedCharacters.Count - 1; i >= 0; i--)
        {
            if (spawnedCharacters[i] == null)
                spawnedCharacters.RemoveAt(i);
        }
    }

    private Transform GetSpawnPoint(int index)
    {
        if (spawnPoints == null ||
            index < 0 ||
            index >= spawnPoints.Length)
        {
            return null;
        }

        return spawnPoints[index];
    }

    /// <summary>
    /// 세션 종료 뒤 생성 캐릭터를 제거해 새 게임에서 다시 생성할 수 있게 합니다.
    /// </summary>
    public void ResetSpawnedCharacters()
    {
        for (int i = spawnedCharacters.Count - 1; i >= 0; i--)
        {
            CharacterBase character = spawnedCharacters[i];

            if (character != null)
                Destroy(character.gameObject);
        }

        spawnedCharacters.Clear();
    }
}
