using UnityEngine;
using TacticsAI.Core;

namespace TacticsAI.BehaviourTree
{
    /// <summary>
    /// 行为树控制器基类
    /// </summary>
    public abstract class BTController : MonoBehaviour, IAIController
    {
        protected Unit unit;
        protected BTNode rootNode;

        [Header("行为树调试")]
        public string lastStatus;

        protected virtual void Awake()
        {
            unit = GetComponent<Unit>();
        }

        protected virtual void Start()
        {
            BuildTree();
        }

        /// <summary>
        /// 构建行为树 - 子类实现
        /// </summary>
        protected abstract void BuildTree();

        /// <summary>
        /// 执行AI（由TacticsGameManager调用）
        /// </summary>
        public virtual void ExecuteAI()
        {
            if (rootNode != null)
            {
                NodeStatus status = rootNode.Tick();
                lastStatus = status.ToString();
                Debug.Log($"[BT] {unit.unitName} 行为树执行结果: {status}");
            }
        }
    }

    /// <summary>
    /// 近战AI行为树控制器
    ///
    /// 行为逻辑：
    /// 1. 如果低血量 → 撤退
    /// 2. 如果有敌人在攻击范围 → 攻击
    /// 3. 如果有敌人 → 追击
    /// 4. 否则 → 待机
    /// </summary>
    public class MeleeBTController : BTController
    {
        protected override void BuildTree()
        {
            /*
             * 近战AI行为树结构:
             *
             * Selector (根节点 - 选择一个成功的行为)
             * ├── Sequence (撤退行为)
             * │   ├── IsLowHealth
             * │   ├── CanMove
             * │   └── Retreat
             * ├── Sequence (攻击行为)
             * │   ├── HasEnemyInAttackRange
             * │   ├── CanAct
             * │   └── AttackEnemy
             * ├── Sequence (追击行为)
             * │   ├── HasEnemy
             * │   ├── CanMove
             * │   └── MoveTowardsEnemy
             * └── Idle (待机)
             */

            rootNode = new Selector(unit, "MeleeRoot")
                // 撤退行为
                .AddChild(new Sequence(unit, "RetreatBehavior")
                    .AddChild(new IsLowHealth(unit))
                    .AddChild(new CanMove(unit))
                    .AddChild(new Retreat(unit)))
                // 攻击行为
                .AddChild(new Sequence(unit, "AttackBehavior")
                    .AddChild(new HasEnemyInAttackRange(unit))
                    .AddChild(new CanAct(unit))
                    .AddChild(new AttackEnemy(unit)))
                // 追击行为
                .AddChild(new Sequence(unit, "ChaseBehavior")
                    .AddChild(new HasEnemy(unit))
                    .AddChild(new CanMove(unit))
                    .AddChild(new MoveTowardsEnemy(unit)))
                // 待机
                .AddChild(new Idle(unit));

            Debug.Log($"[BT] {unit.unitName} 近战行为树构建完成");
        }
    }

    /// <summary>
    /// 远程AI行为树控制器
    ///
    /// 行为逻辑：
    /// 1. 如果被近身 → 逃离
    /// 2. 如果有敌人在攻击范围且安全 → 攻击
    /// 3. 如果有敌人 → 移动到有利位置
    /// 4. 否则 → 待机
    /// </summary>
    public class RangedBTController : BTController
    {
        protected override void BuildTree()
        {
            /*
             * 远程AI行为树结构:
             *
             * Selector (根节点)
             * ├── Sequence (逃离行为 - 被近身时)
             * │   ├── IsEngaged
             * │   ├── CanMove
             * │   └── FleeFromEnemy
             * ├── Sequence (安全攻击行为)
             * │   ├── HasEnemyInAttackRange
             * │   ├── Inverter(IsEngaged) - 不被近身
             * │   ├── CanAct
             * │   └── AttackEnemy
             * ├── Sequence (重新定位行为)
             * │   ├── HasEnemy
             * │   ├── CanMove
             * │   └── MoveToAttackPosition
             * └── Idle (待机)
             */

            rootNode = new Selector(unit, "RangedRoot")
                // 逃离行为 - 被近身时优先逃离
                .AddChild(new Sequence(unit, "FleeBehavior")
                    .AddChild(new IsEngaged(unit))
                    .AddChild(new CanMove(unit))
                    .AddChild(new FleeFromEnemy(unit)))
                // 安全攻击行为
                .AddChild(new Sequence(unit, "SafeAttackBehavior")
                    .AddChild(new HasEnemyInAttackRange(unit))
                    .AddChild(new Inverter(unit, new IsEngaged(unit), "NotEngaged"))
                    .AddChild(new CanAct(unit))
                    .AddChild(new AttackEnemy(unit)))
                // 被近身时的攻击（无法逃离时自保）
                .AddChild(new Sequence(unit, "EngagedAttackBehavior")
                    .AddChild(new IsEngaged(unit))
                    .AddChild(new HasEnemyInAttackRange(unit))
                    .AddChild(new CanAct(unit))
                    .AddChild(new AttackEnemy(unit)))
                // 重新定位行为
                .AddChild(new Sequence(unit, "RepositionBehavior")
                    .AddChild(new HasEnemy(unit))
                    .AddChild(new CanMove(unit))
                    .AddChild(new MoveToAttackPosition(unit)))
                // 待机
                .AddChild(new Idle(unit));

            Debug.Log($"[BT] {unit.unitName} 远程行为树构建完成");
        }
    }
}
