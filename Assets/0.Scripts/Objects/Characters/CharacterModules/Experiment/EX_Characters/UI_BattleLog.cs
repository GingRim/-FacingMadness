using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_BattleLog : MonoBehaviour
{
    [Header("Log Pool")]
    [SerializeField] private Transform core;
    [SerializeField] private TextMeshProUGUI logTemplate;
    [SerializeField, Min(1)] private int poolCount = 5;

    private readonly List<TextMeshProUGUI> logPool = new();
    public int damageAmount;

    public int diceValue;

    public bool hasAbilityModifier;
    public int abilityModifier;

    public int weightModifier;
    public int armorReduction;

    public int finalDamage;

    private void Awake()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        if (core == null)
        {
            Debug.LogWarning(
                "UI_BattleLog: Core가 연결되지 않았습니다.");
            return;
        }

        if (logTemplate == null)
        {
            Debug.LogWarning(
                "UI_BattleLog: Log Template이 연결되지 않았습니다.");
            return;
        }

        logPool.Clear();

        // 원본은 복제용이므로 화면에서 숨김
        logTemplate.gameObject.SetActive(false);

        for (int i = 0; i < poolCount; i++)
        {
            TextMeshProUGUI newLog = Instantiate(logTemplate, core);

            newLog.name = $"Log_{i}";
            newLog.SetText(string.Empty);
            newLog.gameObject.SetActive(false);

            logPool.Add(newLog);
        }
    }

    public void AddLog(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        if (logPool.Count == 0)
            return;


        for (int i = 0; i < logPool.Count; i++)
        {
            TextMeshProUGUI current = logPool[i];
            TextMeshProUGUI next = logPool[i + 1];

            current.SetText(next.text);
            current.gameObject.SetActive(next.gameObject.activeSelf);
        }

        // 마지막 칸에 최신 로그 표시
        TextMeshProUGUI newestLog = logPool[logPool.Count - 1];

        newestLog.SetText(message);
        newestLog.gameObject.SetActive(true);
    }


    public void Clear()
    {
        foreach (TextMeshProUGUI log in logPool)
        {
            log.SetText(string.Empty);
            log.gameObject.SetActive(false);
        }
    }
}
