using UnityEngine;

public class EX_TurnEndButtonTest : MonoBehaviour
{
    public void EndTurn()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("턴 종료 실패: GameManager 없음");
            return;
        }

        if (GameManager.Instance.Battle == null)
        {
            Debug.LogError("턴 종료 실패: BattleManager 없음");
            return;
        }

        GameManager.Instance.Battle.EndTurn();
    }
}

