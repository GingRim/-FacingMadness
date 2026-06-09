using UnityEngine;

/// <summary>
/// 테스트용 코스트 초기화 버튼.
/// 컨트롤러가 연결된 캐릭터의 현재 코스트를 최대치로 회복한다.
/// </summary>
public class EX_CostResetButton : MonoBehaviour
{
    public void ResetCost()
    {
        CharacterBase character = FindControlledCharacter();

        if (character == null)
            return;

        CostModule cost = character.GetModule<CostModule>();

        if (cost == null)
            return;

        cost.RefillAll();
    }

    private CharacterBase FindControlledCharacter()
    {
        CharacterBase[] characters =
            FindObjectsByType<CharacterBase>(FindObjectsSortMode.None);

        foreach (CharacterBase character in characters)
        {
            if (character.Controller != null)
                return character;
        }

        return null;
    }
}