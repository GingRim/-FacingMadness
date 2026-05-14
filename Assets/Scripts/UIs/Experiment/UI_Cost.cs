using TMPro;
using UnityEngine;


public class UI_Cost : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentName;
    [SerializeField] private CostType costName;
    [SerializeField] private CostModule costModule;

    public CostType CostType => costName;
    public void Refresh()
    {
        int current = costModule.GetCurrent(costName);

        currentName.text = current.ToString();
    }

}
