using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_FieldActionPoint : MonoBehaviour
{
    [Header("행동력 표시")]
    [SerializeField]
    private TextMeshProUGUI actionPointText;

    [SerializeField]
    private Image actionPointFill;

    private FieldManager fieldManager;
    private ActionPointModule currentModule;

    private void OnDestroy()
    {
        Unbind();
    }

    public void Bind(
        FieldManager newFieldManager)
    {
        Unbind();

        fieldManager = newFieldManager;

        if (fieldManager == null)
        {
            Clear();
            return;
        }

        fieldManager.OnCurrentPlayerChanged -= HandleCurrentPlayerChanged;

        fieldManager.OnCurrentPlayerChanged += HandleCurrentPlayerChanged;

        HandleCurrentPlayerChanged(fieldManager.CurrentPlayer);
    }

    public void Unbind()
    {
        if (fieldManager != null)
        {
            fieldManager.OnCurrentPlayerChanged -= HandleCurrentPlayerChanged;
        }

        UnbindActionPointModule();

        fieldManager = null;

        Clear();
    }

    private void HandleCurrentPlayerChanged(CharacterBase player)
    {
        UnbindActionPointModule();

        if (player == null)
        {
            Clear();
            return;
        }

        currentModule = player.GetModule<ActionPointModule>();

        if (currentModule == null)
        {
            Debug.LogWarning($"{player.name}: " + "ActionPointModule이 없습니다.");

            Clear();
            return;
        }

        currentModule.OnActionPointChanged -= HandleActionPointChanged;

        currentModule.OnActionPointChanged += HandleActionPointChanged;

        Refresh();
    }

    private void UnbindActionPointModule()
    {
        if (currentModule == null)
            return;

        currentModule.OnActionPointChanged -= HandleActionPointChanged;

        currentModule = null;
    }

    private void HandleActionPointChanged(int current, int maximum)
    {
        SetValue(current, maximum);
    }

    private void Refresh()
    {
        if (currentModule == null)
        {
            Clear();
            return;
        }

        SetValue(currentModule.Current, currentModule.Max);
    }

    private void SetValue(int current, int maximum)
    {
        if (actionPointText != null)
        {
            actionPointText.SetText($"{current} / {maximum}");
        }

        if (actionPointFill != null)
        {
            float ratio = maximum > 0 ? (float)current / maximum : 0f;

            actionPointFill.fillAmount = Mathf.Clamp01(ratio);
        }
    }

    private void Clear()
    {
        if (actionPointText != null)
        {
            actionPointText.SetText("0 / 0");
        }

        if (actionPointFill != null)
        {
            actionPointFill.fillAmount = 0f;
        }
    }
}
