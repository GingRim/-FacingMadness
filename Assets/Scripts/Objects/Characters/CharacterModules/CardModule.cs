using System;
using UnityEngine;

public class CardModule : MonoBehaviour
{ 
    /// <summary>
    /// 카드 사용 시 필요한 코스트 정보
    /// </summary>
    [Serializable]
    public struct CardCostData
    {
        // 사용하는 코스트 종류
        // Action / Auxiliary / Reaction
        public CostType costType;

        // 필요한 코스트 양
        public int amount;
    }

    /// <summary>
    /// 카드 데이터
    /// 카드의 정보와 수치를 저장한다.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCard", menuName = "Card/CardData")]
    public class CardData : ScriptableObject
    {
        [Header("기본 정보")]

        // 카드 이름
        public string cardName;

        // 카드 일러스트
        public Sprite illustration;

        [Header("카드 분류")]

        // 카드 계열(색상)
        public CardColorType color;

        // 카드 효과 태그들
        public CardTagType[] tags;

        [Header("코스트")]

        // 카드 사용 시 필요한 코스트 목록
        public CardCostData[] costs;

        [Header("수치")]

        // 공격 수치
        public int damage;

        // 회복 수치
        public int restoreAmount;

        [Header("설명")]

        // 카드 설명
        [TextArea]
        public string description;
    }

    /// <summary>
    /// 카드 사용 처리 클래스
    /// 실제 카드 사용 및 효과 적용을 담당한다.
    /// </summary>
    public class CardResolver
    {
        /// <summary>
        /// 카드 사용 가능 여부 확인
        /// </summary>
        public bool CanUse(CardData card, ControllerBase user)
        {
            CostModule costModule = user.GetModule<CostModule>();

            if (costModule == null)
                return false;

            // 필요한 코스트가 충분한지 확인
            foreach (CardCostData cost in card.costs)
            {
                if (!costModule.CanUse(cost.costType, cost.amount))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 카드 사용
        /// </summary>
        public bool Use(
            CardData card,
            ControllerBase user,
            ControllerBase target)
        {
            // 사용 불가능하면 실패
            if (!CanUse(card, user))
                return false;

            CostModule costModule = user.GetModule<CostModule>();

            // 코스트 소모
            foreach (CardCostData cost in card.costs)
            {
                costModule.Use(cost.costType, cost.amount);
            }

            // 공격 카드 처리
            if (HasTag(card, CardTagType.Attack))
            {
                DamageStruct damageInfo = new DamageStruct
                {
                    from = user.gameObject,
                    instigator = user,
                    damageAmount = card.damage,
                    critical = false,
                    damageType = DamageType.Physical
                };

                CombatModule targetCombat =
                    target.GetModule<CombatModule>();

                targetCombat.OnHit(damageInfo);
            }

            // 회복 카드 처리
            if (HasTag(card, CardTagType.Restore))
            {
                RestoreStruct restoreInfo = new RestoreStruct
                {
                    from = user.gameObject,
                    instigator = user,
                    restoreAmount = card.restoreAmount
                };

                HitpointModules targetHp =
                    target.GetModule<HitpointModules>();

                targetHp.TakeRestore(restoreInfo);
            }

            return true;
        }

        /// <summary>
        /// 카드가 특정 태그를 가지고 있는지 확인
        /// </summary>
        private bool HasTag(CardData card, CardTagType tag)
        {
            foreach (CardTagType cardTag in card.tags)
            {
                if (cardTag == tag)
                    return true;
            }

            return false;
        }
    }
}
