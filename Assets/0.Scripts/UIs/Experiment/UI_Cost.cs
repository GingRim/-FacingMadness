using System;
using TMPro;
using UnityEngine;


public class UI_Cost : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentName;
    [SerializeField] private CostType costName;
    
    private ActionPointModule actionPointModule;

    public CostType CostType => costName;

    private void OnEnable()
    {
        // 행동력 통합 이후 보조·대응 코스트 칸은 사용하지 않습니다.
        if (costName != CostType.Action)
        {
            gameObject.SetActive(false);
            return;
        }

        SetCharacter(FindControlledCharacter());
    }

    private void OnDisable()
    {
        UnbindActionPoint();
    }

    private CharacterBase FindControlledCharacter()
    {
        PlauerController[] controllers =
            FindObjectsByType<PlauerController>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        foreach (PlauerController controller in controllers)
        {
            if (controller != null && controller.Character != null)
                return controller.Character;
        }

        return null;
    }

    public void SetCharacter(CharacterBase character)
    {
        UnbindActionPoint();

        if (character == null)
        {
            SetValue(0, 0);
            return;
        }

        actionPointModule = character.GetModule<ActionPointModule>();

        if (actionPointModule == null)
        {
            Debug.LogWarning(
                $"{character.name}: 전투 UI에 필요한 ActionPointModule이 없습니다.");

            SetValue(0, 0);
            return;
        }

        actionPointModule.OnActionPointChanged -= HandleActionPointChanged;
        actionPointModule.OnActionPointChanged += HandleActionPointChanged;

        Refresh();
    }

    public void Refresh()
    {
        if (actionPointModule == null)
        {
            SetValue(0, 0);
            return;
        }

        SetValue(actionPointModule.Current, actionPointModule.Max);
    }

    private void HandleActionPointChanged(int current, int maximum)
    {
        SetValue(current, maximum);
    }

    private void SetValue(int current, int maximum)
    {
        if (currentName == null)
            return;

        currentName.SetText($"{current} / {maximum}");
    }

    private void UnbindActionPoint()
    {
        if (actionPointModule == null)
            return;

        actionPointModule.OnActionPointChanged -= HandleActionPointChanged;
        actionPointModule = null;
    }
}
