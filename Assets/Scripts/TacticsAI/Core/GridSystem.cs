using UnityEngine;
using System.Collections.Generic;

namespace TacticsAI.Core
{
    /// <summary>
    /// 网格系统 - 管理战旗游戏的棋盘格子
    /// </summary>
    public class GridSystem : MonoBehaviour
    {
        public static GridSystem Instance { get; private set; }

        [Header("网格设置")]
        public int gridWidth = 8;
        public int gridHeight = 8;
        public float cellSize = 1f;
        public Vector3 gridOrigin = Vector3.zero;

        // 网格数据：存储每个格子的占用状态
        private Unit[,] gridUnits;
        private bool[,] obstacles;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeGrid();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeGrid()
        {
            gridUnits = new Unit[gridWidth, gridHeight];
            obstacles = new bool[gridWidth, gridHeight];
        }

        /// <summary>
        /// 世界坐标转网格坐标
        /// </summary>
        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            int x = Mathf.FloorToInt((worldPosition.x - gridOrigin.x) / cellSize);
            int y = Mathf.FloorToInt((worldPosition.z - gridOrigin.z) / cellSize);
            return new Vector2Int(Mathf.Clamp(x, 0, gridWidth - 1), Mathf.Clamp(y, 0, gridHeight - 1));
        }

        /// <summary>
        /// 网格坐标转世界坐标（中心点）
        /// </summary>
        public Vector3 GridToWorld(Vector2Int gridPosition)
        {
            return new Vector3(
                gridOrigin.x + gridPosition.x * cellSize + cellSize / 2,
                gridOrigin.y,
                gridOrigin.z + gridPosition.y * cellSize + cellSize / 2
            );
        }

        /// <summary>
        /// 检查格子是否有效
        /// </summary>
        public bool IsValidPosition(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight;
        }

        /// <summary>
        /// 检查格子是否可通行
        /// </summary>
        public bool IsWalkable(Vector2Int pos)
        {
            if (!IsValidPosition(pos)) return false;
            return !obstacles[pos.x, pos.y] && gridUnits[pos.x, pos.y] == null;
        }

        /// <summary>
        /// 获取格子上的单位
        /// </summary>
        public Unit GetUnitAt(Vector2Int pos)
        {
            if (!IsValidPosition(pos)) return null;
            return gridUnits[pos.x, pos.y];
        }

        /// <summary>
        /// 放置单位到格子
        /// </summary>
        public void PlaceUnit(Unit unit, Vector2Int pos)
        {
            if (!IsValidPosition(pos)) return;

            // 清除旧位置
            if (unit.GridPosition != pos && IsValidPosition(unit.GridPosition))
            {
                gridUnits[unit.GridPosition.x, unit.GridPosition.y] = null;
            }

            gridUnits[pos.x, pos.y] = unit;
            unit.GridPosition = pos;
            unit.transform.position = GridToWorld(pos);
        }

        /// <summary>
        /// 移除单位
        /// </summary>
        public void RemoveUnit(Vector2Int pos)
        {
            if (!IsValidPosition(pos)) return;
            gridUnits[pos.x, pos.y] = null;
        }

        /// <summary>
        /// 设置障碍物
        /// </summary>
        public void SetObstacle(Vector2Int pos, bool isObstacle)
        {
            if (!IsValidPosition(pos)) return;
            obstacles[pos.x, pos.y] = isObstacle;
        }

        /// <summary>
        /// 计算两点间的曼哈顿距离
        /// </summary>
        public int GetManhattanDistance(Vector2Int from, Vector2Int to)
        {
            return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
        }

