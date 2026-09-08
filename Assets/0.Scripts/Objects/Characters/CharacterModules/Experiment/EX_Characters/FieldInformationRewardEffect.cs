using UnityEngine;

[CreateAssetMenu(fileName = "NewFieldInformationReward", menuName = "Field/Event Effect/Information Reward")]
public class FieldInformationRewardEffect : FieldEventEffect
{
    [SerializeField]
    private FieldInformationData information;

    public override void Execute(FieldEventContext context)
    {
        if (context == null || context.Character == null || information == null)
            return;

        FieldInformationInventory inventory =
            FieldInformationInventory.GetOrCreate(context.Character);

        if (inventory == null)
            return;

        bool wasAdded = inventory.Add(information);

        // 이미 가진 정보라도 새로 생성된 필드의 숨김 경로에는
        // 같은 공개 규칙을 다시 적용할 수 있습니다.
        RevealRegisteredRoute(context);

        if (wasAdded && information.IsMemo)
            context.AddResultMessage("메모를 얻었다.");
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
