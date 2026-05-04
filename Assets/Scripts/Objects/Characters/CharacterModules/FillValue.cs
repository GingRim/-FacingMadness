using System;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;

public delegate void FillValueChangeEvent(FillValue value);

[System.Serializable]
public struct FillValue
{
    [SerializeField] int _current;
    [SerializeField] int _Max;
    int _Min;

    public event FillValueChangeEvent OnChanged;

    public int Current
    {
        readonly get => _current;
        set
        {
            _current = Mathf.Clamp(value, Min, Max);
            OnChanged?.Invoke(this);
        }
    }
    public int Min => _Min;
    public int Max => _Max;
    public float Percent => (float)Current / Max;

    public bool IsEmpty => Current <= Min;
    public bool IsMax => Current >= Max;

    public bool IsUnderZero => Current <= 0;

    public FillValue(int current, int max, int min = 0)
    {
        _current = current;
        _Max = max;
        _Min = min;
        OnChanged = null;
    }
    
    public FillValue(int max)
    {
        _current = _Max = max;
        _Min = 0;
        OnChanged = null;
    }

    public int IncreaseCurrent(int value) => Current += value;
    public int DecreaseCurrent(int value) => Current -= value;
    public int SetCurrent(int value) => Current = value;
    public int SetFull(int value) => Current = Max;
    public int SetEmpty(int value) => Current = Min;
    public int SetPercent(float value) => Current = Mathf.CeilToInt(Mathf.Lerp(Min, Max, Mathf.Clamp(value, 0.0f, 1.0f)));

    public void SetMax(int value) { _Max = value; Current = Current; }
    public void SetMin(int value) { _Min = value; Current = Current; }
}
