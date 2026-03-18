using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Rendering.DebugUI;

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
}
