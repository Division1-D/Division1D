using UnityEngine;
using System.Collections;
using Map;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Settings")]
        public float moveSpeed = 5f; // 이동 속도

        private bool isMoving = false; // 현재 이동 중인지 체크
        private Vector2 inputVec;

        void Start()
        {
            // 시작 시 플레이어를 현재 위치의 타일 중앙으로 강제 정렬
            SnapToGrid();
        }

        void Update()
        {
            // 1. 이동 중이어도 입력값은 계속 갱신합니다. (Return 제거)
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");

            // 대각선 이동 방지: 좌우 입력이 있으면 상하 입력 무시
            if (x != 0) y = 0;

            inputVec = new Vector2(x, y);

            // 2. 이동 중이 아닐 때만 코루틴을 시작합니다.
            if (!isMoving && inputVec != Vector2.zero)
            {
                StartCoroutine(MoveRoutine());
            }
        }

        IEnumerator MoveRoutine()
        {
            isMoving = true;

            // 3. 입력이 있고 + 이동 가능한 동안 계속 반복 (연속 이동 로직)
            while (inputVec != Vector2.zero)
            {
                // 현재 입력 기준으로 목표 위치 계산
                Vector3 targetPos = transform.position + new Vector3(inputVec.x, inputVec.y, 0);

                // 갈 수 있는 곳인지 체크
                if (MapManager.Instance.IsWalkable(targetPos))
                {
                    // 정확한 타일 중앙 좌표 가져오기
                    Vector3 centerTarget = MapManager.Instance.GetTileCenter(targetPos);

                    // 한 칸 이동 수행
                    while ((centerTarget - transform.position).sqrMagnitude > Mathf.Epsilon)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, centerTarget, moveSpeed * Time.deltaTime);
                        yield return null; // 1프레임 대기
                    }

                    // 이동 완료 후 위치 보정
                    transform.position = centerTarget;
                }
                else
                {
                    // 벽이라면 루프를 빠져나가고 이동 종료 (멈춤)
                    break; 
                }
                
                // 루프의 끝에서 다음 프레임 입력을 확인하지 않고 
                // while문 조건(inputVec != 0)으로 돌아가 바로 다음 이동을 시작합니다.
            }

            isMoving = false;
        }

        void SnapToGrid()
        {
            if (MapManager.Instance != null)
            {
                transform.position = MapManager.Instance.GetTileCenter(transform.position);
            }
        }
    }
}