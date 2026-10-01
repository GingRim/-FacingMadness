using UnityEngine;

public class EX_TurnEndButtonTest : MonoBehaviour
{
    public void EndTurn()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        if (GameManager.Instance.Battle == null)
        {
            return;
        }

        GameManager.Instance.Battle.EndTurn();
    }
}

