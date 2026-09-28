using System;
using System.Collections.Generic;
using UnityEngine;

public class FieldInformationInventory : MonoBehaviour
{
    [SerializeField]
    private List<FieldInformationData> acquiredInformation = new();

    private readonly List<FieldInformationData> memoInformation = new();

    public IReadOnlyList<FieldInformationData> AcquiredInformation => acquiredInformation;

    public IReadOnlyList<FieldInformationData> MemoInformation
    {
        get
        {
            RebuildMemoInformation();
            return memoInformation;
        }
    }

    public event Action<FieldInformationData> OnInformationAcquired;
    public event Action<FieldInformationData> OnInformationRemoved;

    public bool Add(FieldInformationData information)
    {
        if (information == null || Contains(information))
            return false;

        acquiredInformation.Add(information);
        OnInformationAcquired?.Invoke(information);
        return true;
    }

    public bool Contains(FieldInformationData information)
    {
        if (information == null)
            return false;

        foreach (FieldInformationData acquired in acquiredInformation)
        {
            if (acquired == information)
                return true;

            if (acquired != null &&
                !string.IsNullOrWhiteSpace(information.InformationId) &&
                acquired.InformationId == information.InformationId)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 지정한 정보와 같은 에셋 또는 같은 Information Id를 가진 정보를 제거합니다.
    /// 정보나 목록이 null이면 아무 작업도 하지 않습니다.
    /// </summary>
    public bool Remove(FieldInformationData information)
    {
        if (information == null || acquiredInformation == null)
            return false;

        for (int i = acquiredInformation.Count - 1; i >= 0; i--)
        {
            FieldInformationData acquired = acquiredInformation[i];

            if (acquired == null)
                continue;

            bool isSameInformation =
                acquired == information ||
                (!string.IsNullOrWhiteSpace(information.InformationId) &&
                 acquired.InformationId == information.InformationId);

            if (!isSameInformation)
                continue;

            acquiredInformation.RemoveAt(i);
            OnInformationRemoved?.Invoke(acquired);
            return true;
        }

        return false;
    }

    private void RebuildMemoInformation()
    {
        memoInformation.Clear();

        foreach (FieldInformationData information in acquiredInformation)
        {
            if (information != null && information.IsMemo)
                memoInformation.Add(information);
        }
    }

    public static FieldInformationInventory Get(CharacterBase character)
    {
        return character != null
            ? character.GetComponentInChildren<FieldInformationInventory>(true)
            : null;
    }

    public static FieldInformationInventory GetOrCreate(CharacterBase character)
    {
        if (character == null)
            return null;

        FieldInformationInventory inventory = Get(character);

        return inventory != null
            ? inventory
            : character.gameObject.AddComponent<FieldInformationInventory>();
    }
}