        /// <summary>
        /// 获取移动范围内的所有格子
        /// </summary>
        public List<Vector2Int> GetMovableCells(Vector2Int origin, int moveRange)
        {
            List<Vector2Int> result = new List<Vector2Int>();
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            Dictionary<Vector2Int, int> visited = new Dictionary<Vector2Int, int>();

            queue.Enqueue(origin);
            visited[origin] = 0;

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                int currentDist = visited[current];

                if (currentDist > 0 && IsWalkable(current))
                {
                    result.Add(current);
                }

                if (currentDist < moveRange)
                {
                    Vector2Int[] neighbors = GetNeighbors(current);
                    foreach (var neighbor in neighbors)
                    {
                        if (IsValidPosition(neighbor) && !visited.ContainsKey(neighbor) && !obstacles[neighbor.x, neighbor.y])
                        {
                            visited[neighbor] = currentDist + 1;
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取攻击范围内的格子
        /// </summary>
        public List<Vector2Int> GetAttackableCells(Vector2Int origin, int attackRange)
        {
            List<Vector2Int> result = new List<Vector2Int>();

            for (int x = -attackRange; x <= attackRange; x++)
            {
                for (int y = -attackRange; y <= attackRange; y++)
                {
                    if (x == 0 && y == 0) continue;

                    Vector2Int pos = new Vector2Int(origin.x + x, origin.y + y);
                    if (IsValidPosition(pos) && GetManhattanDistance(origin, pos) <= attackRange)
                    {
                        result.Add(pos);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取相邻格子
        /// </summary>
        public Vector2Int[] GetNeighbors(Vector2Int pos)
        {
            return new Vector2Int[]
            {
                new Vector2Int(pos.x + 1, pos.y),
                new Vector2Int(pos.x - 1, pos.y),
                new Vector2Int(pos.x, pos.y + 1),
                new Vector2Int(pos.x, pos.y - 1)
            };
        }

        /// <summary>
        /// A*寻路算法
        /// </summary>
        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
        {
            if (!IsValidPosition(start) || !IsValidPosition(end))
                return new List<Vector2Int>();

            var openSet = new List<Vector2Int> { start };
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var gScore = new Dictionary<Vector2Int, float> { [start] = 0 };
            var fScore = new Dictionary<Vector2Int, float> { [start] = GetManhattanDistance(start, end) };

            while (openSet.Count > 0)
            {
                // 找到f值最小的节点
                Vector2Int current = openSet[0];
                float lowestF = fScore.GetValueOrDefault(current, float.MaxValue);
                foreach (var node in openSet)
                {
                    float f = fScore.GetValueOrDefault(node, float.MaxValue);
                    if (f < lowestF)
                    {
                        lowestF = f;
                        current = node;
                    }
                }

                if (current == end)
                {
                    return ReconstructPath(cameFrom, current);
                }

                openSet.Remove(current);

                foreach (var neighbor in GetNeighbors(current))
                {
                    if (!IsValidPosition(neighbor)) continue;
                    if (neighbor != end && !IsWalkable(neighbor)) continue;

                    float tentativeG = gScore.GetValueOrDefault(current, float.MaxValue) + 1;

                    if (tentativeG < gScore.GetValueOrDefault(neighbor, float.MaxValue))
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        fScore[neighbor] = tentativeG + GetManhattanDistance(neighbor, end);

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }

            return new List<Vector2Int>(); // 无路径
        }

        private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
        {
            var path = new List<Vector2Int> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }
            path.RemoveAt(0); // 移除起点
            return path;
        }

        /// <summary>
        /// 可视化网格（调试用）
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.gray;
            for (int x = 0; x <= gridWidth; x++)
            {
                Vector3 start = gridOrigin + new Vector3(x * cellSize, 0, 0);
                Vector3 end = gridOrigin + new Vector3(x * cellSize, 0, gridHeight * cellSize);
                Gizmos.DrawLine(start, end);
            }
            for (int y = 0; y <= gridHeight; y++)
            {
                Vector3 start = gridOrigin + new Vector3(0, 0, y * cellSize);
                Vector3 end = gridOrigin + new Vector3(gridWidth * cellSize, 0, y * cellSize);
                Gizmos.DrawLine(start, end);
            }
        }
    }
}
