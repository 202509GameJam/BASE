using UnityEngine;
using System.Collections.Generic;
using TacticsAI.Core;

namespace TacticsAI.BehaviourTree
{
    /// <summary>
    /// 行为树节点状态
    /// </summary>
    public enum NodeStatus
    {
        Running,    // 正在运行
        Success,    // 成功
        Failure     // 失败
    }

    /// <summary>
    /// 行为树节点基类
    /// </summary>
    public abstract class BTNode
    {
        protected Unit owner;
        protected string nodeName;

        public BTNode(Unit owner, string name = "")
        {
            this.owner = owner;
            this.nodeName = string.IsNullOrEmpty(name) ? GetType().Name : name;
        }

        public abstract NodeStatus Tick();

        protected void Log(string message)
        {
            Debug.Log($"[BT] {owner.unitName} - {nodeName}: {message}");
        }
    }

    #region 组合节点 (Composite Nodes)

    /// <summary>
    /// 序列节点 - 顺序执行子节点，全部成功才返回成功
    /// </summary>
    public class Sequence : BTNode
    {
        protected List<BTNode> children = new List<BTNode>();

        public Sequence(Unit owner, string name = "Sequence") : base(owner, name) { }

        public Sequence AddChild(BTNode child)
        {
            children.Add(child);
            return this;
        }

        public override NodeStatus Tick()
        {
            foreach (var child in children)
            {
                NodeStatus status = child.Tick();
                if (status != NodeStatus.Success)
                {
                    return status;
                }
            }
            return NodeStatus.Success;
        }
    }

    /// <summary>
    /// 选择节点 - 顺序执行子节点，任一成功就返回成功
    /// </summary>
    public class Selector : BTNode
    {
        protected List<BTNode> children = new List<BTNode>();

        public Selector(Unit owner, string name = "Selector") : base(owner, name) { }

        public Selector AddChild(BTNode child)
        {
            children.Add(child);
            return this;
        }

        public override NodeStatus Tick()
        {
            foreach (var child in children)
            {
                NodeStatus status = child.Tick();
                if (status != NodeStatus.Failure)
                {
                    return status;
                }
            }
            return NodeStatus.Failure;
        }
    }

    /// <summary>
    /// 并行节点 - 同时执行所有子节点
    /// </summary>
    public class Parallel : BTNode
    {
        protected List<BTNode> children = new List<BTNode>();
        private int successThreshold;  // 需要多少个成功才算成功

        public Parallel(Unit owner, int successThreshold = 1, string name = "Parallel") : base(owner, name)
        {
            this.successThreshold = successThreshold;
        }

        public Parallel AddChild(BTNode child)
        {
            children.Add(child);
            return this;
        }

        public override NodeStatus Tick()
        {
            int successCount = 0;
            int failureCount = 0;

            foreach (var child in children)
            {
                NodeStatus status = child.Tick();
                if (status == NodeStatus.Success)
                {
                    successCount++;
                }
                else if (status == NodeStatus.Failure)
                {
                    failureCount++;
                }
            }

            if (successCount >= successThreshold)
            {
                return NodeStatus.Success;
            }
            if (failureCount >= children.Count - successThreshold + 1)
            {
                return NodeStatus.Failure;
            }
            return NodeStatus.Running;
        }
    }

    #endregion

    #region 装饰节点 (Decorator Nodes)

    /// <summary>
    /// 反转节点 - 反转子节点结果
    /// </summary>
    public class Inverter : BTNode
    {
        private BTNode child;

        public Inverter(Unit owner, BTNode child, string name = "Inverter") : base(owner, name)
        {
            this.child = child;
        }

        public override NodeStatus Tick()
        {
            NodeStatus status = child.Tick();
            if (status == NodeStatus.Success)
            {
                return NodeStatus.Failure;
            }
            if (status == NodeStatus.Failure)
            {
                return NodeStatus.Success;
            }
            return NodeStatus.Running;
        }
    }

