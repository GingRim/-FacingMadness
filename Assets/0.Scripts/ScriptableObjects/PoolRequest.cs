using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable] // �ø��� / �������� / 
public struct PoolSetting
{
    public string poolName;
    public GameObject target;
    public uint countInitial;
    public uint coutAdditional; // �ѹ��� �߰��� ����
    public UIType uiType; //���� ��
}


    [CreateAssetMenu(fileName = "PoolRequest", menuName = "PoolRequest/DefaultPoolRequest")]
public class PoolRequest : ScriptableObject
{
    public PoolSetting[] settings;
}
