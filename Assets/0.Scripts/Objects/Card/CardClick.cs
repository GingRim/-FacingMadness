using System;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


/// <summary>
/// 카드 클릭 감지 담당.
/// 카드를 클릭하면 사용 선택 팝업을 연다.
/// 실제 카드 효과는 여기서 실행하지 않는다.
/// </summary>
public class CardCrkClick : MonoBehaviour
{
    [SerializeField] private UI_FieldCardSelector fieldCardSelector;
    [SerializeField] private UI_CardUseSelect useSelectUI;
    [SerializeField] private Canvas canvas;

    private UI_Card myCard;
    private RectTransform rectTransform;

    private Transform originalParent;
    private Vector2 originalAnchoredPosition;

    private bool isDragging;
    private GameObject currentHoverObject;

    private void Awake()
    {
        myCard = GetComponent<UI_Card>();
        rectTransform = GetComponent<RectTransform>();

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (useSelectUI == null)
            useSelectUI = FindFirstObjectByType<UI_CardUseSelect>(FindObjectsInactive.Include);

        if (fieldCardSelector == null)
        {
            fieldCardSelector =
                FindFirstObjectByType<UI_FieldCardSelector>(FindObjectsInactive.Include);
        }
    }

    public void SetUseSelectUI(UI_CardUseSelect ui)
    {
        useSelectUI = ui;
    }

    private void OnEnable()
    {
        InputManager.OnMouseLeftButton -= OnMouseLeftButton;
        InputManager.OnMouseLeftButton += OnMouseLeftButton;

        InputManager.OnMouseMove -= OnMouseMove;
        InputManager.OnMouseMove += OnMouseMove;

        InputManager.OnMouseHover -= OnMouseHover;
        InputManager.OnMouseHover += OnMouseHover;
    }

    private void OnDisable()
    {
        InputManager.OnMouseLeftButton -= OnMouseLeftButton;
        InputManager.OnMouseMove -= OnMouseMove;
        InputManager.OnMouseHover -= OnMouseHover;
    }

    private void OnMouseHover(GameObject newTarget, GameObject oldTarget)
    {
        currentHoverObject = newTarget;
    }

    private void OnMouseLeftButton(bool value, Vector2 screenPosition, Vector3 worldPosition)
    {
        if (value)
        {
            BeginDrag(screenPosition);
        }
        else
        {
            EndDrag(screenPosition);
        }
    }

    private void BeginDrag(Vector2 screenPosition)
    {

        if (isDragging)
            return;

        if (myCard == null || myCard.CardData == null)
        {
            return;
        }

        if (useSelectUI != null && useSelectUI.IsOpened)
        {
            return;
        }


        GameObject clickedObject = GameManager.Instance.Input.GetGameObjectUnderCursor();


        UI_Card clickedCard = clickedObject != null ? clickedObject.GetComponentInParent<UI_Card>() : null;

            
        // 중요:
        // 모든 카드가 InputManager 이벤트를 받기 때문에,
        // "내 카드가 클릭된 경우"만 드래그를 시작해야 함.
        if (clickedCard != myCard)
            return;

        if (rectTransform == null)
        {
            return;
        }

        isDragging = true;

        originalParent = transform.parent;
        originalAnchoredPosition = rectTransform.anchoredPosition;

        myCard.SetRaycastBlock(false);

        transform.SetAsLastSibling();

        MoveCard(screenPosition);

    }

    private void OnMouseMove(Vector2 screenPosition, Vector3 worldPosition)
    {
        if (!isDragging)
            return;

        MoveCard(screenPosition);
    }

    private void MoveCard(Vector2 screenPosition)
    {
        if (rectTransform == null)
            return;

        if (canvas == null)
        {
            rectTransform.position = screenPosition;
            return;
        }

        RectTransform canvasRect = canvas.transform as RectTransform;

        if (canvasRect == null)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out Vector2 localPoint);

