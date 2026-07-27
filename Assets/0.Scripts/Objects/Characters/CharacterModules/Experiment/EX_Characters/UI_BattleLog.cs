using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_BattleLog : MonoBehaviour
{
    [Header("Log Pool")]
    [SerializeField] private Transform core;
    [SerializeField] private TextMeshProUGUI logTemplate;
    [SerializeField, Min(1)] private int poolCount = 3;

    private readonly List<TextMeshProUGUI> logPool = new();

    private void Awake()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        if (logPool.Count > 0)
            return;

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

        int createCount = Mathf.Max(1, poolCount);

        // 원본은 복제용이므로 화면에서 숨김
        logTemplate.gameObject.SetActive(false);
        

        for (int i = 0; i < createCount; i++)
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
       
        // Awake보다 먼저 로그 요청이 들어온 경우 대비
        if (logPool.Count == 0)
        {
            InitializePool();
        }

        // 초기화에 실패했거나 생성 개수가 0인 경우
        if (logPool.Count == 0)
        {
            Debug.LogWarning(
                "UI_BattleLog: 사용할 로그 슬롯이 없습니다.");
            return;
        }

        for (int i = 0; i < logPool.Count -1; i++)
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
