using UnityEngine;
using System.Collections.Generic;
using TacticsAI.Core;

namespace TacticsAI.FSM
{
    /// <summary>
    /// 近战AI状态枚举
    /// </summary>
    public enum MeleeStateType
    {
        Idle,       // 待机
        Chase,      // 追击
        Attack,     // 攻击
        Retreat     // 撤退
    }

    #region 近战AI状态

    /// <summary>
    /// 近战AI - 待机状态
    /// </summary>
    public class MeleeIdleState : FSMState
    {
        public MeleeIdleState(Unit owner, FSMController controller) : base(owner, controller) { }

        public override void Update()
        {
            // 检查是否需要撤退（低血量）
            if (owner.IsLowHealth())
            {
                controller.ChangeState(new MeleeRetreatState(owner, controller));
                return;
            }

            // 检查是否有敌人在攻击范围内
            var enemies = TacticsGameManager.Instance.GetEnemiesInAttackRange(owner);
            if (enemies.Count > 0)
            {
                controller.ChangeState(new MeleeAttackState(owner, controller));
                return;
            }

            // 检查是否有敌人需要追击
            Unit nearestEnemy = TacticsGameManager.Instance.GetNearestEnemy(owner);
            if (nearestEnemy != null)
            {
                controller.ChangeState(new MeleeChaseState(owner, controller));
            }
        }

        public override void Execute()
        {
            // 待机状态不执行任何行动
            Debug.Log($"[FSM] {owner.unitName} 待机中...");
        }
    }

    /// <summary>
    /// 近战AI - 追击状态
    /// </summary>
    public class MeleeChaseState : FSMState
    {
        public MeleeChaseState(Unit owner, FSMController controller) : base(owner, controller) { }

        public override void Update()
        {
            // 检查是否需要撤退
            if (owner.IsLowHealth())
            {
                controller.ChangeState(new MeleeRetreatState(owner, controller));
                return;
            }

            // 检查是否有敌人在攻击范围内
            var enemies = TacticsGameManager.Instance.GetEnemiesInAttackRange(owner);
            if (enemies.Count > 0)
            {
                controller.ChangeState(new MeleeAttackState(owner, controller));
                return;
            }

            // 如果没有敌人了，返回待机
            if (TacticsGameManager.Instance.GetNearestEnemy(owner) == null)
            {
                controller.ChangeState(new MeleeIdleState(owner, controller));
            }
        }

        public override void Execute()
        {
            if (owner.hasMoved) return;

            Unit target = TacticsGameManager.Instance.GetNearestEnemy(owner);
            if (target == null) return;

            // 寻路到目标
            var path = GridSystem.Instance.FindPath(owner.GridPosition, target.GridPosition);
            if (path.Count > 0)
            {
                // 移动到尽可能接近目标的位置
                int steps = Mathf.Min(path.Count - 1, owner.moveRange); // -1 是为了不移动到敌人格子上
                if (steps > 0)
                {
                    Vector2Int targetPos = path[steps - 1];
                    if (GridSystem.Instance.IsWalkable(targetPos))
                    {
                        owner.MoveTo(targetPos);
                        Debug.Log($"[FSM] {owner.unitName} 追击 {target.unitName}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// 近战AI - 攻击状态
    /// </summary>
    public class MeleeAttackState : FSMState
    {
        public MeleeAttackState(Unit owner, FSMController controller) : base(owner, controller) { }

        public override void Update()
        {
            // 攻击完成后检查状态
            if (owner.hasActed)
            {
                if (owner.IsLowHealth())
                {
                    controller.ChangeState(new MeleeRetreatState(owner, controller));
                }
                else
                {
                    controller.ChangeState(new MeleeIdleState(owner, controller));
                }
                return;
            }

            // 如果没有敌人在范围内，切换到追击
            var enemies = TacticsGameManager.Instance.GetEnemiesInAttackRange(owner);
            if (enemies.Count == 0)
            {
                controller.ChangeState(new MeleeChaseState(owner, controller));
            }
        }

        public override void Execute()
        {
            if (owner.hasActed) return;

            // 攻击范围内的敌人
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
                Debug.Log($"[FSM] {owner.unitName} 攻击 {target.unitName}");
            }
        }
    }

    /// <summary>
    /// 近战AI - 撤退状态
    /// </summary>
    public class MeleeRetreatState : FSMState
    {
        public MeleeRetreatState(Unit owner, FSMController controller) : base(owner, controller) { }

        public override void Update()
        {
            // 如果血量恢复，重新评估
            if (!owner.IsLowHealth())
            {
                controller.ChangeState(new MeleeIdleState(owner, controller));
            }
        }

        public override void Execute()
        {
            if (owner.hasMoved) return;

            // 找到离敌人最远的安全位置
            Unit nearestEnemy = TacticsGameManager.Instance.GetNearestEnemy(owner);
            if (nearestEnemy == null) return;

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
                Debug.Log($"[FSM] {owner.unitName} 撤退到 ({bestPos.x}, {bestPos.y})");
            }
        }
    }

    #endregion

    /// <summary>
    /// 近战AI控制器
    /// </summary>
    public class MeleeFSMController : FSMController
    {
        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            // 初始状态为待机
            ChangeState(new MeleeIdleState(unit, this));
        }
    }
}
