using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class DataManager : ManagerBase
{

    //프로퍼티는 변수모양이지만 함수
    // int GetLoadCount();

    public int LoadCount => 100;
    protected override IEnumerator OnConnected(GameManager newManager)
    {

        UIBase loading = UIManager.GetUIM2(UIType.Loading);
        IProgress<int> progressUI = loading as IProgress<int>;
        IStatus<string> statusUI = loading as IStatus<string>;



        // 로딩 진행율 => 최대 몇 개인지, 현재 몇 개까지 했는지
        //               현재 / 최대    1 / 100 = 0.01
        for (int i = 0; i < LoadCount; i++) 
        {
            progressUI.AddCurrent(1);
            statusUI.SetCurrentStatus($"Lod Data({i + 1}/{LoadCount})");
            yield return new WaitForSeconds(0.5f);
        }
        yield return null;
    }

    protected override void OnDisconnected()
    {

    }

    bool TryGetFileFromResources<T>(string path, out T result) where T : Object
    {
       result = Resources.Load<T>(path);
       return result != null;
    }

    //Asset Bundle (에셋 번들) (임의로 지정한 카테고리)
    //DLC => 특정 카테고리에 있는 요소를 다운로드 하게 할 것인가 말 것인가?
    //Addressable(어드렛써블)
    bool TryGetFileFromAssetBundle()
    {
        return false;
    }
}
