using UnityEngine;

[CreateAssetMenu(fileName = "NewFieldInformationReward", menuName = "Field/Event Effect/Information Reward")]
public class FieldInformationRewardEffect : FieldEventEffect
{
    [SerializeField]
    private FieldInformationData information;

    [Header("연결할 미션 목표 (선택)")]
    [Tooltip("정보를 처음 획득했을 때 진행시킬 목표 ID입니다. 비워 두면 정보만 지급합니다.")]
    [SerializeField]
    private string missionObjectiveId;

    [SerializeField, Min(1)]
    private int missionProgressAmount = 1;

    public override void Execute(FieldEventContext context)
    {
        if (context == null || context.Character == null || information == null)
            return;

        FieldInformationInventory inventory =
            FieldInformationInventory.GetOrCreate(context.Character);

        if (inventory == null)
            return;

        bool wasAdded = inventory.Add(information);

        if (wasAdded &&
            context.FieldManager != null &&
            !string.IsNullOrWhiteSpace(missionObjectiveId))
        {
            context.FieldManager.AddMissionProgress(
                missionObjectiveId,
                Mathf.Max(1, missionProgressAmount));
        }

        // 이미 가진 정보라도 새로 생성된 필드의 숨김 경로에는
        // 같은 공개 규칙을 다시 적용할 수 있습니다.
        RevealRegisteredRoute(context);

        if (wasAdded)
        {
            AddInformationResultMessage(context);
            AddInformationPopupMessage(context);
        }
    }

    private void AddInformationPopupMessage(FieldEventContext context)
    {
        if (information.IsMemo)
        {
            string title = string.IsNullOrWhiteSpace(information.Title) ? "메모" : information.Title.Trim();

            context.AddPostResultPopupMessage($"{title}{GetObjectParticle(title)} 얻었다.");
            return;
        }

        if (information.IsMemory)
        {
            context.AddPostResultPopupMessage("특징을 기억했다.");
        }
    }

    private static string GetObjectParticle(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "를";

        char lastCharacter = text[text.Length - 1];

        if (lastCharacter < 0xAC00 || lastCharacter > 0xD7A3)
            return "를";

        bool hasFinalConsonant = (lastCharacter - 0xAC00) % 28 != 0;

        return hasFinalConsonant ? "을" : "를";
    }

    private void AddInformationResultMessage(FieldEventContext context)
    {
        string acquiredMessage;

        if (information.IsMemo)
        {
            acquiredMessage = "메모 획득";

            if (!string.IsNullOrWhiteSpace(information.Description))
            {
                acquiredMessage += "\n\n" + information.Description.Trim();
            }
        }
        else if (information.IsMemory)
        {
            acquiredMessage = "특징을 기억했다.";
        }
        else
        {
            return;
        }

        context.AddLeadingResultMessage(acquiredMessage);
    }

    private void RevealRegisteredRoute(FieldEventContext context)
    {
        if (context.FieldManager == null ||
            context.FieldManager.CurrentFieldRoot == null)
        {
            return;
        }

        MissionFieldRoot fieldRoot = context.FieldManager.CurrentFieldRoot;

        foreach (string lineId in information.RevealLineIds)
        {
            if (string.IsNullOrWhiteSpace(lineId))
                continue;

            FieldLine line = fieldRoot.FindLine(lineId);

            if (line == null)
            {
                Debug.LogWarning($"정보 경로 공개 실패: 라인 {lineId}를 찾지 못했습니다.");
                continue;
            }

            line.RevealAs(information.RevealedLineType);
        }

        foreach (string nodeId in information.RevealNodeIds)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
                continue;

            FieldNode node = fieldRoot.FindNode(nodeId);

            if (node == null)
            {
                Debug.LogWarning($"정보 경로 공개 실패: 노드 {nodeId}를 찾지 못했습니다.");
                continue;
            }

            node.DiscoverHiddenArea();
        }
    }
}
