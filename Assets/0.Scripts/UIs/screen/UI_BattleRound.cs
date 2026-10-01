using UnityEngine;
using TMPro;
public class UI_BattleRound : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI roundText;

    private void Awake()
    {
        RefreshCurrentRound();
    }

    private void OnEnable()
    {
        BattleManager.OnRoundChanged -= SetRound;
        BattleManager.OnRoundChanged += SetRound;

        RefreshCurrentRound();
    }

    private void OnDisable()
    {
        BattleManager.OnRoundChanged -= SetRound;
    }

    private void SetRound(int round)
    {
        if (roundText == null)
            return;

        roundText.SetText($"{round}");
    }

    private void RefreshCurrentRound()
    {
        BattleManager battleManager =
            GameManager.Instance != null
                ? GameManager.Instance.Battle
                : null;

        SetRound(battleManager != null
            ? battleManager.DisplayRound
            : 0);
    }
}
