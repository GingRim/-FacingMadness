using UnityEngine;


/// <summary>
/// 데모 캐릭터 생성 테스트
/// 프리셋 캐릭터를 실제 캐릭터로 생성한다.
/// </summary>
public class DemoCharacterSpawner : MonoBehaviour
{
    [SerializeField] private CharacterFactory factory;

    [Header("데모 캐릭터")]
    [SerializeField] private CharacterPresetData[] presets;
    
    [Header("생성 위치")]
    [SerializeField] private Transform[] spawnPoints;

    private void Start()
    {
        SpawnDemoCharacters();
    }

    private void SpawnDemoCharacters()
    {

        if (factory == null)
        {
            return;
        }

        for (int i = 0; i < presets.Length; i++)
        {

            if (presets[i] == null)
                continue;
            
            Vector3 position = Vector3.zero;

            if (spawnPoints != null && i < spawnPoints.Length && spawnPoints[i] != null)
                position = spawnPoints[i].position;

            factory.CreatePlayerCharacter(
                presets[i].ToBuildData(),
                position
            );
        }
    }

}
