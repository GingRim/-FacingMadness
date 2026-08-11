using System.Collections.Generic;
using UnityEngine;
/// <summary>

/// UIManager가 생성하는 필드 화면의 최상위 스크립트
/// </summary>
public class UI_FieldScreen : UI_ScreenBase
{
    [Header("필드 생성 위치")]
    [SerializeField]
    private Transform fieldCore;

    [Header("필드 UI")]
    [SerializeField]
    private UI_MissionSelect missionSelect;

    [SerializeField]
    private UI_FieldEvent fieldEventUI;

    [SerializeField]
    private UI_FieldCharacterMarkers characterMarkers;

    private MissionManager missionManager;
    private FieldManager fieldManager;

    private readonly List<CharacterBase> fieldPlayers = new();

    public Transform FieldCore => fieldCore;

    public UI_MissionSelect MissionSelect => missionSelect;

    public UI_FieldEvent FieldEventUI => fieldEventUI;

    private void Awake()
    {
        if (missionSelect == null)
        {
            missionSelect = GetComponentInChildren<UI_MissionSelect>(true);
        }

        if (fieldEventUI == null)
        {
            fieldEventUI = GetComponentInChildren<UI_FieldEvent>(true);
        }

        if (characterMarkers == null)
        {
            characterMarkers = GetComponentInChildren<UI_FieldCharacterMarkers>(true);
        }
    }

    private void OnDestroy()
    {
        Unbind();
    }

    public void Bind(MissionManager newMissionManager, FieldManager newFieldManager, List<CharacterBase> players)
    {
        Unbind();

        missionManager = newMissionManager;
        fieldManager = newFieldManager;

        fieldPlayers.Clear();

        if (players != null)
        {
            foreach (CharacterBase player in players)
            {
                if (player != null)
                {
                    fieldPlayers.Add(player);
                }
            }
        }

        if (missionSelect != null)
        {
            missionSelect.OnMissionConfirmed -= HandleMissionConfirmed;

            missionSelect.OnMissionConfirmed += HandleMissionConfirmed;
        }

        if (fieldManager != null)
        {
            fieldManager.SetFieldCore(fieldCore);

            fieldManager.OnStartingNodeConfirmed -= HandleStartingNodeConfirmed;

            fieldManager.OnStartingNodeConfirmed += HandleStartingNodeConfirmed;
        }

        if (characterMarkers != null)
        {
            characterMarkers.Bind(fieldManager);
        }

    }

    private void Unbind()
    {
        if (missionSelect != null)
        {
            missionSelect.OnMissionConfirmed -= HandleMissionConfirmed;
        }

        if (fieldManager != null)
        {
            fieldManager.OnStartingNodeConfirmed -= HandleStartingNodeConfirmed;
        }

        if (characterMarkers != null)
        {
            characterMarkers.Unbind();
        }

        missionManager = null;
        fieldManager = null;

        fieldPlayers.Clear();
    }

    public void OpenMissionSelect(MissionManager missionManager)
    {
        if (missionSelect == null || missionManager == null)
        {   
            Debug.LogWarning("UI_FieldScreen: " + "MissionManager가 연결되지 않았습니다.");

            return;
        }
 
        missionSelect.Open(missionManager);
    }

    private void HandleMissionConfirmed(FieldMissionData mission)
    {
        if (fieldManager == null || mission == null)
        {
            return;
        }

        bool loaded = fieldManager.LoadMissionField(mission);

        if (!loaded)
            return;

        // 고정 시작 노드가 있다면 바로 시작
        if (fieldManager.HasStartingNode)
        {
            StartLoadedField();
            return;
        }

        // 시작 후보가 여러 개라면
        // 노드 클릭을 기다림
        Debug.Log("시작할 노드를 선택하십시오.");

    }

    private void HandleStartingNodeConfirmed(FieldNode selectedNode)
    {
        StartLoadedField();
    }

    private void StartLoadedField()
    {
        if (fieldManager == null)
            return;

        if (!fieldManager.HasStartingNode)
        {
            Debug.LogWarning("시작 노드가 정해지지 않았습니다.");

            return;
        }

        if (fieldPlayers.Count == 0)
        {
            Debug.LogWarning("필드 참가자가 없습니다.");

            return;
        }

        fieldManager.StartField(fieldPlayers);
    }

    public void BindFieldManager(FieldManager manager)
    {
        fieldManager = manager;

        if (fieldManager != null)
        {
            fieldManager.SetFieldCore(fieldCore);
        }
    }

}
