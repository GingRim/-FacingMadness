using UnityEngine;
using TMPro;
public class UI_BattleRound : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI roundText;

    private void Awake()
    {
        SetRound(0);
    }

    private void OnEnable()
    {
        BattleManager.OnRoundChanged -= SetRound;
        BattleManager.OnRoundChanged += SetRound;
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
}
