using System;
using UnityEngine;


/// <summary>
/// 카드 클릭 감지 담당.
/// 카드를 클릭하면 사용 선택 팝업을 연다.
/// 실제 카드 효과는 여기서 실행하지 않는다.
/// </summary>
public class CardCrkClick : MonoBehaviour
{
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
        Debug.Log($"{name}: BeginDrag 진입");
        if (isDragging)
            return;

        if (myCard == null || myCard.CardData == null)
        {
            Debug.LogWarning($"{name}: myCard 없음");
            return;
        }

        if (useSelectUI != null && useSelectUI.IsOpened)
        {
            Debug.LogWarning($"{name}: CardData 없음");
            return;
        }


        GameObject clickedObject = GameManager.Instance.Input.GetGameObjectUnderCursor();

        Debug.Log($"클릭 오브젝트: {(clickedObject != null ? clickedObject.name : "null")}");

        UI_Card clickedCard = clickedObject != null ? clickedObject.GetComponentInParent<UI_Card>() : null;

        Debug.Log($"클릭 카드: {(clickedCard != null ? clickedCard.name : "null")} / 내 카드: {myCard.name}");
            
        // 중요:
        // 모든 카드가 InputManager 이벤트를 받기 때문에,
        // "내 카드가 클릭된 경우"만 드래그를 시작해야 함.
        if (clickedCard != myCard)
            return;

        if (rectTransform == null)
        {
            Debug.LogWarning($"{name}: RectTransform 없음");
            return;
        }

        isDragging = true;

        originalParent = transform.parent;
        originalAnchoredPosition = rectTransform.anchoredPosition;

        myCard.SetRaycastBlock(false);

        transform.SetAsLastSibling();

        MoveCard(screenPosition);

        Debug.Log($"카드 드래그 시작: {myCard.CardData.cardName}");
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
            Debug.LogWarning("카드 드롭 실패: 조작 중인 캐릭터 없음");
            return;
        }

        if (myCard == null || myCard.CardData == null)
            return;

        CardData card = myCard.CardData;

        CardDropDecision decision = GetDropDecision(card, user, target);

        switch (decision.result)
        {
            case CardDropResult.Invalid:
                Debug.Log($"카드 사용 불가 / 카드 {card.cardName} / 대상 {(target != null ? target.name : "없음")}"
                );
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
            Debug.LogWarning("카드 직접 사용 실패: 카드 또는 사용자 없음");
            return false;
        }

        CardResolver resolver = new CardResolver();

        if (!resolver.CanUse(card, user, useCost))
        {
            Debug.Log("카드 직접 사용 실패: 코스트 부족 또는 사용 조건 불가");
            return false;
        }

        DeckModule deck = user.GetModule<DeckModule>();

        if (deck == null)
        {
            Debug.LogWarning($"{user.name}: DeckModule 없음");
            return false;
        }

        bool success = resolver.UseWithoutCostCheck(card, user, target, useCost);

        if (!success)
        {
            Debug.Log("카드 직접 사용 실패: 효과 처리 실패");
            return false;
        }

        deck.UseCard(card);

        UI_Hand handUI = GetComponentInParent<UI_Hand>();

        if (handUI == null)
        {
            handUI = FindFirstObjectByType<UI_Hand>();
        }

        if (handUI != null)
        {
            handUI.RefreshFromDeck(deck);
        }

        Debug.Log(
            $"카드 직접 사용 성공 / 카드 {card.cardName} / " +
            $"사용자 {user.name} / " +
            $"대상 {(target != null ? target.name : "없음")} / " +
            $"코스트 {useCost}"
        );

        return true;
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

        Debug.Log($"팝업 열기 / 카드 {card.cardName} / 사용자 {user.name} / 대상 {(target != null ? target.name : "없음")}"
        );

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
        GameObject hoverObject =
            GameManager.Instance.Input.GetGameObjectUnderCursor();

        Debug.Log($"드롭 위치 오브젝트: {(hoverObject != null ? hoverObject.name : "null")}");

        if (hoverObject == null)
            return null;

        CardDropTarget dropTarget = hoverObject.GetComponentInParent<CardDropTarget>();

        if (dropTarget == null)
        {
            dropTarget = hoverObject.GetComponentInChildren<CardDropTarget>();
        }

        if (dropTarget == null)
        {
            Debug.LogWarning($"{hoverObject.name}: CardDropTarget 없음");
            return null;
        }

        CharacterBase character = dropTarget.Character;

        if (character == null)
        {
            Debug.LogWarning($"{dropTarget.name}: CharacterBase 찾기 실패");
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
        }

        Debug.Log($"{card.cardName}: 아직 드롭 조건이 연결되지 않은 카드 색상");
        return CardDropDecision.Invalid();
    }

    private CardDropDecision GetRedDropDecision(CharacterBase user, CharacterBase target)
    {
        TeamType targetType = GetTargetTeamType(user, target);

        if (targetType == TeamType.Enemy)   
        {
            // 적색은 행동/보조 둘 다 공격이라 선택 필요
            return CardDropDecision.Popup();
        }

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
                // 버프 / 드로우 선택
                return CardDropDecision.Popup();

            default:
                return CardDropDecision.Invalid();
        }
    }


}