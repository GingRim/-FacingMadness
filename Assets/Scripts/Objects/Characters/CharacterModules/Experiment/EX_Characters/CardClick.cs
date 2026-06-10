using UnityEngine;


/// <summary>
/// 카드 클릭 감지 담당.
/// 카드를 클릭하면 사용 선택 팝업을 연다.
/// 실제 카드 효과는 여기서 실행하지 않는다.
/// </summary>
public class CardCrkClick : MonoBehaviour
{
    private UI_CardUseSelect useSelectUI;
    


    public void SetUseSelectUI(UI_CardUseSelect ui)
    {
        useSelectUI = ui;
    }

    private void OnEnable()
    {
        InputManager.OnMouseLeftButton -= OnClick;
        InputManager.OnMouseLeftButton += OnClick;
    }

    private void OnDisable()
    {
        InputManager.OnMouseLeftButton -= OnClick;
    }

    private void OnClick(bool value, Vector2 screenPosition, Vector3 worldPosition)
    {
        if (!value)
            return;

        if (useSelectUI != null && useSelectUI.IsOpened)
            return;

        GameObject clickedObject = GameManager.Instance.Input.GetGameObjectUnderCursor();
        

        UI_Card card = clickedObject?.GetComponentInParent<UI_Card>();

        if (card == null)
            return;

        CharacterBase user = FindControlledCharacter();
        CharacterBase target = FindDummyTarget();

        if (user == null || target == null || useSelectUI == null)
            return;
        if (card.CardData.color == CardColorType.Purple)
        {
            CardResolver resolver = new CardResolver();

            bool success = resolver.Use(card.CardData, user, target, CardUseCost.ActionAndAuxiliary);

            if (!success)
            {
                Debug.Log("자색 카드 사용 실패: 코스트 부족");
                return;
            }

            DeckModule deck = user.GetModule<DeckModule>();

            if (deck != null)
            {
                deck.UseCard(card.CardData, isExhaust: true);

                UI_Hand handUI = GetComponentInParent<UI_Hand>();

                handUI?.RefreshFromDeck(deck);
            }

            return;
        }
        useSelectUI.Open(card.CardData, user, target);

    }

    /// <summary>
    /// 컨트롤러가 연결된 캐릭터를 찾는다.
    /// 현재는 플레이어 캐릭터 판별용.
    /// </summary>  
    private CharacterBase FindDummyTarget()
    {
        CharacterBase[] characters =
            FindObjectsByType<CharacterBase>(FindObjectsSortMode.None);

        foreach (CharacterBase character in characters)
        {
            // 플레이어 캐릭터 제외
            if (character.Controller != null)
                continue;

            // 공격 받을 수 없는 오브젝트 제외
            if (character.GetModule<CombatModule>() == null)
                continue;

            // HP 없는 오브젝트 제외
            if (character.GetModule<HitpointModules>() == null)
                continue;

            return character;
        }

        return null;
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

