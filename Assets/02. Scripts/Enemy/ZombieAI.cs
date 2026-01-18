using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Division.Map;
using Division.Player;
using UnityEditor.Rendering;

namespace Division.Enemy
{
    public class ZombieAI : MonoBehaviour
    {
        private ZombieAttack zombieAttack;
        
        [Header("Settings")]
        public float moveSpeed = 3f;

        public float wanderSpeed = 0.5f; //0일시 자리에서 랜덤움직임 없음
        public float detectionRange = 10f;
        public float attackRange = 1.1f;
        
        [Header("Wander Settings")]
        public float wanderInterval = 1f;
        public int wanderRadius = 3;

        [Header("Zone Settings")]
        private const int PatternRange = 3; // 7x7 패턴 범위

        private Transform playerTarget;
        public PlayerHealth playerHealth;

        private bool isMoving = false;
        public bool hasDetectedPlayer = false;
        private Vector3Int currentGridPos;

        // 방향 단축어
        private Vector3Int U = Vector3Int.up;
        private Vector3Int D = Vector3Int.down;
        private Vector3Int L = Vector3Int.left;
        private Vector3Int R = Vector3Int.right;
        private Vector3Int Z = Vector3Int.zero;

        private Vector3Int[,] movePattern;

        public bool isCounterableAttackMode=false;

        // A* 노드 (int형 비용 계산을 위해 수정)
        private class Node
        {
            public Vector3Int Position;
            public Node Parent;
            public int G; // 시작점으로부터의 이동 비용
            public int H; // 목표까지의 가중치 포함 예상 거리
            public int F => G + H;
            public Node(Vector3Int pos) { Position = pos; }
        }

        void Awake()
        {
            InitializeMovePattern();
        }

        void Start()
        {
            zombieAttack = GetComponent<ZombieAttack>();
            playerTarget = playerHealth.transform;
            SnapToGrid();
            StartCoroutine(ZombieMoveUpdate());
        }

        void InitializeMovePattern()
        {
            movePattern = new Vector3Int[7, 7]
            {
                {  D, D, D, D, D, D, L },
                {  R, D, D, D, D, D, L },
                {  R, R, R, D, L, L, L },
                {  R, R, R, Z, L, L, L },
                {  R, R, U, U, U, L, L },
                {  R, U, U, U, U, U, L },
                {  U, U, U, U, U, U, U }
            };
        }
        
        int CheckIfPlayerForward(Vector3Int a, Vector3Int b)
        {
            // x 좌표 차이의 절댓값 + y 좌표 차이의 절댓값
            return Mathf.Abs(b.x-a.x) + Mathf.Abs(b.y-a.y);
        }
        
        IEnumerator ZombieMoveUpdate()
        {
            WaitForSeconds wait = new WaitForSeconds(0.1f);

            while (true)
            {
                if (playerTarget == null || (playerHealth != null && playerHealth.GetIsDead()))
                {
                    isMoving = false;
                    yield return wait;
                    continue; // [수정] 죽었거나 타겟 없으면 아래 로직 실행 안 하고 다음 루프로
                }

                // [핵심 문제 해결 부분]
                if (isMoving) 
                {
                    yield return wait; // 0.1초 대기
                    continue; // [수정] 대기 후, 아래 로직을 실행하지 않고 루프의 처음(while)으로 돌아갑니다.
                }

                float distance = Vector3.Distance(transform.position, playerTarget.position);

                if (!hasDetectedPlayer)
                {
                    if (distance <= detectionRange)
                    {
                        hasDetectedPlayer = true;
                        StopAllCoroutines();
                        isMoving = false;
                        StartCoroutine(MoveToPlayerRoutine());
                    }
                    else
                    {
                        StartCoroutine(WanderRoutine());
                    }
                }
                else
                {
                    StartCoroutine(MoveToPlayerRoutine());
                }
        
                // 루프 끝에서 한 프레임 쉬거나 대기 시간을 줍니다.
                yield return null; 
            }
        }
        IEnumerator WanderRoutine()
        {
            isMoving = true;
            Vector3Int startPos = MapManager.Instance.tilemap.WorldToCell(transform.position);
            Vector3Int[] dirs = { U, D, L, R };
            Vector3Int randomDir = dirs[Random.Range(0, dirs.Length)];
            Vector3Int nextPos = startPos + randomDir;

            if (IsWalkable(nextPos))
            {
                yield return StartCoroutine(MoveToTarget(nextPos, wanderSpeed));
            }

            yield return null;//new WaitForSeconds(Random.Range(0.5f, wanderInterval));
            isMoving = false;
        }

