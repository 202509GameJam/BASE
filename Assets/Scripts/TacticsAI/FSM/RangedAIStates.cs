using UnityEngine;
using System.Collections.Generic;
using TacticsAI.Core;

namespace TacticsAI.FSM
{
    /// <summary>
    /// 远程AI状态枚举
    /// </summary>
    public enum RangedStateType
    {
        Idle,           // 待机
        Attack,         // 攻击
        Reposition,     // 重新定位（保持距离）
        Flee            // 逃离（被近身时）
    }

    #region 远程AI状态

    /// <summary>
    /// 远程AI - 待机状态
    /// </summary>
    public class RangedIdleState : FSMState
    {
        public RangedIdleState(Unit owner, FSMController controller) : base(owner, controller) { }

        public override void Update()
        {
            // 检查是否被近身
            if (owner.IsEngagedByEnemy())
            {
                controller.ChangeState(new RangedFleeState(owner, controller));
                return;
            }

            // 检查是否有敌人在攻击范围内
            var enemies = TacticsGameManager.Instance.GetEnemiesInAttackRange(owner);
            if (enemies.Count > 0)
            {
                controller.ChangeState(new RangedAttackState(owner, controller));
                return;
            }

            // 检查是否需要重新定位
            Unit nearestEnemy = TacticsGameManager.Instance.GetNearestEnemy(owner);
            if (nearestEnemy != null)
            {
                controller.ChangeState(new RangedRepositionState(owner, controller));
            }
        }

        public override void Execute()
        {
            Debug.Log($"[FSM] {owner.unitName} 待机中...");
        }
    }

    /// <summary>
    /// 远程AI - 攻击状态
    /// </summary>
    public class RangedAttackState : FSMState
    {
        public RangedAttackState(Unit owner, FSMController controller) : base(owner, controller) { }

        public override void Update()
        {
            // 优先检查是否被近身
            if (owner.IsEngagedByEnemy())
            {
                controller.ChangeState(new RangedFleeState(owner, controller));
                return;
            }

            // 攻击完成后
            if (owner.hasActed)
            {
                controller.ChangeState(new RangedIdleState(owner, controller));
                return;
            }

            // 如果没有敌人在攻击范围内
            var enemies = TacticsGameManager.Instance.GetEnemiesInAttackRange(owner);
            if (enemies.Count == 0)
            {
                controller.ChangeState(new RangedRepositionState(owner, controller));
            }
        }

        public override void Execute()
        {
            if (owner.hasActed) return;

            var enemies = TacticsGameManager.Instance.GetEnemiesInAttackRange(owner);
            if (enemies.Count > 0)
            {
                // 选择血量最低的敌人攻击
                Unit target = enemies[0];
                foreach (var enemy in enemies)
                {
                    if (enemy.currentHealth < target.currentHealth)
                    {
                        target = enemy;
                    }
                }

                owner.Attack(target);
                Debug.Log($"[FSM] {owner.unitName} 远程攻击 {target.unitName}");
            }
        }
    }

    /// <summary>
    /// 远程AI - 重新定位状态（保持距离并移动至有利位置）
    /// </summary>
    public class RangedRepositionState : FSMState
    {
        public RangedRepositionState(Unit owner, FSMController controller) : base(owner, controller) { }

        public override void Update()
        {
            // 检查是否被近身
            if (owner.IsEngagedByEnemy())
            {
                controller.ChangeState(new RangedFleeState(owner, controller));
                return;
            }

            // 检查是否已经有敌人在攻击范围内
            var enemies = TacticsGameManager.Instance.GetEnemiesInAttackRange(owner);
            if (enemies.Count > 0 && !owner.IsEngagedByEnemy())
            {
                controller.ChangeState(new RangedAttackState(owner, controller));
                return;
            }

            // 移动完成后返回待机
            if (owner.hasMoved)
            {
                controller.ChangeState(new RangedIdleState(owner, controller));
            }
        }

        public override void Execute()
        {
            if (owner.hasMoved) return;

            Unit nearestEnemy = TacticsGameManager.Instance.GetNearestEnemy(owner);
            if (nearestEnemy == null) return;

            // 找到最佳攻击位置：在攻击范围内，但尽可能远离敌人
            Vector2Int bestPos = FindBestAttackPosition(nearestEnemy);

            if (bestPos != owner.GridPosition && GridSystem.Instance.IsWalkable(bestPos))
            {
                // 检查是否能直接移动到目标位置
                int distance = GridSystem.Instance.GetManhattanDistance(owner.GridPosition, bestPos);
                if (distance <= owner.moveRange)
                {
                    owner.MoveTo(bestPos);
                    Debug.Log($"[FSM] {owner.unitName} 重新定位到 ({bestPos.x}, {bestPos.y})");
                }
                else
                {
                    // 沿路径移动
                    var path = GridSystem.Instance.FindPath(owner.GridPosition, bestPos);
                    if (path.Count > 0)
                    {
                        int steps = Mathf.Min(path.Count, owner.moveRange);
                        Vector2Int targetPos = path[steps - 1];
                        owner.MoveTo(targetPos);
                        Debug.Log($"[FSM] {owner.unitName} 向攻击位置移动");
                    }
                }
            }
        }

