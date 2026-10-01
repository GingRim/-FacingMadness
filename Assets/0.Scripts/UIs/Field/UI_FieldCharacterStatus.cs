using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 필드의 현재 플레이어 아이콘, 이름, 생명력과 정신력을 표시합니다.
/// GameManager가 생성한 FieldManager를 자동으로 찾아 연결합니다.
/// </summary>
public class UI_FieldCharacterStatus : MonoBehaviour
{
    [Header("캐릭터")]
    [SerializeField] private Image characterIcon;
    [SerializeField] private TextMeshProUGUI characterNameText;

    [Header("생명력")]
    [SerializeField] private Slider hitpointSlider;
    [SerializeField] private TextMeshProUGUI hitpointText;

    [Header("정신력")]
    [SerializeField] private Slider sanitySlider;
    [SerializeField] private TextMeshProUGUI sanityText;

    private FieldManager fieldManager;
    private CharacterBase boundCharacter;
    private HitpointModules boundHitpoint;
    private SanityModule boundSanity;
    private Coroutine bindRoutine;

    private void OnEnable()
    {
        TryBindRuntimeFieldManager();

        if (fieldManager == null)
            bindRoutine = StartCoroutine(WaitForRuntimeFieldManager());
    }

    private void OnDisable()
    {
        if (bindRoutine != null)
        {
            StopCoroutine(bindRoutine);
            bindRoutine = null;
        }

        UnbindFieldManager();
        UnbindCharacter();
    }

    private IEnumerator WaitForRuntimeFieldManager()
    {
        while (isActiveAndEnabled)
        {
            if (TryBindRuntimeFieldManager())
                break;

            yield return null;
        }

        bindRoutine = null;
    }

    private bool TryBindRuntimeFieldManager()
    {
        FieldManager runtimeField =
            GameManager.Instance != null
                ? GameManager.Instance.Field
                : null;

        if (runtimeField == null)
            return false;

        if (fieldManager != runtimeField)
        {
            UnbindFieldManager();
            fieldManager = runtimeField;
            fieldManager.OnCurrentPlayerChanged += HandleCurrentPlayerChanged;
        }

        BindCharacter(fieldManager.CurrentPlayer);
        return true;
    }

    private void UnbindFieldManager()
    {
        if (fieldManager != null)
            fieldManager.OnCurrentPlayerChanged -= HandleCurrentPlayerChanged;

        fieldManager = null;
    }

    private void HandleCurrentPlayerChanged(CharacterBase character)
    {
        BindCharacter(character);
    }

    private void BindCharacter(CharacterBase character)
    {
        if (boundCharacter == character)
        {
            RefreshAll();
            return;
        }

        UnbindCharacter();
        boundCharacter = character;

        if (boundCharacter == null)
        {
            Clear();
            return;
        }

        boundHitpoint = boundCharacter.GetModule<HitpointModules>();
        boundSanity = boundCharacter.GetModule<SanityModule>();

        if (boundHitpoint != null)
            boundHitpoint.OnHitpointChanged += HandleHitpointChanged;

        if (boundSanity != null)
            boundSanity.OnSanityChanged += HandleSanityChanged;

        RefreshAll();
    }

    private void UnbindCharacter()
    {
        if (boundHitpoint != null)
            boundHitpoint.OnHitpointChanged -= HandleHitpointChanged;

        if (boundSanity != null)
            boundSanity.OnSanityChanged -= HandleSanityChanged;

        boundHitpoint = null;
        boundSanity = null;
        boundCharacter = null;
    }

    private void RefreshAll()
    {
        if (boundCharacter == null)
        {
            Clear();
            return;
        }

        if (characterIcon != null)
        {
            characterIcon.sprite = boundCharacter.Icon;
            characterIcon.enabled = boundCharacter.Icon != null;
        }

        if (characterNameText != null)
            characterNameText.SetText(boundCharacter.DisplayName);

        if (boundHitpoint != null)
            SetHitpoint(boundHitpoint.Current, boundHitpoint.Max);
        else
            SetHitpoint(0, 0);

        if (boundSanity != null)
            SetSanity(boundSanity.CurrentSanity, boundSanity.MaxSanity);
        else
            SetSanity(0, 0);
    }

    private void HandleHitpointChanged(int current, int maximum)
    {
        SetHitpoint(current, maximum);
    }

    private void HandleSanityChanged(int current, int maximum)
    {
        SetSanity(current, maximum);
    }

    private void SetHitpoint(int current, int maximum)
    {
        SetSlider(hitpointSlider, current, maximum);

        if (hitpointText != null)
            hitpointText.SetText($"{current} / {maximum}");
    }

    private void SetSanity(int current, int maximum)
    {
        SetSlider(sanitySlider, current, maximum);

        if (sanityText != null)
            sanityText.SetText($"{current} / {maximum}");
    }

    private static void SetSlider(Slider slider, int current, int maximum)
    {
        if (slider == null)
            return;

        slider.minValue = 0f;
        slider.maxValue = Mathf.Max(1, maximum);
        slider.value = Mathf.Clamp(current, 0, Mathf.Max(0, maximum));
    }

    private void Clear()
    {
        if (characterIcon != null)
        {
            characterIcon.sprite = null;
            characterIcon.enabled = false;
        }

        if (characterNameText != null)
            characterNameText.SetText(string.Empty);

        SetHitpoint(0, 0);
        SetSanity(0, 0);
    }
}
