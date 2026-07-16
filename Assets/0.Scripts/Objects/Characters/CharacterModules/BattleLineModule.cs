using UnityEngine;

public class BattleLineModule : MonoBehaviour
{
    public class BattlePosition
    {
        public TeamType team;
        public int index; // 0~3
        public ControllerBase character;
    }

    public class BattleLine
    {
        // 전투 라인 전체
        // 0~3 : 아군 뒤 → 앞
        // 4~7 : 적군 앞 → 뒤
        private ControllerBase[] line = new ControllerBase[8];

        /*
            index 구조

            [0] 아군 4번
            [1] 아군 3번
            [2] 아군 2번
            [3] 아군 1번

            [4] 적군 1번
            [5] 적군 2번
            [6] 적군 3번
            [7] 적군 4번
        */

        /// <summary>
        /// 지정한 위치에 캐릭터 배치
        /// </summary>
        public void SetPosition(int index, ControllerBase character)
        {
            line[index] = character;
        }

        /// <summary>
        /// 캐릭터 제거
        /// </summary>
        public void RemovePosition(int index)
        {
            line[index] = null;
        }

        /// <summary>
        /// 특정 캐릭터의 위치 반환
        /// 없으면 -1 반환
        /// </summary>
        public int GetIndex(ControllerBase character)
        {
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == character)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// 두 캐릭터 사이 거리 계산
        ///
        /// 규칙:
        /// - 빈 칸(null)은 거리 증가에 포함되지 않음
        /// - 캐릭터가 있는 칸만 거리 증가
        /// - 최소 거리는 1
        /// </summary>
        public int GetDistance(ControllerBase from, ControllerBase target)
        {
            int fromIndex = GetIndex(from);
            int targetIndex = GetIndex(target);

            // 둘 중 하나라도 라인에 없으면 실패
            if (fromIndex < 0 || targetIndex < 0)
                return -1;

            // 작은 index ~ 큰 index 범위 계산
            int start = Mathf.Min(fromIndex, targetIndex);
            int end = Mathf.Max(fromIndex, targetIndex);

            int distance = 0;

            // 두 캐릭터 사이를 순회
            for (int i = start + 1; i <= end; i++)
            {
                // 캐릭터가 존재하는 칸만 거리 증가
                if (line[i] != null)
                    distance++;
            }

            // 최소 거리 1 보장
            return Mathf.Max(1, distance);
        }
    }
}
