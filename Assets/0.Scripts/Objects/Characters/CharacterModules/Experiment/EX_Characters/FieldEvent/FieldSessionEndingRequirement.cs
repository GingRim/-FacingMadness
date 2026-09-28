using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어가 가진 정보와 생존 상태로 확정하는 세션 엔딩 조건입니다.
/// </summary>
[Serializable]
public class FieldSessionEndingRequirement
{
    [SerializeField]
    private FieldSessionEndingData ending;

    [SerializeField]
    private List<FieldInformationData> requiredInformation = new();

    [SerializeField]
    private bool requireAlive = true;

    public FieldSessionEndingData Ending => ending;

    public bool IsSatisfied(CharacterBase character)
    {
        if (character == null || ending == null)
            return false;

        if (requireAlive)
        {
            HitpointModules hitpoint = character.GetModule<HitpointModules>();
            SanityModule sanity = character.GetModule<SanityModule>();

            if ((hitpoint != null && hitpoint.IsEmpty) ||
                (sanity != null && sanity.IsEmpty))
            {
                return false;
            }
        }

        FieldInformationInventory inventory =
            FieldInformationInventory.Get(character);

        if (requiredInformation == null || requiredInformation.Count == 0)
            return true;

        if (inventory == null)
            return false;

        foreach (FieldInformationData information in requiredInformation)
        {
            if (information == null || !inventory.Contains(information))
                return false;
        }

        return true;
    }
}
