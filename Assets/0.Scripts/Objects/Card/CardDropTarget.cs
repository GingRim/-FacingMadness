using UnityEngine;

/// <summary>
/// 전투 캐릭터에게 카드를 놓을 수 있는 드롭 위치입니다.
/// 대상 캐릭터를 확인하고 전투 카드 처리기로 전달합니다.
/// </summary>
public class CardDropTarget : CardDropReceiver
{
    [SerializeField]
    private BattleCardUseController cardUseController;

    private CharacterBase cachedCharacter;

    public CharacterBase Character
    {
        get
        {
            if (cachedCharacter != null)
                return cachedCharacter;

            cachedCharacter = GetComponentInParent<CharacterBase>();

            if (cachedCharacter == null)
            {
                cachedCharacter = GetComponentInChildren<CharacterBase>();
            }

            return cachedCharacter;
        }
    }

    public override bool TryReceiveCard(CardInstance card)
    {
        CharacterBase target = Character;

        if (card == null || card.Data == null || target == null)
        {
            return false;
        }

        if (cardUseController == null)
        {
            cardUseController = FindFirstObjectByType<BattleCardUseController>();
        }

        if (cardUseController == null)
        {
            cardUseController = FindFirstObjectByType<BattleCardUseController>(FindObjectsInactive.Include);
        }

        if (cardUseController == null)
        {
            Debug.LogWarning(
                "전투 카드 처리기를 찾지 못했습니다.");

            return false;
        }

        return
            cardUseController.TryUseCard(card, target);
    }

    public bool TryGetCharacter(out CharacterBase character)
    {
        character = Character;

        return character != null;
    }
}
