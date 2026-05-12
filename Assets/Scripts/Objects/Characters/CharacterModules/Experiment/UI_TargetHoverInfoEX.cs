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
    [SerializeField] TextMeshProUGUI coommentText;
    [SerializeField] UnityEngine.UI.Image icon;
    [SerializeField] UnityEngine.UI.Slider bar;

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

        Refresh(target);

        Open();
    }

    /// <summary>
    /// 대상 캐릭터의 정보를 UI에 표시
    /// </summary>
    void Refresh(CharacterBase character)
    {
        nameText.SetText(character.DisplayName);

        HitpointModules hp = character.GetModule<HitpointModules>();

        if (hp == null)
        {
            coommentText.SetText("HP 정보 없음");
            bar.value = 0;
            return;
        }

        coommentText.SetText($"{hp.Current} / {hp.Max}");

        bar.maxValue = hp.Max;
        bar.value = hp.Current;
    }
}
