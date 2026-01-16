using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Division.Map;    // MapManager 접근용
using Division.Player;
using UnityEngine.UI; // PlayerHealth 접근용

namespace Division.Enemy
{
    public class ZombieAttack : MonoBehaviour
    {
        [Header("Attack Settings")]
        public int attackRangeN = 3;   // 앞 N칸
        public float castTime = 1.0f;  // 공격 준비 시간 (1초)
        public int damageAmount = 10;  // 데미지

        private ZombieAI zombieAI;
        private Tilemap tilemap;
        private bool isAttacking = false;

        public bool canCounterableAttack = true;
        private float attackCooltime = 8.0f;
        
        SpriteRenderer zombieRender; //나중에 따로 코드 만들어서 분리하기

        public int GetDamage()
        {
            return damageAmount;
        }
        
        private void Start()
        {
            zombieAI = GetComponent<ZombieAI>();
            zombieRender=transform.GetChild(0).GetComponent<SpriteRenderer>();
            // MapManager가 초기화된 후 실행되도록 Start에서 할당
            if (MapManager.Instance != null)
            {
                tilemap = MapManager.Instance.tilemap;
            }
        }

        /// <summary>
        /// 외부(ZombieAI)에서 호출하는 공격 시작 함수
        /// </summary>
        /// <param name="direction">공격할 방향 (Vector3Int.up/down/left/right)</param>
        // 공격 시작 부분
        public void TryCounterableAttack(Vector3Int direction)
        {
           // if(canCounterableAttack)
            StartCoroutine(AttackRoutine(direction));
        }

        IEnumerator AttackRoutine(Vector3Int direction)
        {
            isAttacking = true;
            
            // 1. 좀비 AI의 움직임을 멈춤 (ZombieAI쪽에 StopMoving 함수가 필요하거나 코루틴 정지)
            //zombieAI.StopAllCoroutines(); 

            // 2. 공격할 타일 좌표들 계산
            Vector3Int startPos = tilemap.WorldToCell(transform.position);
            List<Vector3Int> attackTiles = new List<Vector3Int>();

            for (int i = 1; i <= attackRangeN; i++)
            {
                Vector3Int targetPos = startPos + (direction * i);
                
                // 맵 밖이나 벽도 범위 표시 다 함
                // 벽 뒤는 공격 안하려면 MapManager.IsWalkable 체크 추가 가능
                attackTiles.Add(targetPos);
            }

            // 3. 타일 색상을 빨갛게 변경 (경고 표시)
            // 1. 앞쪽 3칸의 '중앙 좌표(Vector3)'들을 가져옴
            //List<Vector3> targetCenters = GetForwardTileCenters(direction, 3);

            foreach (Vector3 center in attackTiles)
            {
                // 중앙 좌표를 다시 그리드 좌표로 변환하여 색칠 (기존 로직 활용)
                Vector3Int gridPos = MapManager.Instance.tilemap.WorldToCell(center);

                if (MapManager.Instance.tilemap.HasTile(gridPos))
                {
                    MapManager.Instance.tilemap.SetColor(gridPos, Color.red);
                }

                // 디버그용: 씬 뷰에서 위치 확인 (노란 공으로 표시)
                Debug.DrawRay(center, Vector3.up, Color.yellow, 1.0f);
            }

            // --- [카운터 가능 타이밍 시작] ---
            // 여기에 "삐빅" 사운드 재생 코드 추가
            Debug.Log("공격 예고! (1초 대기)");
            zombieRender.color = Color.red;
            yield return new WaitForSeconds(castTime);

            // --- [카운터 가능 타이밍 끝] ---
            
            // 5. 타일 색상 원상복구
            foreach (var pos in attackTiles)
            {
                if (tilemap.HasTile(pos))
                {
                    tilemap.SetColor(pos, Color.white);
                }
            }
            yield return new WaitForSeconds(0.25f);
            // 4. 1초 뒤 피격 판정
            // 플레이어를 태그로 찾거나 MapManager 등을 통해 현재 위치를 가져옴
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector3Int playerPos = tilemap.WorldToCell(player.transform.position);

                // 플레이어가 공격 범위 타일 안에 있는가?
                if (attackTiles.Contains(playerPos))
                {
                    // 플레이어 체력 감소 로직
                    var hp = player.transform.parent.GetComponent<PlayerHealth>();
                    if (hp != null)
                    {
                        Debug.Log($"<color=red>플레이어 피격! 좌표: {playerPos}</color>");
                        hp.TakeDamage(damageAmount); // PlayerHealth에 구현된 함수 호출
                        Debug.Log("데미지 적용됨");
                    }
                }
                else
                {
                    Debug.Log("플레이어가 공격 범위를 피했습니다.");
                }

                
            }
            
            zombieRender.color = Color.white;
            // 6. 공격 종료 후 딜레이를 조금 줄지, 바로 움직일지 결정
            yield return new WaitForSeconds(1f); // 후딜레이
            
            isAttacking = false;
            
            // 7. 좀비 움직임 AI 다시 가동 
            zombieAI.isCounterableAttackMode = false;
            
            //다시 카운터 공격 쿨타임 기다림
            StartCoroutine(WaitForCounterableAttackCooltime());
        }

        IEnumerator WaitForCounterableAttackCooltime()
        {
            canCounterableAttack = false;
            yield return new WaitForSeconds(attackCooltime);
            canCounterableAttack = true;
        }
        
        
        
        public List<Vector3> GetForwardTileCenters(Vector3Int direction, int range = 3)
        {
            List<Vector3> centers = new List<Vector3>();

            // 1. 좀비의 현재 그리드 좌표 계산
            Vector3Int startGridPos = MapManager.Instance.tilemap.WorldToCell(transform.position);

            // 2. 앞쪽으로 range(3)칸 만큼 반복
            for (int i = 1; i <= range; i++)
            {
                // 방향 * 거리만큼 더해서 목표 그리드 좌표 계산
                // 예: 오른쪽(1,0) 방향이면 -> (x+1, y), (x+2, y), (x+3, y)
                Vector3Int targetGridPos = startGridPos + (direction * i);

                // 3. 그리드 좌표를 타일의 정확한 '월드 중앙 좌표'로 변환
                // MapManager의 tilemap을 직접 사용하여 가져옵니다.
                Vector3 centerPos = MapManager.Instance.tilemap.GetCellCenterWorld(targetGridPos);
        
                centers.Add(centerPos);
            }

            return centers;
        }
        
    }
}