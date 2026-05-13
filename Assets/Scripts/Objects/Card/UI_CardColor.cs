using UnityEngine;
using UnityEngine.UI;

public class UI_CardColor : MonoBehaviour
{
    [SerializeField] private Image targetImage;

    public void SetColor(CardColorType type)
    {
        targetImage.color = GetColor(type);
    }

    private Color GetColor(CardColorType type)
    {
        switch (type)
        {
            case CardColorType.Red:
                return Color.red;

            case CardColorType.Yellow:
                return Color.yellow;

            case CardColorType.Green:
                return Color.green;

            case CardColorType.Blue:
                return Color.blue;

            case CardColorType.Purple:
                return new Color(0.6f, 0.2f, 1f);

            case CardColorType.Colorless:
                return Color.gray;

            default:
                return Color.white;
        }
    }
}