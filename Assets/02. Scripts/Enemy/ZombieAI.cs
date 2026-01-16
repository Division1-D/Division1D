using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Division.Map;
using Division.Player;

namespace Division.Enemy
{
    public class ZombieAI : MonoBehaviour
    {
        private ZombieAttack zombieAttack;
        
        [Header("Settings")]
        public float moveSpeed = 3f;
        public float detectionRange = 10f;
        public float attackRange = 1.1f;
        
        [Header("Wander Settings")]
        public float wanderInterval = 2.0f;
        public int wanderRadius = 3;

        [Header("Zone Settings")]
        private const int PatternRange = 3; // 7x7 패턴 범위

        private Transform playerTarget;
        public PlayerHealth playerHealth;

        private bool isMoving = false;
        private bool hasDetectedPlayer = false;
        private Vector3Int currentGridPos;

        // 방향 단축어
        private Vector3Int U = Vector3Int.up;
        private Vector3Int D = Vector3Int.down;
        private Vector3Int L = Vector3Int.left;
        private Vector3Int R = Vector3Int.right;
        private Vector3Int Z = Vector3Int.zero;

        private Vector3Int[,] movePattern;

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
            SnapToGrid();
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            zombieAttack = GetComponent<ZombieAttack>();
            if (player != null)
            {
                playerTarget = player.transform;
                playerHealth = player.GetComponent<PlayerHealth>();
            }
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
        
        // ZombieAI 클래스 안의 아무 곳(마지막 부분 등)에 추가하세요.
        int GetManhattanDistance(Vector3Int a, Vector3Int b)
        {
            // x 좌표 차이의 절댓값 + y 좌표 차이의 절댓값
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
        
        void Update()
        {
            if (playerTarget == null || (playerHealth != null && playerHealth.GetIsDead()))
            {
                StopAllCoroutines();
                isMoving = false;
                return;
            }
            
            // --- [추가된 부분] 십자 1칸 범위 체크 로직 시작 ---
            Vector3Int myGridPos = MapManager.Instance.tilemap.WorldToCell(transform.position);
            Vector3Int targetGridPos = MapManager.Instance.tilemap.WorldToCell(playerTarget.position);

            // 맨해튼 거리가 1이면 상하좌우 중 하나에 딱 붙어있다는 뜻 (대각선은 2가 됨)
            if (GetManhattanDistance(myGridPos, targetGridPos) == 1)
            {
                Debug.Log($"<color=red>공격 가능! (십자 범위 1칸)</color> Zombie: {myGridPos}, Player: {targetGridPos}");
                
                // 여기에 공격 로직(AttackCoroutine 등)을 실행하면 됩니다.
            }
            // --- [추가된 부분] 끝 ---
            
            if (isMoving) return;

            float distance = Vector3.Distance(transform.position, playerTarget.position);

            if (!hasDetectedPlayer)
            {
                if (distance <= detectionRange)
                {
                    hasDetectedPlayer = true;
                    StopAllCoroutines();
                    isMoving = false;
                }
                else
                {
                    StartCoroutine(WanderRoutine());
                }
            }
            else
            {
                if (distance > attackRange)
                {
                    StartCoroutine(MoveToPlayerRoutine());
                }
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
                yield return StartCoroutine(MoveToTarget(nextPos));
            }

            yield return new WaitForSeconds(Random.Range(1f, wanderInterval));
            isMoving = false;
        }

        IEnumerator MoveToPlayerRoutine()
        {
            isMoving = true;
            Vector3Int nextGridPos = GetNextStepTowards(playerTarget.position);

            if (nextGridPos != currentGridPos)
            {
               yield return StartCoroutine(MoveToTarget(nextGridPos));
            }
            isMoving = false;
        }

        IEnumerator MoveToTarget(Vector3Int targetGridPos)
        {
            Vector3 targetWorldPos = MapManager.Instance.GetTileCenter(new Vector3(targetGridPos.x, targetGridPos.y, 0));

            while ((targetWorldPos - transform.position).sqrMagnitude > Mathf.Epsilon)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime);
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

        // === [수정됨] A* 길찾기 (가로 이동 우선) ===
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

        // === [핵심 변경] 가로 우선 휴리스틱 함수 ===
        // 목표까지 남은 거리를 계산할 때, X축 거리에 가중치를 더 부여합니다.
        // 이렇게 하면 X축 거리를 줄이는(가로 이동) 선택이 Y축 거리를 줄이는 선택보다 F값을 더 낮추게 됩니다.
        int GetWeightedHeuristic(Vector3Int a, Vector3Int b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);

            // X축 거리 1칸 = 비용 11 (가중치 높음 -> 빨리 없애고 싶어함 -> 우선 선택)
            // Y축 거리 1칸 = 비용 10 (표준)
            // 즉, X거리가 줄어드는 쪽이 Y거리가 줄어드는 쪽보다 '예상 남은 비용'이 더 급격히 감소하므로 선호됨.
            return (dx * 11) + (dy * 10);
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