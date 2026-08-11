using System.Collections.Generic;
using UnityEngine;

public class UI_FieldCharacterMarkers : MonoBehaviour
{
    [SerializeField]
    private UI_FieldCharacterMarker markerTemplate;

    private FieldManager fieldManager;

    private readonly List<FieldNode> registeredNodes = new();

    private readonly Dictionary<CharacterBase, UI_FieldCharacterMarker> markers = new();

    private void Awake()
    {
        if (markerTemplate != null)
        {
            markerTemplate.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        Unbind();
    }

    public void Bind(FieldManager newFieldManager)
    {
        Unbind();

        fieldManager = newFieldManager;

        if (fieldManager == null)
            return;

        fieldManager.OnMissionFieldLoaded -= HandleMissionFieldLoaded;

        fieldManager.OnMissionFieldLoaded += HandleMissionFieldLoaded;

        if (fieldManager.CurrentFieldRoot != null)
        {
            HandleMissionFieldLoaded(fieldManager.CurrentFieldRoot);
        }
    }

    public void Unbind()
    {
        if (fieldManager != null)
        {
            fieldManager.OnMissionFieldLoaded -= HandleMissionFieldLoaded;
        }

        UnregisterNodes();

        fieldManager = null;
    }

    private void HandleMissionFieldLoaded(MissionFieldRoot fieldRoot)
    {
        UnregisterNodes();

        if (fieldRoot == null)
            return;

        foreach (FieldNode node in fieldRoot.Nodes)
        {
            if (node == null)
                continue;

            registeredNodes.Add(node);

            node.OnCharacterEntered -= HandleCharacterEntered;

            node.OnCharacterEntered += HandleCharacterEntered;

            node.OnCharacterExited -= HandleCharacterExited;

            node.OnCharacterExited += HandleCharacterExited;
        }
    }

    private void UnregisterNodes()
    {
        foreach (FieldNode node in registeredNodes)
        {
            if (node == null)
                continue;

            node.OnCharacterEntered -= HandleCharacterEntered;

            node.OnCharacterExited -= HandleCharacterExited;
        }

        registeredNodes.Clear();
    }

    private void HandleCharacterEntered(FieldNode node, CharacterBase character)
    {
        if (node == null || character == null)
        {
            return;
        }

        UI_FieldCharacterMarker marker = GetOrCreateMarker(character);

        if (marker == null)
            return;

        marker.SetCharacter(character);
        marker.MoveToNode(node);
    }

    private void HandleCharacterExited(FieldNode node, CharacterBase character)
    {
        if (character == null)
            return;

        if (!markers.TryGetValue(character, out UI_FieldCharacterMarker marker))
        {
            return;
        }

        marker.gameObject.SetActive(false);
    }

    private UI_FieldCharacterMarker
        GetOrCreateMarker(CharacterBase character)
    {
        if (markers.TryGetValue(character, out UI_FieldCharacterMarker existing))
        {
            return existing;
        }

        if (markerTemplate == null)
        {
            Debug.LogWarning("필드 캐릭터 마커 템플릿이 없습니다.");

            return null;
        }

        UI_FieldCharacterMarker newMarker = Instantiate(markerTemplate, transform);

        newMarker.name = $"FieldMarker_{character.name}";

        newMarker.SetCharacter(character);

        markers.Add(character, newMarker);

        return newMarker;
    }

    public void ClearMarkers()
    {
        foreach (UI_FieldCharacterMarker marker in markers.Values)
        {
            if (marker != null)
            {
                marker.Clear();
            }
        }

        markers.Clear();
    }
}