        IEnumerator MoveToPlayerRoutine()
        {
            isMoving = true;

            // 플레이어를 추적하는 동안 계속 루프
            while (playerTarget != null && !playerHealth.GetIsDead())
            {
                // 1. [공격 판정] 현재 타일 중앙에 서 있는 상태에서 공격 가능한지 체크
                if (TryCheckAndAttack())
                {
                    // 공격이 시작되었다면, 공격이 끝날 때까지(isCounterableAttackMode가 false가 될 때까지) 대기
                    while (isCounterableAttackMode)
                    {
                        yield return null;
                    }
                    
                    // 공격이 끝나면 바로 다시 상황 판단 (루프 처음으로)
                    continue; 
                }

                // 2. [이동 계산] 공격 불가능하면 다음 칸으로 이동
                Vector3Int nextGridPos = GetNextStepTowards(playerTarget.position);

                if (nextGridPos != currentGridPos)
                {
                    // 한 칸 이동을 수행하고 완료될 때까지 대기
                    // 이 함수가 끝나면 좀비는 정확히 nextGridPos 타일의 중앙에 위치함
                    yield return StartCoroutine(MoveToTarget(nextGridPos, moveSpeed));
                }
                else
                {
                    // 갈 곳이 없다면 잠시 대기
                    yield return null;
                }
            }

            isMoving = false;
        }
        // 중앙에 있을 때 호출되는 공격 판정 함수
        bool TryCheckAndAttack()
        {
            if (!zombieAttack.canCounterableAttack) return false;
            Vector3Int myGridPos = MapManager.Instance.tilemap.WorldToCell(transform.position);
            Vector3Int targetGridPos = MapManager.Instance.tilemap.WorldToCell(playerTarget.position);

            // 자기가 보는방향 바로 앞에 플레이어가 있으면 공격 시도
            if (CheckIfPlayerForward(myGridPos, targetGridPos) == 1)
            {
                Vector3Int attackDir = targetGridPos - myGridPos;

                Debug.Log($"<color=red>중앙 도착 후 공격 시도! Zombie: {myGridPos}</color>");

                // 공격 상태로 전환
                isCounterableAttackMode = true;
                zombieAttack.TryCounterableAttack(attackDir);
                
                return true; // 공격 시작함
            }
            return false; // 공격 안 함
        }
        IEnumerator MoveToTarget(Vector3Int targetGridPos, float speed)
        {
            Vector3 targetWorldPos = MapManager.Instance.GetTileCenter(new Vector3(targetGridPos.x, targetGridPos.y, 0));

            while ((targetWorldPos - transform.position).sqrMagnitude > Mathf.Epsilon)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, speed * Time.deltaTime);
                yield return null;
            }