        transform.SetParent(canvasRect, false);
        rectTransform.anchoredPosition = localPoint;
    }

    private void EndDrag(Vector2 screenPosition)
    {
        if (!isDragging)
            return;

        CharacterBase target = FindDropTarget();

        ReturnCard();

        isDragging = false;

        CharacterBase user = FindControlledCharacter();

        if (user == null)
        {
            return;
        }

        if (myCard == null || myCard.CardData == null)
            return;

        CardData card = myCard.CardData;

        // 필드 이벤트가 카드 선택을 기다리는 중이라면
        // 전투 카드 사용 처리로 넘어가지 않는다.
        if (TryHandleFieldCardSelection(card))
        {
            return;
        }

        CardDropDecision decision = GetDropDecision(card, user, target);


        switch (decision.result)
        {
            case CardDropResult.Invalid:

                return;

            case CardDropResult.OpenPopup:
                OpenPopup(card, user, target);
                return;

            case CardDropResult.UseDirect:
                TryUseCardDirect(card, user, target, decision.useCost);
                return;
        }
    }

    private bool TryUseCardDirect(CardData card, CharacterBase user, CharacterBase target, CardUseCost useCost)
    {
        if (card == null || user == null)
        {
            return false;
        }

        CardResolver resolver = new CardResolver();

        if (!resolver.CanUse(card, user, useCost))
        {
            BattleManager.ClaimBattleLog("코스트가<br>부족합니다.");
            return false;
        }

        DeckModule deck = user.GetModule<DeckModule>();

        if (deck == null)
        {
            return false;
        }

        bool success = resolver.UseWithoutCostCheck(card, user, target, useCost);

        if (!success)
        {
            return false;
        }

        bool isExhaust = ShouldExhaustOnUse(card);

        deck.UseCard(card, isExhaust);

        UI_Hand handUI = GetComponentInParent<UI_Hand>();

        if (handUI == null)
        {
            handUI = FindFirstObjectByType<UI_Hand>();
        }

        if (handUI != null)
        {
            handUI.RefreshFromDeck(deck);
        }


        return true;
    }

    private bool ShouldExhaustOnUse(CardData card)
    {
        if (card == null)
            return false;

        // 기본 자색 카드
        if (card.color == CardColorType.Purple && card.magicCardType == MagicCardType.None)
            return true;

        // 생성된 마법 카드도 사용 시 삭제/소멸 처리
        if (card.magicCardType != MagicCardType.None)
            return true;

        return false;
    }

    private void OpenPopup(CardData card, CharacterBase user, CharacterBase target)
    {
        if (useSelectUI == null)
        {
            useSelectUI = FindFirstObjectByType<UI_CardUseSelect>(FindObjectsInactive.Include);
        }

        if (useSelectUI == null)
        {
            Debug.LogWarning("팝업 열기 실패: UI_CardUseSelect 없음");
            return;
        }


        useSelectUI.Open(card, user, target);
    }

    private void ReturnCard()
    {
        if (myCard != null)
            myCard.SetRaycastBlock(true);

        if (originalParent != null)
        {
            transform.SetParent(originalParent, false);

            // 원래 자리로 돌리지 않고 핸드의 맨 뒤로 보냄
            transform.SetAsLastSibling();
        }

        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;

            // 중요:
            // anchoredPosition을 원래 값으로 되돌리지 않음.
            // LayoutGroup이 있으면 자동 정렬되게 둔다.
        }

        ForceRefreshHandLayout();
    }

    private void ForceRefreshHandLayout()
    {
        if (originalParent == null)
            return;

        RectTransform parentRect =
            originalParent as RectTransform;

        if (parentRect == null)
            return;

        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
    }

    private CharacterBase FindDropTarget()
    {
        GameObject hoverObject = GameManager.Instance.Input.GetGameObjectUnderCursor();


        if (hoverObject == null)
            return null;

        CardDropTarget dropTarget = hoverObject.GetComponentInParent<CardDropTarget>();

        if (dropTarget == null)
        {
            dropTarget = hoverObject.GetComponentInChildren<CardDropTarget>();
        }

        if (dropTarget == null)
        {
            return null;
        }

        CharacterBase character = dropTarget.Character;

        if (character == null)
        {
            return null;
        }

        return character;
    }

    private CharacterBase FindControlledCharacter()
    {
        CharacterBase[] characters = FindObjectsByType<CharacterBase>(FindObjectsSortMode.None);

        foreach (CharacterBase character in characters)
        {
            if (character.Controller != null)
                return character;
        }

        return null;
    }

    private TeamType GetTargetTeamType(CharacterBase user, CharacterBase target)
    {
        if (user == null || target == null)
            return TeamType.None;

        if (user == target)
            return TeamType.Self;

        bool userIsPlayer = user.Controller != null;
        bool targetIsPlayer = target.Controller != null;

        if (userIsPlayer == targetIsPlayer)
            return TeamType.Ally;

        return TeamType.Enemy;
    }

    private struct CardDropDecision
    {
        public CardDropResult result;
        public CardUseCost useCost;

        public static CardDropDecision Invalid()
        {
            return new CardDropDecision
            {
                result = CardDropResult.Invalid,
                useCost = CardUseCost.Action
            };
        }

        public static CardDropDecision Popup()
        {
            return new CardDropDecision
            {
                result = CardDropResult.OpenPopup,
                useCost = CardUseCost.Action
            };
        }

        public static CardDropDecision Direct(CardUseCost cost)
        {
            return new CardDropDecision
            {
                result = CardDropResult.UseDirect,
                useCost = cost
            };
        }   
    }

    private CardDropDecision GetDropDecision(CardData card, CharacterBase user, CharacterBase target)
    {
        if (card == null || user == null)
            return CardDropDecision.Invalid();

        // 생성된 마법 카드는 magicCardType으로 먼저 분기
        if (card.magicCardType != MagicCardType.None)
        {
            return GetMagicDropDecision(card, user, target);
        }

        switch (card.color)
        {
            case CardColorType.Red:
                return GetRedDropDecision(user, target);

            case CardColorType.Yellow:
                return GetYellowDropDecision(user, target);

            case CardColorType.Green:
                return GetGreenDropDecision(user, target);

            case CardColorType.Blue:
                return GetBlueDropDecision(user, target);

            case CardColorType.Purple:
                return GetBasicPurpleDropDecision();

            case CardColorType.Colorless:
                return GatColorlessDecision(user, target);
        }

        Debug.Log($"{card.cardName}: 아직 드롭 조건이 연결되지 않은 카드 색상");
        return CardDropDecision.Invalid();
    }


    private CardDropDecision GetMagicDropDecision(CardData card, CharacterBase user, CharacterBase target)
    {
        TeamType targetType = GetTargetTeamType(user, target);

        switch (card.magicCardType)
        {
            case MagicCardType.Attack:
                if (targetType == TeamType.Enemy)
                    return CardDropDecision.Direct(CardUseCost.ActionAndAuxiliary);

                BattleManager.ClaimBattleLog("공격 마법 사용 불가<br>적 대상만 가능");
                return CardDropDecision.Invalid();

            case MagicCardType.Defense:
                if (targetType == TeamType.Self || targetType == TeamType.Ally)
                    return CardDropDecision.Direct(CardUseCost.ActionAndAuxiliary);

                BattleManager.ClaimBattleLog("방어 마법 사용 불가<br>자신 또는 아군 대상만 가능");
                return CardDropDecision.Invalid();

            case MagicCardType.Buff:
                // 대상이 없어도 사용 가능.
                // 사용 시 자신과 아군 전체에게 버프.
                return CardDropDecision.Direct(CardUseCost.ActionAndAuxiliary);

            case MagicCardType.Forbidden:
                if (targetType == TeamType.Enemy)
                    return CardDropDecision.Direct(CardUseCost.ActionAndAuxiliary);

                BattleManager.ClaimBattleLog("금지된 마법 사용 불가<br>적 대상만 가능");
                return CardDropDecision.Invalid();
        }

        return CardDropDecision.Invalid();
    }

    private CardDropDecision GetBasicPurpleDropDecision()
    {
        // 기본 자색 카드는 대상 지정 없이 사용
        return CardDropDecision.Direct(CardUseCost.ActionAndAuxiliary);
    }

    private CardDropDecision GetRedDropDecision(CharacterBase user, CharacterBase target)
    {
        TeamType targetType = GetTargetTeamType(user, target);

        if (targetType == TeamType.Enemy)   
        {
            // 적색은 행동/보조 둘 다 공격이라 선택 필요
            return CardDropDecision.Popup();
        }
        BattleManager.ClaimBattleLog("적색 카드 사용 불가<br>적 대상만 가능");
        return CardDropDecision.Invalid();
    }

    private CardDropDecision GetYellowDropDecision(CharacterBase user, CharacterBase target)
    {
        TeamType targetType = GetTargetTeamType(user, target);

        switch (targetType)
        {
            case TeamType.Enemy:
                return CardDropDecision.Direct(CardUseCost.Action);

            case TeamType.Self:
            case TeamType.Ally:
                return CardDropDecision.Direct(CardUseCost.Auxiliary);

            default:
                
                return CardDropDecision.Invalid();
        }
    }

    private CardDropDecision GetGreenDropDecision(CharacterBase user, CharacterBase target)
    {
        TeamType targetType = GetTargetTeamType(user, target);

        switch (targetType)
        {
            case TeamType.Self:
                // 자신은 힐/장갑 선택
                return CardDropDecision.Popup();

            case TeamType.Ally:
                // 아군은 장갑 자동
                return CardDropDecision.Direct(CardUseCost.Auxiliary);

            default:
                BattleManager.ClaimBattleLog("녹색 카드 사용 불가<br>팀 대상만 가능");
                return CardDropDecision.Invalid();
        }
    }

    private CardDropDecision GetBlueDropDecision(CharacterBase user, CharacterBase target)
    {
        TeamType targetType = GetTargetTeamType(user, target);

        switch (targetType)
        {
            case TeamType.Enemy:
                return CardDropDecision.Direct(CardUseCost.Action);

            case TeamType.Self:
            case TeamType.Ally:
                return CardDropDecision.Direct(CardUseCost.Auxiliary);

            default:
                return CardDropDecision.Invalid();
        }
    }
    private CardDropDecision GatColorlessDecision(CharacterBase user, CharacterBase target)
    {
        TeamType targetType = GetTargetTeamType(user, target);

        switch (targetType)
        {
            case TeamType.Enemy:
                return CardDropDecision.Direct(CardUseCost.Action);

            case TeamType.Self:
            case TeamType.Ally:
                return CardDropDecision.Direct(CardUseCost.Auxiliary);

            default:
                return CardDropDecision.Invalid();
        }
    }


    private bool TryHandleFieldCardSelection(CardData card)
    {
        if (card == null)
            return false;

        if (fieldCardSelector == null)
        {
            fieldCardSelector = FindFirstObjectByType<UI_FieldCardSelector>(FindObjectsInactive.Include);
        }

        if (fieldCardSelector == null)
            return false;

        return fieldCardSelector.TrySelectCard(card);
    }

}