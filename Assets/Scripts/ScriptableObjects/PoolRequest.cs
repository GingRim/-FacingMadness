using UnityEngine;

[CreateAssetMenu(fileName = "PoolRequest", menuName = "PoolRequest/DefaultPoolRequest")]
public class PoolRequest : ScriptableObject
{
    public PoolSetting[] settings;
}
