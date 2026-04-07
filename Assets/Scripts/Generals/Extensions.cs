using System.Collections;
using System.Threading.Tasks;
using Unity.Mathematics.Geometry;
using Unity.VisualScripting;
using UnityEngine;

public static class Extensions
{


    public static float normalized(float targer)
    {
        if(targer > 0) return 1.0f;
        else if (targer < 0) return -1.0f;
        else return 0.0f;
    }

    public static T TryAddComponent<T> (this GameObject target) where T : Component
    {
        T result = null;
        if (target == null) return result;
        result = target.GetComponent<T>() ?? target.AddComponent<T>();
       
        return result;
    }

    public static T TryAddComponent<T>(this Component target) where T : Component
    {
        T result = null;
        if (target == null) return result;
        result = target.GetComponent<T>() ?? target.gameObject.TryAddComponent<T>();

        return result;
    }

    public static IEnumerator WaitForTask(this Task targetTask)
    {
        yield return new WaitUntil(() => targetTask.IsCompleted);
        targetTask.Dispose();
    }

    public static float GetPenetratedDistance(float aHalf, float bHalf, float aPos, float bPos)
    {
        float absAHalf = Mathf.Abs(aHalf);
        float absBHalf = Mathf.Abs(bHalf);
        //그래서 겹쳤다면, 만약에 원래 안 겹쳤을 때에 있을 수 있는 공간
        float minSpace = absAHalf + absBHalf;
        //지금 이 둘 사이의 거리가 얼마나 가까운지!
        float distance = aPos - bPos;
        //x최소 거리와 둘 사이의 거리 차이! => 예외처리!
        float penetration = minSpace - Mathf.Abs(distance);
        //어느 방향으로 묻혀 있는지 확인하는 것도 중요!
        //A가 왼쪽 => +로 보여줄까 -로 보여줄까!
        //xDistanct의 부호를 그대로 따라가게 하려면
        // 마이너스면 -1 / 0 이상이면 1
        penetration *= Mathf.Sign(distance);
        return penetration;
    }

    // rect 렉트 직, 정 직사각형
    public static Vector2 AABB(this Rect A, Rect B)
    {
        Vector2 result = Vector2.zero;
        Vector2 aMin = A.min;
        Vector2 aMax = A.max;
        Vector2 aHalf = A.size * 0.5f;
        Vector2 bMin = B.min;
        Vector2 bMax = B.max;
        Vector2 bHalf = B.size * 0.5f;

        if (aMax.x > bMin.x && bMax.x > aMin.x)
        {
            result.x = GetPenetratedDistance(aHalf.x, bHalf.x, A.position.x, B.position.x);
        }

        if (aMax.y > bMin.y && bMax.y > aMin.y)
        {
            result.y = GetPenetratedDistance(aHalf.y, bHalf.y, A.position.y, B.position.y);
        }

        return result;

    }

    public static float GetOutboundDistance(float inMin, float outMin, float inMax, float outMax)
    {
        float result = 0.0f;

        //전체 맵보다 카메라가 커다면
        bool leftOut = inMin < outMin;
        bool rightOut = inMax > outMax;

        if (leftOut ^ rightOut)
        {
           if(leftOut) result = outMin - inMin;
           if(rightOut) result = outMax - inMax;
        }

        return result;

    }

    //삐져 나온 양을 체크하는 방법!
    //오른쪽으로 2만큼 빠져나왔다면 (-2, 0)
    //왼쪽으로 3만큼 빠저나왔다면 (3, 0)
    //아래로 1만큼 바져나왔다면 (0, 1)
    //위로 1만큼 빠져나왔다면 (0, -1)
    public static Vector2 InversedAABB(this Rect target, Rect bound)
    {
        Vector2 result;

       result.x = GetOutboundDistance(target.xMin, bound.xMin, target.xMax ,bound.xMax);
       result.y = GetOutboundDistance(target.yMin, bound.yMin, target.yMax ,bound.yMax);

        return result;
    }

}
// 0.5(1) 2.25(2)
// 2.25 - 0.5 = 1.75
