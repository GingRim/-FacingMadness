using System;
using UnityEngine;

public class SanityModule : CharacterModule
{
    public sealed override System.Type RegistrationType
        => typeof(SanityModule);

    [SerializeField] private int currentSanity;
    [SerializeField] private int maxSanity;

    public int CurrentSanity => currentSanity;
    public int MaxSanity => maxSanity;
    public bool IsEmpty => currentSanity <= 0;

    public event Action<int, int> OnSanityChanged;
    public event Action OnEmpty;

    /// <summary>
    /// 최대 정신력 설정
    /// </summary>
    public void SetMaxSanity(int value)
    {
        maxSanity = Mathf.Max(0, value);

        if (currentSanity > maxSanity)
        {
            currentSanity = maxSanity;
        }

        OnSanityChanged?.Invoke(currentSanity, maxSanity);
    }

    /// <summary>
    /// 정신력 최대치까지 회복
    /// </summary>
    public void FillSanity()
    {
        currentSanity = maxSanity;

        OnSanityChanged?.Invoke(currentSanity, maxSanity);

        Debug.Log($"정신력 최대 회복: {currentSanity}/{maxSanity}");
    }

    /// <summary>
    /// 정신력 감소
    /// </summary>
    public void TakeSanityDamage(int value)
    {
        if (value <= 0)
            return;

        int before = currentSanity;

        currentSanity = Mathf.Max(0, currentSanity - value);

        if (currentSanity != before)
            OnSanityChanged?.Invoke(currentSanity, maxSanity);

        Debug.Log($"정신력 감소: {before} -> {currentSanity} / 감소 {value}");


        if (before > 0 && currentSanity <= 0)
        {
            EnterMadness();
            OnEmpty?.Invoke();
        }
    }

    private void EnterMadness()
    {
        Debug.Log("정신력 0 도달: 광기 진입");

       // OnMadnessEntered?.Invoke();
    }

    /// <summary>
    /// 정신력 회복
    /// </summary>
    public void RestoreSanity(int value)
    {
        if (value <= 0)
            return;

        int before = currentSanity;

        currentSanity = Mathf.Min(maxSanity, currentSanity + value);

        if (currentSanity != before)
            OnSanityChanged?.Invoke(currentSanity, maxSanity);

        Debug.Log($"정신력 회복: {before} -> {currentSanity} / 회복 {value}");
    }

    private void OnSanityBroken()
    {
        Debug.Log("정신력 0 도달");
    }
}
