using System.Collections.Generic;
using UnityEngine;

public class UI_TurnOrder : MonoBehaviour
{
    [Header("Marker Pool")]
    [SerializeField] private Transform core;
    [SerializeField] private UI_TurnOrderMarker markerTemplate;

    [SerializeField, Range(1, 10)]
    private int maxMarkerCount = 10;

    private readonly List<UI_TurnOrderMarker>
        markerPool = new();

    private void Awake()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        if (markerPool.Count > 0)
            return;

        if (core == null || markerTemplate == null)
        {
            Debug.LogWarning(
                "UI_TurnOrder: Core 또는 Marker Template이 없습니다.");
            return;
        }

        markerTemplate.gameObject.SetActive(false);

        for (int i = 0; i < maxMarkerCount; i++)
        {
            UI_TurnOrderMarker marker =
                Instantiate(markerTemplate, core);

            marker.name = $"TurnMarker_{i}";
            marker.Clear();

            markerPool.Add(marker);
        }
    }

    public void Refresh(
        IReadOnlyList<CharacterBase> orderedCharacters)
    {
        Clear();

        if (orderedCharacters == null)
            return;

        int displayCount =
            Mathf.Min(
                orderedCharacters.Count,
                markerPool.Count);

        for (int i = 0; i < displayCount; i++)
        {
            CharacterBase character =
                orderedCharacters[i];

            if (character == null)
                continue;

            int initiative =
                GetInitiative(character);

            Sprite portrait =
                GetPortrait(character);

            markerPool[i].SetMarker(
                character,
                portrait,
                initiative);
        }

        if (orderedCharacters.Count > markerPool.Count)
        {
            Debug.LogWarning(
                $"턴 순서 표시 인원이 최대치 " +
                $"{markerPool.Count}명을 초과했습니다.");
        }
    }

    public void Clear()
    {
        foreach (UI_TurnOrderMarker marker in markerPool)
        {
            marker.Clear();
        }
    }

    private int GetInitiative(CharacterBase character)
    {
        // 임시 부분:
        // 최종적으로는 BattleManager가 계산한 값을 받아야 함
        return 0;
    }

    private Sprite GetPortrait(CharacterBase character)
    {
        // 캐릭터 이미지 보관 위치 확인 후 연결
        return null;
    }
}
