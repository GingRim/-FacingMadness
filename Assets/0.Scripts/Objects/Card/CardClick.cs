using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 카드의 클릭과 드래그 표현만 담당합니다.
/// 드롭 결과의 규칙은 드롭 위치의 CardDropReceiver가 처리합니다.
/// </summary>
public class CardClick : MonoBehaviour
{
    [Header("드래그")]
    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private CanvasGroup canvasGroup;

    [SerializeField, Min(0f)]
    private float dragThreshold = 10f;

    private UI_Card myCard;
    private RectTransform rectTransform;
    private Transform originalParent;
    private Vector2 pressScreenPosition;
    private bool isDragging;
    private bool hasMoved;

    /// <summary>
    /// 드래그하지 않고 카드를 클릭했을 때 발생합니다.
    /// </summary>
    public event Action<CardInstance> OnClicked;

    private void Awake()
    {
        myCard = GetComponent<UI_Card>();
        rectTransform = GetComponent<RectTransform>();

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }
    }

    private void OnEnable()
    {
        InputManager.OnMouseLeftButton -= OnMouseLeftButton;
        InputManager.OnMouseLeftButton += OnMouseLeftButton;

        InputManager.OnMouseMove -= OnMouseMove;
        InputManager.OnMouseMove += OnMouseMove;
    }

    private void OnDisable()
    {
        InputManager.OnMouseLeftButton -= OnMouseLeftButton;
        InputManager.OnMouseMove -= OnMouseMove;

        if (isDragging && hasMoved)
        {
            ReturnCard();
        }

        isDragging = false;
        hasMoved = false;
    }

    private void OnMouseLeftButton(
        bool pressed,
        Vector2 screenPosition,
        Vector3 worldPosition)
    {
        if (pressed)
        {
            BeginDrag(screenPosition);
        }
        else
        {
            EndDrag();
        }
    }

    private void BeginDrag(Vector2 screenPosition)
    {
        if (isDragging ||
            myCard == null ||
            myCard.CardInstance == null ||
            rectTransform == null ||
            GameManager.Instance == null ||
            GameManager.Instance.Input == null)
        {
            return;
        }

        GameObject clickedObject =
            GameManager.Instance.Input.GetGameObjectUnderCursor();

        UI_Card clickedCard = clickedObject != null
            ? clickedObject.GetComponentInParent<UI_Card>()
            : null;

        // 모든 카드가 공용 입력 이벤트를 받으므로
        // 실제로 누른 카드만 드래그를 시작합니다.
        if (clickedCard != myCard)
            return;

        isDragging = true;
        hasMoved = false;
        originalParent = transform.parent;
        pressScreenPosition = screenPosition;
    }

    private void OnMouseMove(
        Vector2 screenPosition,
        Vector3 worldPosition)
    {
        if (!isDragging)
            return;

        if (!hasMoved)
        {
            float distance = Vector2.Distance(
                pressScreenPosition,
                screenPosition);

            if (distance < dragThreshold)
                return;

            hasMoved = true;
            SetRaycastBlock(false);
            transform.SetAsLastSibling();
        }

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

        RectTransform canvasRect =
            canvas.transform as RectTransform;

        if (canvasRect == null)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera,
            out Vector2 localPoint);

        transform.SetParent(canvasRect, false);
        rectTransform.anchoredPosition = localPoint;
    }

    private void EndDrag()
    {
        if (!isDragging)
            return;

        bool wasDragged = hasMoved;
        isDragging = false;
        hasMoved = false;

        CardInstance card =
            myCard != null
                ? myCard.CardInstance
                : null;

        if (!wasDragged)
        {
            if (card != null)
            {
                OnClicked?.Invoke(card);
            }

            return;
        }

        CardDropReceiver receiver = FindDropReceiver();

        ReturnCard();

        if (card == null || receiver == null)
            return;

        // 전투 대상, 이벤트 판정, 일반 필드 사용은
        // 각 드롭 위치가 CardInstance를 받아 처리합니다.
        receiver.TryReceiveCard(card);
    }

    private CardDropReceiver FindDropReceiver()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.Input == null)
        {
            return null;
        }

        GameObject hoverObject =
            GameManager.Instance.Input.GetGameObjectUnderCursor();

        return hoverObject != null
            ? hoverObject.GetComponentInParent<CardDropReceiver>()
            : null;
    }

    private void ReturnCard()
    {
        SetRaycastBlock(true);

        if (originalParent != null)
        {
            transform.SetParent(originalParent, false);
            transform.SetAsLastSibling();
        }

        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
        }

        ForceRefreshHandLayout();
    }

    private void SetRaycastBlock(bool value)
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = value;
        }
    }

    public void ClearClickListeners()
    {
        OnClicked = null;
    }

    private void ForceRefreshHandLayout()
    {
        RectTransform parentRect =
            originalParent as RectTransform;

        if (parentRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
        }
    }
}