    /// <summary>
    /// 重复节点 - 重复执行子节点指定次数
    /// </summary>
    public class Repeater : BTNode
    {
        private BTNode child;
        private int repeatCount;
        private int currentCount;

        public Repeater(Unit owner, BTNode child, int count = -1, string name = "Repeater") : base(owner, name)
        {
            this.child = child;
            this.repeatCount = count;
            this.currentCount = 0;
        }

        public override NodeStatus Tick()
        {
            if (repeatCount != -1 && currentCount >= repeatCount)
            {
                return NodeStatus.Success;
            }

            NodeStatus status = child.Tick();
            if (status == NodeStatus.Success || status == NodeStatus.Failure)
            {
                currentCount++;
            }

            return NodeStatus.Running;
        }
    }

    /// <summary>
    /// 成功节点 - 总是返回成功
    /// </summary>
    public class AlwaysSucceed : BTNode
    {
        private BTNode child;

        public AlwaysSucceed(Unit owner, BTNode child, string name = "AlwaysSucceed") : base(owner, name)
        {
            this.child = child;
        }

        public override NodeStatus Tick()
        {
            child.Tick();
            return NodeStatus.Success;
        }
    }

    #endregion

    #region 条件节点 (Condition Nodes)

    /// <summary>
    /// 检查是否有敌人在攻击范围内
    /// </summary>
    public class HasEnemyInAttackRange : BTNode
    {
        public HasEnemyInAttackRange(Unit owner) : base(owner, "HasEnemyInAttackRange") { }

        public override NodeStatus Tick()
        {
            var enemies = TacticsGameManager.Instance.GetEnemiesInAttackRange(owner);
            bool hasEnemy = enemies.Count > 0;
            Log(hasEnemy ? "有敌人在攻击范围内" : "没有敌人在攻击范围内");
            return hasEnemy ? NodeStatus.Success : NodeStatus.Failure;
        }
    }

    /// <summary>
    /// 检查是否低血量
    /// </summary>
    public class IsLowHealth : BTNode
    {
        private float threshold;

        public IsLowHealth(Unit owner, float threshold = 0.3f) : base(owner, "IsLowHealth")
        {
            this.threshold = threshold;
        }

        public override NodeStatus Tick()
        {
            bool isLow = owner.IsLowHealth(threshold);
            Log(isLow ? "血量低" : "血量正常");
            return isLow ? NodeStatus.Success : NodeStatus.Failure;
        }
    }

    /// <summary>
    /// 检查是否被近身
    /// </summary>
    public class IsEngaged : BTNode
    {
        public IsEngaged(Unit owner) : base(owner, "IsEngaged") { }

        public override NodeStatus Tick()
        {
            bool engaged = owner.IsEngagedByEnemy();
            Log(engaged ? "被近身" : "未被近身");
            return engaged ? NodeStatus.Success : NodeStatus.Failure;
        }
    }

    /// <summary>
    /// 检查是否可以移动
    /// </summary>
    public class CanMove : BTNode
    {
        public CanMove(Unit owner) : base(owner, "CanMove") { }

        public override NodeStatus Tick()
        {
            return !owner.hasMoved ? NodeStatus.Success : NodeStatus.Failure;
        }
    }

    /// <summary>
    /// 检查是否可以行动
    /// </summary>
    public class CanAct : BTNode
    {
        public CanAct(Unit owner) : base(owner, "CanAct") { }

        public override NodeStatus Tick()
        {
            return !owner.hasActed ? NodeStatus.Success : NodeStatus.Failure;
        }
    }

    /// <summary>
    /// 检查是否有敌人存在
    /// </summary>
    public class HasEnemy : BTNode
    {
        public HasEnemy(Unit owner) : base(owner, "HasEnemy") { }

        public override NodeStatus Tick()
        {
            var enemy = TacticsGameManager.Instance.GetNearestEnemy(owner);
            return enemy != null ? NodeStatus.Success : NodeStatus.Failure;
        }
    }

