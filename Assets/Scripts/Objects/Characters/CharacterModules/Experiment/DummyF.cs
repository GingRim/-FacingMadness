using UnityEngine;

public class DummyF : MonoBehaviour
{
    [SerializeField] private DummyS Dummy;

    [Header("데모 캐릭터")]
    [SerializeField] private CharacterPresetData[] presets;

    [Header("생성 위치")]
    [SerializeField] private Transform[] spawnPoints;

    private void Start()
    {
        SpawnDemoDummy();
    }


    private void SpawnDemoDummy()
    {

        if (Dummy == null)
        {
            return;
        }

        for (int i = 0; i < presets.Length; i++)
        {
            Debug.Log($"스폰 루프 실행: {i} / {presets[i].characterName}");

            if (presets[i] == null)
                continue;

            Vector3 position = Vector3.zero;

            if (spawnPoints != null && i < spawnPoints.Length && spawnPoints[i] != null)
                position = spawnPoints[i].position;

            Dummy.CreatePlayerCharacter(
                presets[i].ToBuildData(),
                position
            );
        }
    }

}