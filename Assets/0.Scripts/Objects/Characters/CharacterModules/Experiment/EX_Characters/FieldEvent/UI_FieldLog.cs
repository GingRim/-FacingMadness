using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 필드 진행 로그를 고정된 슬롯 수만큼 표시합니다.
/// 가장 최근 기록은 아래쪽 슬롯에 표시됩니다.
/// </summary>
public class UI_FieldLog : MonoBehaviour
{
    [SerializeField] private Transform core;
    [SerializeField] private TextMeshProUGUI logTemplate;
    [SerializeField, Min(1)] private int poolCount = 5;

    private readonly List<TextMeshProUGUI> logPool = new();

    private void Awake()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        if (logPool.Count > 0)
            return;

        if (core == null || logTemplate == null)
        {
            Debug.LogWarning("UI_FieldLog: Core 또는 Log Template이 연결되지 않았습니다.");
            return;
        }

        logTemplate.gameObject.SetActive(false);

        for (int i = 0; i < Mathf.Max(1, poolCount); i++)
        {
            TextMeshProUGUI newLog = Instantiate(logTemplate, core);

            newLog.name = $"FieldLog_{i}";
            newLog.SetText(string.Empty);
            newLog.gameObject.SetActive(false);

            logPool.Add(newLog);
        }
    }

    public void AddLog(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        if (logPool.Count == 0)
            InitializePool();

        if (logPool.Count == 0)
            return;

        for (int i = 0; i < logPool.Count - 1; i++)
        {
            TextMeshProUGUI current = logPool[i];
            TextMeshProUGUI next = logPool[i + 1];

            current.SetText(next.text);
            current.gameObject.SetActive(next.gameObject.activeSelf);
        }

        TextMeshProUGUI newest = logPool[logPool.Count - 1];

        newest.SetText(message);
        newest.gameObject.SetActive(true);
    }

    public void Clear()
    {
        foreach (TextMeshProUGUI log in logPool)
        {
            if (log == null)
                continue;

            log.SetText(string.Empty);
            log.gameObject.SetActive(false);
        }
    }
}
