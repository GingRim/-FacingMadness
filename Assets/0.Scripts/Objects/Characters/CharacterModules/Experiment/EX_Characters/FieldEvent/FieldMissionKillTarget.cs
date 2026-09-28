using UnityEngine;

/// <summary>
/// 이 컴포넌트가 붙은 캐릭터를 플레이어가 직접 처치하면
/// 지정한 필드 미션 목표를 진행시킨다.
/// 소녀, 보스 등 대상 종류와 관계없이 재사용할 수 있다.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterBase))]
public class FieldMissionKillTarget : MonoBehaviour
{
    [Header("완료할 미션 목표")]
    [SerializeField]
    private string objectiveId;

    [SerializeField, Min(1)]
    private int amount = 1;

    private CharacterBase target;
    private HitpointModules hitpoint;
    private bool killReported;

    private void OnEnable()
    {
        BattleManager.OnBattleStarted -= HandleBattleStarted;
        BattleManager.OnBattleStarted += HandleBattleStarted;
        BindHitpoint();
    }

    private void Start()
    {
        BindHitpoint();
    }

    private void OnDisable()
    {
        BattleManager.OnBattleStarted -= HandleBattleStarted;
        UnbindHitpoint();
    }

    private void HandleBattleStarted()
    {
        BindHitpoint();
    }

    private void BindHitpoint()
    {
        if (target == null)
            target = GetComponent<CharacterBase>();

        HitpointModules resolved =
            target != null
                ? target.GetModule<HitpointModules>()
                : null;

        if (resolved == hitpoint)
            return;

        UnbindHitpoint();
        hitpoint = resolved;

        if (hitpoint != null)
            hitpoint.OnKilled += HandleKilled;
    }

    private void UnbindHitpoint()
    {
        if (hitpoint != null)
            hitpoint.OnKilled -= HandleKilled;

        hitpoint = null;
    }

    private void HandleKilled(DamageStruct killingDamage)
    {
        if (killReported || !WasKilledDirectlyByPlayer(killingDamage))
            return;

        FieldManager fieldManager =
            GameManager.Instance != null
                ? GameManager.Instance.Field
                : null;

        if (fieldManager == null)
            return;

        if (!fieldManager.AddMissionProgress(
                objectiveId,
                Mathf.Max(1, amount)))
        {
            return;
        }

        killReported = true;
        Debug.Log($"직접 처치 미션 목표 달성: {objectiveId} / 대상:{target.name}");
    }

    private bool WasKilledDirectlyByPlayer(DamageStruct killingDamage)
    {
        if (killingDamage.instigator is PlauerController)
            return true;

        CharacterBase attacker = null;

        if (killingDamage.from != null)
        {
            attacker =
                killingDamage.from.GetComponent<CharacterBase>();

            if (attacker == null)
            {
                attacker =
                    killingDamage.from.GetComponentInParent<CharacterBase>();
            }
        }

        return attacker != null &&
               attacker.Controller is PlauerController;
    }
}