    /// <summary>
    /// 检查是否处于安全位置（不会被近身）
    /// </summary>
    public class IsSafePosition : BTNode
    {
        public IsSafePosition(Unit owner) : base(owner, "IsSafePosition") { }

        public override NodeStatus Tick()
        {
            var enemies = TacticsGameManager.Instance.GetEnemiesOf(owner.team);
            foreach (var enemy in enemies)
            {
                int dist = GridSystem.Instance.GetManhattanDistance(owner.GridPosition, enemy.GridPosition);
                // 检查敌人是否能在一回合内近身
                if (dist <= enemy.moveRange + 1)
                {
                    return NodeStatus.Failure;
                }
            }
            return NodeStatus.Success;
        }
    }

    #endregion

    #region 行动节点 (Action Nodes)

    /// <summary>
    /// 攻击范围内的敌人
    /// </summary>
    public class AttackEnemy : BTNode
    {
        public AttackEnemy(Unit owner) : base(owner, "AttackEnemy") { }

        public override NodeStatus Tick()
        {
            if (owner.hasActed)
            {
                return NodeStatus.Failure;
            }

            var enemies = TacticsGameManager.Instance.GetEnemiesInAttackRange(owner);
            if (enemies.Count == 0)
            {
                return NodeStatus.Failure;
            }

            // 选择血量最低的敌人
            Unit target = enemies[0];
            foreach (var enemy in enemies)
            {
                if (enemy.currentHealth < target.currentHealth)
                {
                    target = enemy;
                }
            }

            owner.Attack(target);
            Log($"攻击 {target.unitName}");
            return NodeStatus.Success;
        }
    }

    /// <summary>
    /// 向最近的敌人移动
    /// </summary>
    public class MoveTowardsEnemy : BTNode
    {
        public MoveTowardsEnemy(Unit owner) : base(owner, "MoveTowardsEnemy") { }

        public override NodeStatus Tick()
        {
            if (owner.hasMoved)
            {
                return NodeStatus.Failure;
            }

            Unit target = TacticsGameManager.Instance.GetNearestEnemy(owner);
            if (target == null)
            {
                return NodeStatus.Failure;
            }

            var path = GridSystem.Instance.FindPath(owner.GridPosition, target.GridPosition);
            if (path.Count == 0)
            {
                return NodeStatus.Failure;
            }

            // 移动到接近敌人但不重叠的位置
            int steps = Mathf.Min(path.Count - 1, owner.moveRange);
            if (steps > 0)
            {
                Vector2Int targetPos = path[steps - 1];
                if (GridSystem.Instance.IsWalkable(targetPos))
                {
                    owner.MoveTo(targetPos);
                    Log($"向 {target.unitName} 移动");
                    return NodeStatus.Success;
                }
            }

            return NodeStatus.Failure;
        }
    }

    /// <summary>
    /// 远离敌人（撤退）
    /// </summary>
    public class Retreat : BTNode
    {
        public Retreat(Unit owner) : base(owner, "Retreat") { }

        public override NodeStatus Tick()
        {
            if (owner.hasMoved)
            {
                return NodeStatus.Failure;
            }

            Unit nearestEnemy = TacticsGameManager.Instance.GetNearestEnemy(owner);
            if (nearestEnemy == null)
            {
                return NodeStatus.Failure;
            }

            List<Vector2Int> movableCells = GridSystem.Instance.GetMovableCells(owner.GridPosition, owner.moveRange);

            Vector2Int bestPos = owner.GridPosition;
            int maxDistance = 0;

            foreach (var pos in movableCells)
            {
                int dist = GridSystem.Instance.GetManhattanDistance(pos, nearestEnemy.GridPosition);
                if (dist > maxDistance)
                {
                    maxDistance = dist;
                    bestPos = pos;
                }
            }

            if (bestPos != owner.GridPosition)
            {
                owner.MoveTo(bestPos);
                Log($"撤退到 ({bestPos.x}, {bestPos.y})");
                return NodeStatus.Success;
            }

            return NodeStatus.Failure;
        }
    }

