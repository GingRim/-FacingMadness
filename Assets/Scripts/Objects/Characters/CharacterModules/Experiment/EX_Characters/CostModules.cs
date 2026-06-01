using System;
using UnityEngine;


public class CostModules : MonoBehaviour
{
    public int Cost;
    int Max = 100;

    public void Initialize(CostType target)
    {
        setMax(target);
    }

    private void setMax(CostType target)
    {

    }
}
