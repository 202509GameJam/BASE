using UnityEngine;

namespace TacticsAI.Core
{
    /// <summary>
    /// 战旗游戏场景快速设置脚本
    /// 挂载到空物体上，运行时自动创建游戏所需的所有组件
    /// </summary>
    public class TacticsSceneSetup : MonoBehaviour
    {
        [Header("场景设置")]
        public bool autoSetupOnStart = true;
        public AIType defaultAIType = AIType.FSM;

        [Header("网格设置")]
        public int gridWidth = 8;
        public int gridHeight = 8;
        public float cellSize = 1f;

        [Header("单位设置")]
        public bool createTestUnits = true;

        [Header("玩家单位")]
        public Vector2Int playerMeleeStart = new Vector2Int(1, 3);
        public Vector2Int playerRangedStart = new Vector2Int(1, 4);

        [Header("敌方单位")]
        public Vector2Int enemyMeleeStart = new Vector2Int(6, 3);
        public Vector2Int enemyRangedStart = new Vector2Int(6, 4);

        [Header("单位属性预设")]
        public int meleeHealth = 120;
        public int meleeAttack = 25;
        public int meleeMoveRange = 3;
        public int meleeAttackRange = 1;

        public int rangedHealth = 80;
        public int rangedAttack = 20;
        public int rangedMoveRange = 2;
        public int rangedAttackRange = 3;

        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupScene();
            }
        }

        /// <summary>
        /// 设置整个游戏场景
        /// </summary>
        [ContextMenu("Setup Scene")]
        public void SetupScene()
        {
            SetupGrid();
            SetupGameManager();
            SetupAIComparison();

            if (createTestUnits)
            {
                CreateTestUnits();
            }

            Debug.Log("=== 战旗游戏场景设置完成 ===");
            Debug.Log($"网格大小: {gridWidth}x{gridHeight}");
            Debug.Log($"AI类型: {defaultAIType}");
        }

        /// <summary>
        /// 设置网格系统
        /// </summary>
        private void SetupGrid()
        {
            if (GridSystem.Instance != null)
            {
                Debug.Log("网格系统已存在");
                return;
            }

            GameObject gridObj = new GameObject("GridSystem");
            GridSystem grid = gridObj.AddComponent<GridSystem>();
            grid.gridWidth = gridWidth;
            grid.gridHeight = gridHeight;
            grid.cellSize = cellSize;
            grid.gridOrigin = Vector3.zero;

            Debug.Log($"创建网格系统: {gridWidth}x{gridHeight}");
        }

        /// <summary>
        /// 设置游戏管理器
        /// </summary>
        private void SetupGameManager()
        {
            if (TacticsGameManager.Instance != null)
            {
                Debug.Log("游戏管理器已存在");
                return;
            }

            GameObject managerObj = new GameObject("TacticsGameManager");
            TacticsGameManager manager = managerObj.AddComponent<TacticsGameManager>();
            manager.aiActionDelay = 0.5f;

            Debug.Log("创建游戏管理器");
        }

        /// <summary>
        /// 设置AI比较控制器
        /// </summary>
        private void SetupAIComparison()
        {
            if (AIComparisonController.Instance != null)
            {
                Debug.Log("AI比较控制器已存在");
                return;
            }

            GameObject aiObj = new GameObject("AIComparisonController");
            AIComparisonController ai = aiObj.AddComponent<AIComparisonController>();
            ai.currentAIType = defaultAIType;

            Debug.Log($"创建AI比较控制器，默认类型: {defaultAIType}");
        }

        /// <summary>
        /// 创建测试单位
        /// </summary>
        private void CreateTestUnits()
        {
            // 玩家近战单位
            CreateUnit("玩家战士", UnitType.Melee, Team.Player, playerMeleeStart,
                meleeHealth, meleeAttack, meleeMoveRange, meleeAttackRange, Color.blue);

            // 玩家远程单位
            CreateUnit("玩家弓箭手", UnitType.Ranged, Team.Player, playerRangedStart,
                rangedHealth, rangedAttack, rangedMoveRange, rangedAttackRange, Color.cyan);

            // 敌方近战单位
            CreateUnit("敌方战士", UnitType.Melee, Team.Enemy, enemyMeleeStart,
                meleeHealth, meleeAttack, meleeMoveRange, meleeAttackRange, Color.red);

            // 敌方远程单位
            CreateUnit("敌方弓箭手", UnitType.Ranged, Team.Enemy, enemyRangedStart,
                rangedHealth, rangedAttack, rangedMoveRange, rangedAttackRange, Color.magenta);

            Debug.Log("创建测试单位完成");
        }

        /// <summary>
        /// 创建单位
        /// </summary>
        private Unit CreateUnit(string name, UnitType type, Team team, Vector2Int gridPos,
            int health, int attack, int moveRange, int attackRange, Color color)
        {
            // 创建简单的可视化物体（立方体）
            GameObject unitObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            unitObj.name = name;
            unitObj.transform.localScale = new Vector3(0.8f, 1f, 0.8f);

            // 设置颜色
            Renderer renderer = unitObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }

            // 添加Unit组件
            Unit unit = unitObj.AddComponent<Unit>();
            unit.unitName = name;
            unit.unitType = type;
            unit.team = team;
            unit.maxHealth = health;
            unit.currentHealth = health;
            unit.attackPower = attack;
            unit.moveRange = moveRange;
            unit.attackRange = attackRange;

            // 设置位置
            if (GridSystem.Instance != null)
            {
                unit.GridPosition = gridPos;
                unitObj.transform.position = GridSystem.Instance.GridToWorld(gridPos) + Vector3.up * 0.5f;
            }

            // 配置AI（仅为敌方单位，或自动战斗时为所有单位）
            if (team == Team.Enemy || TacticsGameManager.Instance?.useAutoBattle == true)
            {
                AIComparisonController.Instance?.SetupUnitAI(unit);
            }

            Debug.Log($"创建单位: {name} @ ({gridPos.x}, {gridPos.y})");
            return unit;
        }

        /// <summary>
        /// 在编辑器中可视化网格
        /// </summary>
        private void OnDrawGizmos()
        {
            // 绘制网格
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            for (int x = 0; x <= gridWidth; x++)
            {
                Vector3 start = new Vector3(x * cellSize, 0, 0);
                Vector3 end = new Vector3(x * cellSize, 0, gridHeight * cellSize);
                Gizmos.DrawLine(start, end);
            }
            for (int y = 0; y <= gridHeight; y++)
            {
                Vector3 start = new Vector3(0, 0, y * cellSize);
                Vector3 end = new Vector3(gridWidth * cellSize, 0, y * cellSize);
                Gizmos.DrawLine(start, end);
            }

            // 绘制单位起始位置
            float halfCell = cellSize / 2;

            // 玩家单位（蓝色）
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(new Vector3(playerMeleeStart.x * cellSize + halfCell, 0.5f, playerMeleeStart.y * cellSize + halfCell), Vector3.one * 0.8f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(new Vector3(playerRangedStart.x * cellSize + halfCell, 0.5f, playerRangedStart.y * cellSize + halfCell), Vector3.one * 0.8f);

            // 敌方单位（红色）
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(new Vector3(enemyMeleeStart.x * cellSize + halfCell, 0.5f, enemyMeleeStart.y * cellSize + halfCell), Vector3.one * 0.8f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(new Vector3(enemyRangedStart.x * cellSize + halfCell, 0.5f, enemyRangedStart.y * cellSize + halfCell), Vector3.one * 0.8f);
        }
    }
}
