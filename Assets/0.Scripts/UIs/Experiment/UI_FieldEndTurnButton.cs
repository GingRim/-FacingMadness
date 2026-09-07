using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 필드에서 남은 행동력을 포기하고 현재 턴을 종료합니다.
/// 이벤트나 신화 턴 진행 중에는 작동하지 않습니다.
/// </summary>
[RequireComponent(typeof(Button))]
public class UI_FieldEndTurnButton : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        button.onClick.RemoveListener(EndTurn);
        button.onClick.AddListener(EndTurn);
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(EndTurn);
        }
    }

    public void EndTurn()
    {
        FieldManager fieldManager =
            GameManager.Instance != null
                ? GameManager.Instance.Field
                : null;

        if (fieldManager == null)
            return;

        if (!fieldManager.TryEndFieldTurn())
        {
            Debug.Log("현재는 필드 턴을 종료할 수 없습니다.");
        }
    }
}
