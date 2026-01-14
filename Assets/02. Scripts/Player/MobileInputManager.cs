using UnityEngine;

namespace Player
{
    public class MobileInputManager : MonoBehaviour
    {
        // 외부(플레이어 캐릭터)에서 가져갈 최종 방향 벡터
        public Vector2 InputVector { get; private set; }

        // 각 방향의 눌림 상태 저장
        private bool isUp, isDown, isLeft, isRight;

        // 화살표 버튼들이 호출하는 함수
        public void SetPressState(ArrowType type, bool isPressed)
        {
            switch (type)
            {
                case ArrowType.Up: isUp = isPressed; break;
                case ArrowType.Down: isDown = isPressed; break;
                case ArrowType.Left: isLeft = isPressed; break;
                case ArrowType.Right: isRight = isPressed; break;
            }
        }

        void Update()
        {
            // 1. 눌린 버튼에 따라 벡터 계산
            float x = 0;
            float y = 0;

            if (isRight) x += 1;
            if (isLeft) x -= 1;
            if (isUp) y += 1;
            if (isDown) y -= 1;

            // 2. 최종 벡터 생성 및 정규화 (대각선 이동 시 속도 일정하게 유지)
            // 입력이 없으면 (0,0), 있으면 길이가 1인 벡터 반환
            InputVector = new Vector2(x, y).normalized;
        }
    }
}