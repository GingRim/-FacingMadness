using System.Collections;
using UnityEngine;

public class SettingManager : ManagerBase
{
    protected override IEnumerator OnConnected(GameManager newManager)
    {
        //스마트폰의 방향은 가로 세로 역가로 역세로 가로 카메라 -> 메뉴 역가로 메뉴 -> 카메라
        Screen.autorotateToLandscapeLeft = true;      //카메라가 왼쪽
        Screen.autorotateToLandscapeRight = true;     // 카메라가 오른쪽
        Screen.autorotateToPortrait = false;           //카메라가 위쪽
        Screen.autorotateToPortraitUpsideDown = false; // 카메라가 아래쪽

        Screen.orientation = ScreenOrientation.LandscapeLeft;
        //스크린이 얼마나 오랫동안 터치가 안되면 꺼질지!
        Screen.sleepTimeout = 1000;

        yield return null;
    }

    protected override void OnDisconnected()
    {

    }
}
