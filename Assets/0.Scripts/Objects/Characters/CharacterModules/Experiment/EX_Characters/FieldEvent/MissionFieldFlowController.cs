using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 선택된 미션의 필드 오브젝트를 불러오고
/// 생성된 플레이어들을 필드에 참가시킨다.
/// </summary>
public class MissionFieldFlowController : MonoBehaviour
{
    [Header("진행 관리")]
    [SerializeField]
    private ScenarioProgressController progressController;

    [SerializeField]
    private FieldManager fieldManager;

    [Header("플레이어")]
    [SerializeField]
    private DemoCharacterSpawner characterSpawner;

    [Header("필드 생성 위치")]
    [SerializeField]
    private Transform fieldCore;

    [Header("필드 화면")]
    [SerializeField]
    private GameObject fieldScreenRoot;

    private readonly List<CharacterBase> pendingPlayers = new();

    /// <summary>
    /// 미션 선택과 시작 노드 확정 이벤트를 등록한다.
    /// </summary>
    private void OnEnable()
    {
        if (progressController != null)
        {
            progressController.OnMissionConfirmed -=
                HandleMissionConfirmed;

            progressController.OnMissionConfirmed +=
                HandleMissionConfirmed;
        }

        if (fieldManager != null)
        {
            fieldManager.OnStartingNodeConfirmed -=
                HandleStartingNodeConfirmed;

            fieldManager.OnStartingNodeConfirmed +=
                HandleStartingNodeConfirmed;
        }
    }

    /// <summary>
    /// 등록했던 미션 및 시작 노드 이벤트를 해제한다.
    /// </summary>
    private void OnDisable()
    {
        if (progressController != null)
        {
            progressController.OnMissionConfirmed -=
                HandleMissionConfirmed;
        }

        if (fieldManager != null)
        {
            fieldManager.OnStartingNodeConfirmed -=
                HandleStartingNodeConfirmed;
        }

        pendingPlayers.Clear();
    }

    /// <summary>
    /// 플레이어가 미션을 선택하면 해당 미션의 필드를 불러온다.
    /// </summary>
    /// <param name="mission">플레이어가 선택한 미션</param>
    private void HandleMissionConfirmed(FieldMissionData mission)
    {
        if (mission == null)
            return;

        if (fieldManager == null)
        {
            Debug.LogWarning(
                "MissionFieldFlowController: FieldManager가 없습니다.");

            return;
        }

        if (fieldCore == null)
        {
            Debug.LogWarning(
                "MissionFieldFlowController: FieldCore가 없습니다.");

            return;
        }

        if (!CollectPlayers())
        {
            Debug.LogWarning(
                "MissionFieldFlowController: 필드에 참가할 플레이어가 없습니다.");

            return;
        }

        fieldManager.SetFieldCore(fieldCore);

        bool loaded = fieldManager.LoadMissionField(mission);

        if (!loaded)
        {
            pendingPlayers.Clear();
            return;
        }

        if (fieldScreenRoot != null)
        {
            fieldScreenRoot.SetActive(true);
        }

        if (fieldManager.HasStartingNode)
        {
            StartLoadedField();
            return;
        }

        if (!fieldManager.IsSelectingStartingNode)
        {
            Debug.LogWarning(
                "MissionFieldFlowController: 사용할 수 있는 시작 노드가 없습니다.");

            pendingPlayers.Clear();
        }

        // 시작 노드 후보가 여러 개라면
        // OnStartingNodeConfirmed가 호출될 때까지 기다린다.
    }

    /// <summary>
    /// 생성기에서 현재 생성된 플레이어 목록을 가져온다.
    /// </summary>
    /// <returns>한 명 이상의 플레이어를 가져왔으면 true</returns>
    private bool CollectPlayers()
    {
        pendingPlayers.Clear();

        if (characterSpawner == null)
        {
            Debug.LogWarning(
                "MissionFieldFlowController: DemoCharacterSpawner가 없습니다.");

            return false;
        }

        IReadOnlyList<CharacterBase> spawnedCharacters =
            characterSpawner.SpawnedCharacters;

        if (spawnedCharacters == null)
            return false;

        foreach (CharacterBase character in spawnedCharacters)
        {
            if (character == null)
                continue;

            if (pendingPlayers.Contains(character))
                continue;

            pendingPlayers.Add(character);
        }

        return pendingPlayers.Count > 0;
    }

    /// <summary>
    /// 여러 시작 노드 중 하나가 확정되면 대기 중인 필드를 시작한다.
    /// </summary>
    /// <param name="startingNode">선택된 시작 노드</param>
    private void HandleStartingNodeConfirmed(FieldNode startingNode)
    {
        if (startingNode == null)
            return;

        if (pendingPlayers.Count == 0)
            return;

        StartLoadedField();
    }

    /// <summary>
    /// 불러온 필드에 대기 중인 플레이어들을 참가시킨다.
    /// </summary>
    private void StartLoadedField()
    {
        if (fieldManager == null ||
            !fieldManager.HasStartingNode ||
            pendingPlayers.Count == 0)
        {
            return;
        }

        List<CharacterBase> playersToStart =
            new List<CharacterBase>(pendingPlayers);

        pendingPlayers.Clear();

        fieldManager.StartField(playersToStart);

        Debug.Log(
            $"필드 시작: {playersToStart.Count}명 참가");
    }
}