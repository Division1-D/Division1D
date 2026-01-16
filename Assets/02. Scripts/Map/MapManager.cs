using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace Division.Map
{
    public class MapManager : MonoBehaviour
    {
        public static MapManager Instance; // 싱글톤 패턴 (어디서든 접근 가능)

        [Header("Settings")] public Tilemap tilemap; // 검사할 타일맵

        // 벽으로 인식할 타일 이름들
        public List<string> wallTileNames = new List<string>
        {
            "floor 1_55",
            "floor 1_111",
            "floor 1_159"
        };

        // 좌표별 벽 여부를 저장할 딕셔너리 (멀티플레이시 서버 데이터 역할)
        // Key: 타일 그리드 좌표, Value: true면 벽(이동불가)
        private Dictionary<Vector3Int, bool> obstacleMap = new Dictionary<Vector3Int, bool>();

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            ParseTilemapData();
        }

        // 게임 시작 시 타일맵 전체를 스캔하여 데이터화 합니다.
        void ParseTilemapData()
        {
            BoundsInt bounds = tilemap.cellBounds;
            TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

            for (int x = 0; x < bounds.size.x; x++)
            {
                for (int y = 0; y < bounds.size.y; y++)
                {
                    TileBase tile = allTiles[x + y * bounds.size.x];
                    if (tile != null)
                    {
                        // 타일 이름이 벽 리스트에 포함되어 있는지 확인
                        Vector3Int pos = new Vector3Int(bounds.xMin + x, bounds.yMin + y, 0);

                        // [추가됨] 모든 타일의 LockColor 플래그를 미리 해제합니다.
                        // 이제 어디서든 tilemap.SetColor(pos, Color.red)를 바로 쓸 수 있습니다.
                        tilemap.SetTileFlags(pos, TileFlags.None);

                        if (wallTileNames.Contains(tile.name))
                        {
                            obstacleMap.Add(pos, true);
                        }
                    }
                }
            }

            Debug.Log($"Map Parsing Completed. Total Walls: {obstacleMap.Count}");
        }

        // 외부에서 특정 월드 좌표로 이동 가능한지 물어보는 함수
        public bool IsWalkable(Vector3 targetWorldPos)
        {
            Vector3Int gridPos = tilemap.WorldToCell(targetWorldPos);
            if (obstacleMap.ContainsKey(gridPos) && obstacleMap[gridPos]) return false;
            return true;
        }

        // 월드 좌표를 받아 타일의 정확한 중앙 좌표를 반환하는 헬퍼 함수
        public Vector3 GetTileCenter(Vector3 worldPos)
        {
            Vector3Int cellPos = tilemap.WorldToCell(worldPos);
            return tilemap.GetCellCenterWorld(cellPos);
        }
    }
}