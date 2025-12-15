using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TacticsAI.Core
{
    /// <summary>
    /// 游戏阶段枚举
    /// </summary>
    public enum GamePhase
    {
        PlayerTurn,
        EnemyTurn,
        GameOver
    }

    /// <summary>
    /// 战旗游戏管理器 - 管理回合制战斗流程
    /// </summary>
    public class TacticsGameManager : MonoBehaviour
    {
        public static TacticsGameManager Instance { get; private set; }

        [Header("游戏状态")]
        public GamePhase currentPhase = GamePhase.PlayerTurn;
        public int turnNumber = 1;

        [Header("单位列表")]
        public List<Unit> playerUnits = new List<Unit>();
        public List<Unit> enemyUnits = new List<Unit>();

        [Header("AI设置")]
        public bool useAutoBattle = false;  // 是否自动战斗（用于AI测试）
        public float aiActionDelay = 0.5f;  // AI行动间隔

        // 当前行动的AI单位索引
        private int currentEnemyIndex = 0;
        private float aiTimer = 0f;

        // 事件
        public event System.Action<GamePhase> OnPhaseChanged;
        public event System.Action<int> OnTurnChanged;
        public event System.Action<Team> OnGameOver;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // 收集场景中的所有单位
            CollectUnits();
            StartPlayerTurn();
        }

        private void Update()
        {
            // 处理AI回合
            if (currentPhase == GamePhase.EnemyTurn)
            {
                ProcessEnemyTurn();
            }

            // 自动战斗模式下，玩家回合也由AI控制
            if (useAutoBattle && currentPhase == GamePhase.PlayerTurn)
            {
                ProcessPlayerAutoBattle();
            }
        }

        /// <summary>
        /// 收集场景中的所有单位
        /// </summary>
        public void CollectUnits()
        {
            playerUnits.Clear();
            enemyUnits.Clear();

            Unit[] allUnits = FindObjectsOfType<Unit>();
            foreach (var unit in allUnits)
            {
                if (unit.team == Team.Player)
                {
                    playerUnits.Add(unit);
                    unit.OnDeath += OnUnitDeath;
                }
                else
                {
                    enemyUnits.Add(unit);
                    unit.OnDeath += OnUnitDeath;
                }
            }

            Debug.Log($"收集到 {playerUnits.Count} 个玩家单位，{enemyUnits.Count} 个敌方单位");
        }

        /// <summary>
        /// 开始玩家回合
        /// </summary>
        public void StartPlayerTurn()
        {
            currentPhase = GamePhase.PlayerTurn;

            // 重置玩家单位状态
            foreach (var unit in playerUnits)
            {
                if (!unit.isDead)
                {
                    unit.ResetTurn();
                }
            }

            Debug.Log($"=== 玩家回合 {turnNumber} 开始 ===");
            OnPhaseChanged?.Invoke(currentPhase);
        }

        /// <summary>
        /// 结束玩家回合
        /// </summary>
        public void EndPlayerTurn()
        {
            if (currentPhase != GamePhase.PlayerTurn) return;

            StartEnemyTurn();
        }

        /// <summary>
        /// 开始敌方回合
        /// </summary>
        public void StartEnemyTurn()
        {
            currentPhase = GamePhase.EnemyTurn;
            currentEnemyIndex = 0;
            aiTimer = 0f;

            // 重置敌方单位状态
            foreach (var unit in enemyUnits)
            {
                if (!unit.isDead)
                {
                    unit.ResetTurn();
                }
            }

            Debug.Log($"=== 敌方回合 {turnNumber} 开始 ===");
            OnPhaseChanged?.Invoke(currentPhase);
        }

        /// <summary>
        /// 处理敌方AI回合
        /// </summary>
        private void ProcessEnemyTurn()
        {
            aiTimer += Time.deltaTime;
            if (aiTimer < aiActionDelay) return;
            aiTimer = 0f;

            // 找到下一个可行动的敌方单位
            while (currentEnemyIndex < enemyUnits.Count)
            {
                Unit enemy = enemyUnits[currentEnemyIndex];
                if (!enemy.isDead && enemy.CanAct())
                {
                    // 获取AI控制器并执行行动
                    var aiController = enemy.GetComponent<IAIController>();
                    if (aiController != null)
                    {
                        aiController.ExecuteAI();
                    }

                    // 如果单位完成了所有行动，移动到下一个
                    if (!enemy.CanAct())
                    {
                        currentEnemyIndex++;
                    }
                    return;
                }
                currentEnemyIndex++;
            }

            // 所有敌方单位行动完毕，结束敌方回合
            EndEnemyTurn();
        }

        /// <summary>
        /// 处理玩家自动战斗（AI测试用）
        /// </summary>
        private void ProcessPlayerAutoBattle()
        {
            aiTimer += Time.deltaTime;
            if (aiTimer < aiActionDelay) return;
            aiTimer = 0f;

            foreach (var unit in playerUnits)
            {
                if (!unit.isDead && unit.CanAct())
                {
                    var aiController = unit.GetComponent<IAIController>();
                    if (aiController != null)
                    {
                        aiController.ExecuteAI();
                    }
                    return;
                }
            }

            // 所有玩家单位行动完毕
            EndPlayerTurn();
        }

        /// <summary>
        /// 结束敌方回合
        /// </summary>
        private void EndEnemyTurn()
        {
            turnNumber++;
            OnTurnChanged?.Invoke(turnNumber);
            StartPlayerTurn();
        }

        /// <summary>
        /// 单位死亡回调
        /// </summary>
        private void OnUnitDeath(Unit unit)
        {
            CheckGameOver();
        }

        /// <summary>
        /// 检查游戏是否结束
        /// </summary>
        private void CheckGameOver()
        {
            // 检查玩家单位
            bool allPlayersDead = playerUnits.All(u => u.isDead);
            if (allPlayersDead)
            {
                GameOver(Team.Enemy);
                return;
            }

            // 检查敌方单位
            bool allEnemiesDead = enemyUnits.All(u => u.isDead);
            if (allEnemiesDead)
            {
                GameOver(Team.Player);
            }
        }

        /// <summary>
        /// 游戏结束
        /// </summary>
        private void GameOver(Team winner)
        {
            currentPhase = GamePhase.GameOver;
            Debug.Log($"=== 游戏结束！{winner} 获胜！===");
            OnGameOver?.Invoke(winner);
            OnPhaseChanged?.Invoke(currentPhase);
        }

        /// <summary>
        /// 获取敌对单位列表
        /// </summary>
        public List<Unit> GetEnemiesOf(Team team)
        {
            if (team == Team.Player)
            {
                return enemyUnits.Where(u => !u.isDead).ToList();
            }
            return playerUnits.Where(u => !u.isDead).ToList();
        }

        /// <summary>
        /// 获取最近的敌人
        /// </summary>
        public Unit GetNearestEnemy(Unit unit)
        {
            List<Unit> enemies = GetEnemiesOf(unit.team);
            if (enemies.Count == 0) return null;

            Unit nearest = null;
            int minDistance = int.MaxValue;

            foreach (var enemy in enemies)
            {
                int dist = GridSystem.Instance.GetManhattanDistance(unit.GridPosition, enemy.GridPosition);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearest = enemy;
                }
            }

            return nearest;
        }

        /// <summary>
        /// 获取攻击范围内的敌人
        /// </summary>
        public List<Unit> GetEnemiesInAttackRange(Unit unit)
        {
            List<Unit> enemies = GetEnemiesOf(unit.team);
            return enemies.Where(e => unit.IsInAttackRange(e)).ToList();
        }
    }

    /// <summary>
    /// AI控制器接口
    /// </summary>
    public interface IAIController
    {
        void ExecuteAI();
    }
}
