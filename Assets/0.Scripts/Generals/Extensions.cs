using System.Collections;
using System.Threading.Tasks;
using Unity.Mathematics.Geometry;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

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

    //왼쪽 오른쪽 친구를 가지고 비교를 해서 그 결과가 bool로 나오는 형태의 함수를 Comparison(비교)
    public static T GetExtreme<T>(this IEnumerable targetList, float defaultScore, System.Func<T, float> ScoreFunction, System.Func<float, float, bool> Comparison)
    {
        T result = default; //공
        float firstScore = defaultScore;  // 차


        foreach (T currentTarget in targetList) // 공
        {
            float currntDistance = ScoreFunction(currentTarget);//공
            //Priority 거리
            if (Comparison(currntDistance, firstScore )) //차
            {
                result = currentTarget; //공
                firstScore = currntDistance; // 공
            }
            
        }
        return result; //공
    }


    public static T GetMaximum<T>(this IEnumerable targetList, System.Func<T, float> ScoreFunction)
    => targetList.GetExtreme(float.MinValue, ScoreFunction, (a, b) => a > b);

    public static T GetMinimum<T>(this IEnumerable targetList, System.Func<T, float> ScoreFunction)
    => targetList.GetExtreme(float.MaxValue, ScoreFunction, (a,b) => a < b);


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
        //�׷��� ���ƴٸ�, ���࿡ ���� �� ������ ���� ���� �� �ִ� ����
        float minSpace = absAHalf + absBHalf;
        //���� �� �� ������ �Ÿ��� �󸶳� �������!
        float distance = aPos - bPos;
        //x�ּ� �Ÿ��� �� ������ �Ÿ� ����! => ����ó��!
        float penetration = minSpace - Mathf.Abs(distance);
        //��� �������� ���� �ִ��� Ȯ���ϴ� �͵� �߿�!
        //A�� ���� => +�� �����ٱ� -�� �����ٱ�!
        //xDistanct�� ��ȣ�� �״�� ���󰡰� �Ϸ���
        // ���̳ʽ��� -1 / 0 �̻��̸� 1
        penetration *= Mathf.Sign(distance);
        return penetration;
    }

    // rect ��Ʈ ��, �� ���簢��
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

        //��ü �ʺ��� ī�޶� Ŀ�ٸ�
        bool leftOut = inMin < outMin;
        bool rightOut = inMax > outMax;

        if (leftOut ^ rightOut)
        {
           if(leftOut) result = outMin - inMin;
           if(rightOut) result = outMax - inMax;
        }

        return result;

    }

    //���� ���� ���� üũ�ϴ� ���!
    //���������� 2��ŭ �������Դٸ� (-2, 0)
    //�������� 3��ŭ �������Դٸ� (3, 0)
    //�Ʒ��� 1��ŭ �������Դٸ� (0, 1)
    //���� 1��ŭ �������Դٸ� (0, -1)
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