            transform.position = targetWorldPos;
            currentGridPos = targetGridPos;
        }

        Vector3Int GetNextStepTowards(Vector3 targetWorldPos)
        {
            Vector3Int startPos = MapManager.Instance.tilemap.WorldToCell(transform.position);
            Vector3Int targetPos = MapManager.Instance.tilemap.WorldToCell(targetWorldPos);

            int offsetX = startPos.x - targetPos.x;
            int offsetY = startPos.y - targetPos.y;

            // 1. 근접 패턴 범위 (7x7) -> 패턴 이동
            if (Mathf.Abs(offsetX) <= PatternRange && Mathf.Abs(offsetY) <= PatternRange)
            {
                int colIndex = offsetX + 3;
                int rowIndex = 3 - offsetY;

                Vector3Int direction = movePattern[rowIndex, colIndex];
                Vector3Int nextStep = startPos + direction;

                if (IsWalkable(nextStep)) return nextStep;
                else return GetAStarNextStep(startPos, targetPos); // 패턴이 막히면 A*
            }
            // 2. 원거리 -> A* (가로 우선)
            else
            {
                return GetAStarNextStep(startPos, targetPos);
            }
        }

        // === A* 길찾기 (가로 이동 우선) ===
        Vector3Int GetAStarNextStep(Vector3Int start, Vector3Int target)
        {
            if (start == target) return start;

            List<Node> openSet = new List<Node>();
            HashSet<Vector3Int> closedSet = new HashSet<Vector3Int>();

            // 시작 노드 생성
            Node startNode = new Node(start) { G = 0, H = GetWeightedHeuristic(start, target) };
            openSet.Add(startNode);

            int loopLimit = 500; 

            while (openSet.Count > 0 && loopLimit > 0)
            {
                loopLimit--;
                
                // F값이 가장 낮은 노드 선택 (F가 같다면 H가 낮은 것 = 목표에 더 가까운 것)
                openSet.Sort((a, b) => a.F.CompareTo(b.F));
                Node currentNode = openSet[0];
                openSet.RemoveAt(0);
                closedSet.Add(currentNode.Position);

                // 목표 도착
                if (currentNode.Position == target) return RetracePath(startNode, currentNode);

                // 인접 타일 탐색
                // (탐색 순서도 가로(Left, Right)를 먼저 넣어주면 미세하게 더 유리)
                Vector3Int[] neighbors = { L, R, U, D };

                foreach (Vector3Int dir in neighbors)
                {
                    Vector3Int neighborPos = currentNode.Position + dir;
                    if (closedSet.Contains(neighborPos) || !IsWalkable(neighborPos)) continue;

                    // 이동 비용: 1칸당 10 (정수 계산을 위해 1.0 대신 10 사용)
                    int newCost = currentNode.G + 10;
                    
                    Node neighborNode = openSet.Find(n => n.Position == neighborPos);
                    if (neighborNode == null || newCost < neighborNode.G)
                    {
                        if (neighborNode == null) 
                        { 
                            neighborNode = new Node(neighborPos); 
                            openSet.Add(neighborNode); 
                        }
                        
                        neighborNode.G = newCost;
                        // [핵심] 가중치가 적용된 H값 계산
                        neighborNode.H = GetWeightedHeuristic(neighborPos, target);
                        neighborNode.Parent = currentNode;
                    }
                }
            }
            return start; // 경로 없음
        }

       
        // === 동적 가중치 휴리스틱 함수 ===
        int GetWeightedHeuristic(Vector3Int a, Vector3Int b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);

            // 거리가 더 많이 남은 축을 우선적으로 줄이도록 가중치를 부여합니다.
            // X축 거리가 Y축 거리보다 크거나 같으면 -> X축 이동 우선 (dx에 높은 가중치)
            if (dx >= dy)
            {
                return (dx * 11) + (dy * 10);
            }
            // Y축 거리가 더 크면 -> Y축 이동 우선 (dy에 높은 가중치)
            else
            {
                return (dx * 10) + (dy * 11);
            }
        }

        Vector3Int RetracePath(Node startNode, Node endNode)
        {
            Node currentNode = endNode;
            while (currentNode.Parent != null && currentNode.Parent != startNode)
            {
                currentNode = currentNode.Parent;
            }
            return currentNode.Position;
        }

        bool IsWalkable(Vector3Int cellPos)
        {
            return MapManager.Instance.IsWalkable(MapManager.Instance.tilemap.CellToWorld(cellPos));
        }

        void SnapToGrid()
        {
            if (MapManager.Instance != null)
            {
                transform.position = MapManager.Instance.GetTileCenter(transform.position);
                currentGridPos = MapManager.Instance.tilemap.WorldToCell(transform.position);
            }
        }
    }
}