using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonsterAIProfile))]
public class MonsterAIProfileEditor : Editor
{
    private SerializedProperty maxActionsPerTurn;
    private SerializedProperty actions;
    private SerializedProperty reactions;

    private void OnEnable()
    {
        maxActionsPerTurn = serializedObject.FindProperty("maxActionsPerTurn");
        actions = serializedObject.FindProperty("actions");
        reactions = serializedObject.FindProperty("reactions");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("턴 행동 제한", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(
            maxActionsPerTurn,
            new GUIContent("턴당 전체 행동 수", "몬스터가 자기 턴에 실행할 수 있는 전체 행동 수의 상한입니다."));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("행동 목록", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(
            actions,
            new GUIContent("행동", "조건과 우선순위를 확인하여 자기 턴에 선택할 행동 목록입니다."),
            true);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("대응 목록", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(
            reactions,
            new GUIContent("대응", "공격받을 때 조건과 우선순위를 확인하여 선택할 대응 목록입니다."),
            true);

        serializedObject.ApplyModifiedProperties();
    }
}

internal static class MonsterAIEditorGUI
{
    public readonly struct FieldInfo
    {
        public readonly string PropertyName;
        public readonly string KoreanName;
        public readonly string Tooltip;

        public FieldInfo(string propertyName, string koreanName, string tooltip)
        {
            PropertyName = propertyName;
            KoreanName = koreanName;
            Tooltip = tooltip;
        }
    }

    public static float GetHeight(SerializedProperty property, IReadOnlyList<FieldInfo> fields)
    {
        float height = EditorGUIUtility.singleLineHeight;

        if (!property.isExpanded)
            return height;

        for (int i = 0; i < fields.Count; i++)
        {
            SerializedProperty child = property.FindPropertyRelative(fields[i].PropertyName);

            if (child == null)
                continue;

            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(child, true);
        }

        return height;
    }

    public static void Draw(
        Rect position,
        SerializedProperty property,
        string elementName,
        IReadOnlyList<FieldInfo> fields)
    {
        EditorGUI.BeginProperty(position, GUIContent.none, property);

        Rect line = new Rect(
            position.x,
            position.y,
            position.width,
            EditorGUIUtility.singleLineHeight);

        property.isExpanded = EditorGUI.Foldout(
            line,
            property.isExpanded,
            GetElementLabel(property, elementName),
            true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            for (int i = 0; i < fields.Count; i++)
            {
                FieldInfo field = fields[i];
                SerializedProperty child = property.FindPropertyRelative(field.PropertyName);

                if (child == null)
                    continue;

                float childHeight = EditorGUI.GetPropertyHeight(child, true);
                line.y += line.height + EditorGUIUtility.standardVerticalSpacing;
                line.height = childHeight;

                EditorGUI.PropertyField(
                    line,
                    child,
                    new GUIContent(field.KoreanName, field.Tooltip),
                    true);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    private static GUIContent GetElementLabel(SerializedProperty property, string elementName)
    {
        int start = property.propertyPath.LastIndexOf("data[", StringComparison.Ordinal);

        if (start < 0)
            return new GUIContent(elementName);

        start += 5;
        int end = property.propertyPath.IndexOf(']', start);

        if (end <= start ||
            !int.TryParse(property.propertyPath.Substring(start, end - start), out int index))
        {
            return new GUIContent(elementName);
        }

        return new GUIContent($"{elementName} {index + 1}");
    }
}

[CustomPropertyDrawer(typeof(MonsterAICondition))]
public class MonsterAIConditionDrawer : PropertyDrawer
{
    private static readonly MonsterAIEditorGUI.FieldInfo[] Fields =
    {
        new("conditionType", "조건 종류", "이 조건에서 검사할 내용입니다."),
        new("percent", "기준 체력 비율", "체력 비율 조건의 기준값입니다. 50이면 최대 체력의 50%입니다."),
        new("round", "기준 라운드", "라운드 조건에서 사용할 기준 라운드입니다."),
        new("statusType", "검사할 상태", "상태 보유 여부 조건에서 검사할 상태입니다.")
    };

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return MonsterAIEditorGUI.GetHeight(property, Fields);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        MonsterAIEditorGUI.Draw(position, property, "조건", Fields);
    }
}

[CustomPropertyDrawer(typeof(MonsterAIAction))]
public class MonsterAIActionDrawer : PropertyDrawer
{
    private static readonly MonsterAIEditorGUI.FieldInfo[] Fields =
    {
        new("actionId", "행동 식별자", "같은 프로필 안에서 행동을 구분하는 고유 식별자입니다."),
        new("displayName", "표시 이름", "전투 로그에 표시할 행동 이름입니다."),
        new("actionType", "행동 종류", "실제로 실행할 행동의 종류입니다."),
        new("targetType", "대상 선택 방식", "행동을 적용할 대상을 정하는 방식입니다."),
        new("priority", "우선순위", "숫자가 가장 높은 우선순위의 행동만 선택 후보가 됩니다."),
        new("weight", "선택 가중치", "우선순위가 같은 후보 사이의 무작위 선택 비중입니다."),
        new("actionPointCost", "행동력 비용", "행동을 한 번 실행할 때 소비하는 행동력입니다."),
        new("maxUsesPerTurn", "턴당 최대 사용 횟수", "0이면 턴당 사용 횟수를 제한하지 않습니다."),
        new("maxUsesPerBattle", "전투당 최대 사용 횟수", "0이면 전투당 사용 횟수를 제한하지 않습니다."),
        new("cooldownRounds", "재사용 대기 라운드", "0이면 재사용 대기시간이 없습니다."),
        new("conditions", "사용 조건", "목록의 조건을 모두 충족해야 행동 후보가 됩니다. 비어 있으면 조건이 없습니다."),
        new("fixedValue", "고정값", "주사위 결과와 능력치 보정에 더하는 고정 수치입니다."),
        new("diceCount", "주사위 개수", "효과 수치를 계산할 때 굴릴 주사위의 개수입니다."),
        new("diceType", "주사위 종류", "효과 수치를 계산할 때 사용할 주사위입니다."),
        new("modifierStat", "보정 능력치", "효과 수치에 보정치를 더할 능력치입니다."),
        new("statusType", "부여할 상태", "상태 부여 행동에서 적용할 상태입니다."),
        new("damageType", "피해 종류", "공격 행동이 입히는 피해의 종류입니다."),
        new("damageForm", "피해 형식", "타격·참격·관통·화염 중 공격의 실제 피해 형식입니다."),
        new("canCounter", "반격 허용", "공격 대상이 반격 대응을 사용할 수 있는지 정합니다."),
        new("allowCritical", "치명타 허용", "몬스터의 민첩 판정으로 치명타가 발생할 수 있는지 정합니다.")
    };

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return MonsterAIEditorGUI.GetHeight(property, Fields);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        MonsterAIEditorGUI.Draw(position, property, "행동", Fields);
    }
}

[CustomPropertyDrawer(typeof(MonsterAIReaction))]
public class MonsterAIReactionDrawer : PropertyDrawer
{
    private static readonly MonsterAIEditorGUI.FieldInfo[] Fields =
    {
        new("reactionId", "대응 식별자", "같은 프로필 안에서 대응을 구분하는 고유 식별자입니다."),
        new("reactionType", "대응 종류", "공격받았을 때 사용할 방어, 회피 또는 반격입니다."),
        new("priority", "우선순위", "숫자가 가장 높은 우선순위의 대응만 선택 후보가 됩니다."),
        new("weight", "선택 가중치", "우선순위가 같은 후보 사이의 무작위 선택 비중입니다."),
        new("chancePercent", "발동 확률", "조건을 충족한 뒤 이 대응을 시도할 확률입니다."),
        new("actionPointCost", "행동력 비용", "대응을 한 번 실행할 때 소비하는 행동력입니다."),
        new("maxUsesPerRound", "라운드당 최대 사용 횟수", "0이면 라운드당 사용 횟수를 제한하지 않습니다."),
        new("requiresStatRoll", "능력치 판정 사용", "발동 시 1D100 능력치 이하 판정을 추가로 실행합니다."),
        new("rollUnderStat", "판정 능력치", "1D100 결과와 비교할 능력치입니다."),
        new("conditions", "발동 조건", "목록의 조건을 모두 충족해야 대응 후보가 됩니다. 비어 있으면 조건이 없습니다.")
    };

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return MonsterAIEditorGUI.GetHeight(property, Fields);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        MonsterAIEditorGUI.Draw(position, property, "대응", Fields);
    }
}