    /// <summary>
    /// 移动到有利攻击位置（保持距离）
    /// </summary>
    public class MoveToAttackPosition : BTNode
    {
        public MoveToAttackPosition(Unit owner) : base(owner, "MoveToAttackPosition") { }

        public override NodeStatus Tick()
        {
            if (owner.hasMoved)
            {
                return NodeStatus.Failure;
            }

            Unit target = TacticsGameManager.Instance.GetNearestEnemy(owner);
            if (target == null)
            {
                return NodeStatus.Failure;
            }

            Vector2Int bestPos = FindBestPosition(target);

            if (bestPos != owner.GridPosition)
            {
                int distance = GridSystem.Instance.GetManhattanDistance(owner.GridPosition, bestPos);
                if (distance <= owner.moveRange)
                {
                    owner.MoveTo(bestPos);
                    Log($"移动到攻击位置 ({bestPos.x}, {bestPos.y})");
                    return NodeStatus.Success;
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
                        Log($"向攻击位置移动");
                        return NodeStatus.Success;
                    }
                }
            }

            return NodeStatus.Failure;
        }

        private Vector2Int FindBestPosition(Unit target)
        {
            List<Vector2Int> movableCells = GridSystem.Instance.GetMovableCells(owner.GridPosition, owner.moveRange);
            movableCells.Add(owner.GridPosition);

            Vector2Int bestPos = owner.GridPosition;
            int bestScore = int.MinValue;

            var allEnemies = TacticsGameManager.Instance.GetEnemiesOf(owner.team);

            foreach (var pos in movableCells)
            {
                int distToTarget = GridSystem.Instance.GetManhattanDistance(pos, target.GridPosition);
                int score = 0;

                // 优先在攻击范围内
                if (distToTarget <= owner.attackRange)
                {
                    score += 100;
                    score += distToTarget * 10; // 在攻击范围内，越远越安全
                }
                else
                {
                    score -= distToTarget * 5;
                }

                // 检查是否会被近身
                foreach (var enemy in allEnemies)
                {
                    if (GridSystem.Instance.GetManhattanDistance(pos, enemy.GridPosition) <= 1)
                    {
                        score -= 200;
                        break;
                    }
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
    /// 逃离近身敌人
    /// </summary>
    public class FleeFromEnemy : BTNode
    {
        public FleeFromEnemy(Unit owner) : base(owner, "FleeFromEnemy") { }

        public override NodeStatus Tick()
        {
            if (owner.hasMoved)
            {
                return NodeStatus.Failure;
            }

            List<Vector2Int> movableCells = GridSystem.Instance.GetMovableCells(owner.GridPosition, owner.moveRange);
            var allEnemies = TacticsGameManager.Instance.GetEnemiesOf(owner.team);

            Vector2Int bestPos = owner.GridPosition;
            int maxMinDistance = 0;

            foreach (var pos in movableCells)
            {
                int minDistToEnemy = int.MaxValue;
                foreach (var enemy in allEnemies)
                {
                    int dist = GridSystem.Instance.GetManhattanDistance(pos, enemy.GridPosition);
                    if (dist < minDistToEnemy)
                    {
                        minDistToEnemy = dist;
                    }
                }

                if (minDistToEnemy > maxMinDistance)
                {
                    maxMinDistance = minDistToEnemy;
                    bestPos = pos;
                }
            }

            if (bestPos != owner.GridPosition)
            {
                owner.MoveTo(bestPos);
                Log($"逃离到 ({bestPos.x}, {bestPos.y})");
                return NodeStatus.Success;
            }

            return NodeStatus.Failure;
        }
    }

    /// <summary>
    /// 待机（不做任何事）
    /// </summary>
    public class Idle : BTNode
    {
        public Idle(Unit owner) : base(owner, "Idle") { }

        public override NodeStatus Tick()
        {
            Log("待机中");
            return NodeStatus.Success;
        }
    }

    #endregion
}
