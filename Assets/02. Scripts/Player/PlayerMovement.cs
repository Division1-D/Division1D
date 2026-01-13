using UnityEngine;
using System.Collections;

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
            // 이동 중이라면 입력을 받지 않음
            if (isMoving) return;

            // 입력 받기 (상하좌우, 대각선 방지 로직 포함)
            inputVec.x = Input.GetAxisRaw("Horizontal");
            inputVec.y = Input.GetAxisRaw("Vertical");

            // 대각선 이동 방지: X축 입력이 있으면 Y축 입력 무시
            if (inputVec.x != 0) inputVec.y = 0;

            // 입력이 있을 때만 이동 로직 수행
            if (inputVec != Vector2.zero)
            {
                Vector3 targetPos = transform.position + new Vector3(inputVec.x, inputVec.y, 0);
            
                // MapManager에게 해당 위치로 갈 수 있는지 물어봄
                if (Map.MapManager.Instance.IsWalkable(targetPos))
                {
                    // 타일맵 기준 정확한 타일 중앙 좌표를 받아옴 (미세한 오차 방지)
                    Vector3 centerTarget = Map.MapManager.Instance.GetTileCenter(targetPos);
                    StartCoroutine(MoveRoutine(centerTarget));
                }
            }
        }

        IEnumerator MoveRoutine(Vector3 targetPos)
        {
            isMoving = true;

            // 현재 위치에서 목표 위치까지 부드럽게 이동
            while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
            {
                // MoveTowards를 사용하여 일정한 속도로 이동
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }

            // 이동 완료 후 위치를 정확하게 목표지점으로 고정
            transform.position = targetPos;
            isMoving = false;
        }

        // 초기화 시 위치를 타일 중앙에 맞추는 함수
        void SnapToGrid()
        {
            if (Map.MapManager.Instance != null)
            {
                transform.position = Map.MapManager.Instance.GetTileCenter(transform.position);
            }
        }
    }
   
}
