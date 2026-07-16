using TMPro;
using UnityEngine;

public class UI_TargetHoverInfoEX : OpenableUIBase
{
    [SerializeField] Vector2 shiftedPosition;

    /// <summary>
    /// 현재 마우스가 가리킨 캐릭터
    /// </summary>
    CharacterBase target;

    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI HPcoommentText;
    [SerializeField] TextMeshProUGUI SANcoommentText;
    [SerializeField] UnityEngine.UI.Image icon;
    [SerializeField] UnityEngine.UI.Slider HPbar;
    [SerializeField] UnityEngine.UI.Slider SANbar;

    public override void Registration(UIManager manager)
    {
        base.Registration(manager);

        InputManager.OnMouseHover -= HoverInfoChange;
        InputManager.OnMouseHover += HoverInfoChange;
    }

    public override void Unregistration(UIManager manager)
    {
        base.Unregistration(manager);

        InputManager.OnMouseHover -= HoverInfoChange;
    }

    private void MoveToMouse(Vector2 screenPosition, Vector3 worldPosition)
    {
        transform.position = screenPosition + shiftedPosition;
    }

    void HoverInfoChange(GameObject newTarget, GameObject oldTarget)
    {
        CharacterBase asCharacter = newTarget?.GetComponent<CharacterBase>();

        if (asCharacter == null)
        {
            target = null;
            Close();
            return;
        }

        target = asCharacter;

        HPRefresh(target);
        SANRefresh(target);

        Open();
    }

    /// <summary>
    /// 대상 캐릭터의 정보를 UI에 표시
    /// </summary>
    void HPRefresh(CharacterBase character)
    {
        nameText.SetText(character.DisplayName);

        HitpointModules hp = character.GetModule<HitpointModules>();

        if (hp == null)
        {
            HPcoommentText.SetText("HP 정보 없음");
            HPbar.value = 0;
            return;
        }

        HPcoommentText.SetText($"{hp.Current} / {hp.Max}");

        HPbar.maxValue = hp.Max;
        HPbar.value = hp.Current;
    }

    void SANRefresh(CharacterBase character)
    {
        nameText.SetText(character.DisplayName);

        SanityModule san = character.GetModule<SanityModule>();

        if (san == null)
        {
            SANcoommentText.SetText("HP 정보 없음");
            HPbar.value = 0;
            return;
        }

        SANcoommentText.SetText($"{san.CurrentSanity} / {san.MaxSanity}");

        SANbar.maxValue = san.MaxSanity;
        SANbar.value = san.CurrentSanity;
    }
}