        /// <summary>
        /// 找到最佳攻击位置
        /// </summary>
        private Vector2Int FindBestAttackPosition(Unit target)
        {
            List<Vector2Int> movableCells = GridSystem.Instance.GetMovableCells(owner.GridPosition, owner.moveRange);
            movableCells.Add(owner.GridPosition); // 包括当前位置

            Vector2Int bestPos = owner.GridPosition;
            int bestScore = int.MinValue;

            foreach (var pos in movableCells)
            {
                int distToTarget = GridSystem.Instance.GetManhattanDistance(pos, target.GridPosition);

                // 评分：优先选择在攻击范围内且距离敌人较远的位置
                int score = 0;

                // 如果在攻击范围内，加分
                if (distToTarget <= owner.attackRange)
                {
                    score += 100;
                    // 在攻击范围内，距离越远越好（保持安全距离）
                    score += distToTarget * 10;
                }
                else
                {
                    // 不在攻击范围内，越近越好（尽快进入攻击范围）
                    score -= distToTarget * 5;
                }

                // 检查是否会被近身
                bool wouldBeEngaged = false;
                var allEnemies = TacticsGameManager.Instance.GetEnemiesOf(owner.team);
                foreach (var enemy in allEnemies)
                {
                    if (GridSystem.Instance.GetManhattanDistance(pos, enemy.GridPosition) <= 1)
                    {
                        wouldBeEngaged = true;
                        break;
                    }
                }

                // 如果会被近身，大幅减分
                if (wouldBeEngaged)
                {
                    score -= 200;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestPos = pos;
                }
            }

            return bestPos;
        }
    }

    /// <summary>
    /// 远程AI - 逃离状态（被近身时远离敌人）
    /// </summary>
    public class RangedFleeState : FSMState
    {
        public RangedFleeState(Unit owner, FSMController controller) : base(owner, controller) { }

        public override void Update()
        {
            // 如果不再被近身，切换到重新定位
            if (!owner.IsEngagedByEnemy())
            {
                controller.ChangeState(new RangedRepositionState(owner, controller));
                return;
            }

            // 移动完成后重新评估
            if (owner.hasMoved)
            {
                if (owner.IsEngagedByEnemy())
                {
                    // 仍然被近身，可能需要攻击自保
                    var enemies = TacticsGameManager.Instance.GetEnemiesInAttackRange(owner);
                    if (enemies.Count > 0 && !owner.hasActed)
                    {
                        controller.ChangeState(new RangedAttackState(owner, controller));
                    }
                }
                else
                {
                    controller.ChangeState(new RangedRepositionState(owner, controller));
                }
            }
        }

        public override void Execute()
        {
            if (owner.hasMoved) return;

            // 找到离所有敌人最远的安全位置
            Vector2Int bestPos = FindSafestPosition();

            if (bestPos != owner.GridPosition)
            {
                owner.MoveTo(bestPos);
                Debug.Log($"[FSM] {owner.unitName} 逃离到 ({bestPos.x}, {bestPos.y})");
            }
        }

        /// <summary>
        /// 找到最安全的位置
        /// </summary>
        private Vector2Int FindSafestPosition()
        {
            List<Vector2Int> movableCells = GridSystem.Instance.GetMovableCells(owner.GridPosition, owner.moveRange);

            Vector2Int bestPos = owner.GridPosition;
            int maxMinDistance = 0;

            var allEnemies = TacticsGameManager.Instance.GetEnemiesOf(owner.team);

            foreach (var pos in movableCells)
            {
                // 计算到最近敌人的距离
                int minDistToEnemy = int.MaxValue;
                foreach (var enemy in allEnemies)
                {
                    int dist = GridSystem.Instance.GetManhattanDistance(pos, enemy.GridPosition);
                    if (dist < minDistToEnemy)
                    {
                        minDistToEnemy = dist;
                    }
                }

                // 选择到最近敌人距离最大的位置
                if (minDistToEnemy > maxMinDistance)
                {
                    maxMinDistance = minDistToEnemy;
                    bestPos = pos;
                }
            }

            return bestPos;
        }
    }

    #endregion

    /// <summary>
    /// 远程AI控制器
    /// </summary>
    public class RangedFSMController : FSMController
    {
        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            // 初始状态为待机
            ChangeState(new RangedIdleState(unit, this));
        }
    }
}
