using TMPro;
using UnityEngine;


public class UI_Keyword : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentName;
    [SerializeField] private KeywordType keywordName;
    

    public KeywordType KeywordType => keywordName;


}
