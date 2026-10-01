using UnityEngine;

/// <summary>
/// 몬스터 전용 피해 형식 내성입니다.
/// 1은 보통, 1보다 작으면 내성, 1보다 크면 약점입니다.
/// 플레이어 캐릭터에는 적용하지 않습니다.
/// </summary>
public class DamageResistanceModule : CharacterModule
{
    public sealed override System.Type RegistrationType
        => typeof(DamageResistanceModule);

    private float bluntMultiplier = 1f;
    private float slashMultiplier = 1f;
    private float pierceMultiplier = 1f;
    private float fireMultiplier = 1f;

    public void Configure(
        float blunt,
        float slash,
        float pierce,
        float fire)
    {
        bluntMultiplier = Mathf.Max(0f, blunt);
        slashMultiplier = Mathf.Max(0f, slash);
        pierceMultiplier = Mathf.Max(0f, pierce);
        fireMultiplier = Mathf.Max(0f, fire);
    }

    public float GetMultiplier(DamageFormType damageForm)
    {
        switch (damageForm)
        {
            case DamageFormType.Slash:
                return slashMultiplier;

            case DamageFormType.Pierce:
                return pierceMultiplier;

            case DamageFormType.Fire:
                return fireMultiplier;

            case DamageFormType.Blunt:
            default:
                return bluntMultiplier;
        }
    }
}
