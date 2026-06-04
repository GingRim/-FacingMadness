using UnityEngine;


/// <summary>
/// 카드 클릭 감지 담당.
/// 카드를 클릭하면 사용 선택 팝업을 연다.
/// 실제 카드 효과는 여기서 실행하지 않는다.
/// </summary>
public class CardCrkClick : MonoBehaviour
{

    [Header("사용 선택 팝업")]
    [SerializeField] private UI_CardUseSelect useSelectUI;
    
    [Header("임시 고정 타겟")]
    [SerializeField] private CharacterBase fixedTarget;


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

        GameObject clickedObject = GameManager.Instance.Input.GetGameObjectUnderCursor();

        UI_Card card = clickedObject?.GetComponentInParent<UI_Card>();

        if (card == null)
            return;

        CharacterBase user = FindControlledCharacter();

        if (user == null)
            return;

        if (useSelectUI == null)
            return;

        // 카드 클릭 시 팝업만 연다.
        useSelectUI.Open(card.CardData, user, fixedTarget);

    }

    /// <summary>
    /// 컨트롤러가 연결된 캐릭터를 찾는다.
    /// 현재는 플레이어 캐릭터 판별용.
    /// </summary>  
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

