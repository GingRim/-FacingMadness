using UnityEngine;

public class MagicCardDatabase : MonoBehaviour
{
public static MagicCardDatabase Instance { get; private set; }

    [Header("자색 카드 생성 결과")]
    public CardData ForbiddenMagicCard;
    public CardData AttackMagicCard;
    public CardData DefenseMagicCard;
    public CardData BuffMagicCard;

    private void Awake()
    {
        Instance = this;
    }
}
