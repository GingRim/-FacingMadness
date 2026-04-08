using UnityEngine;
using UnityEngine.UI;

public class UI_LoadingSceen : UI_ScreenBase, IOpenable, IProgress<int>, IStatus<string>
{
    public bool IsOpen => gameObject.activeSelf;

    public int Current {  get; protected set; }
    public int Max { get; protected set; }
    public float Progress => Max != 0 ? (float)Current / Max : 0.0f;

    public int AddCurrent(int value) => Set(Current + value, Max);

    public int AddMax(int value) => Set(Current, Max + value);

    public void Close() => gameObject.SetActive(false);
    
    public void Open() => gameObject.SetActive(true);


    // 함수는 함수끼리\
    // 프로퍼티는 프로퍼티끼리
    // 변수는 변수끼리
    // 변수는 크기가 큰 순서에서 작은 순서로 배치


    public UnityEngine.UI.Slider progressBar;
    public TMPro.TextMeshProUGUI progressText;
    public TMPro.TextMeshProUGUI explainText;
    // IStatus
    public string SetCurrentStatus(string newText)
    {
        explainText.SetText(newText);
        return newText;
    }

    public int Set(int newCurrent)
    {
        Current = Mathf.Min(newCurrent, Max);
        progressBar.value = Progress;
        //Format String(폴멧 스트링) => 서식
        progressText.SetText($"{Progress * 100.0f : 0.00}%");
        return Current;
    }

    public int Set(int newCurrent, int newMax)
    {
        Max = newMax;
        return Set(newCurrent);
    }

    public void Toggle() => gameObject.SetActive(!IsOpen);


}
