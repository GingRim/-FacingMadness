using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable] // 시리얼 / 연속적인 / 
public struct PoolSetting
{
    public string poolName;
    public GameObject target;
    public uint countInitial;
    public uint coutAdditional; // 한번에 추가할 개수
    public UIType uiType; //실험 중
}


    [CreateAssetMenu(fileName = "PoolRequest", menuName = "PoolRequest/DefaultPoolRequest")]
public class PoolRequest : ScriptableObject
{
    public PoolSetting[] settings;
}
