using UnityEngine;
using System;

namespace TacticsAI.Core
{
    /// <summary>
    /// 单位类型枚举
    /// </summary>
    public enum UnitType
    {
        Melee,   // 近战
        Ranged   // 远程
    }

    /// <summary>
    /// 阵营枚举
    /// </summary>
    public enum Team
    {
        Player,
        Enemy
    }

    /// <summary>
    /// 单位基类 - 所有战斗单位的基础
    /// </summary>
    public class Unit : MonoBehaviour
    {
        [Header("单位基本信息")]
        public string unitName = "Unit";
        public UnitType unitType = UnitType.Melee;
        public Team team = Team.Player;

        [Header("属性")]
        public int maxHealth = 100;
        public int currentHealth;
        public int attackPower = 20;
        public int moveRange = 3;
        public int attackRange = 1;

        [Header("状态")]
        public bool hasMoved = false;
        public bool hasActed = false;
        public bool isDead = false;

        // 网格位置
        public Vector2Int GridPosition { get; set; }

        // 事件
        public event Action<Unit> OnDeath;
        public event Action<Unit, int> OnDamaged;
        public event Action<Unit> OnHealed;

        protected virtual void Awake()
        {
            currentHealth = maxHealth;
        }

        protected virtual void Start()
        {
            // 注册到网格系统
            if (GridSystem.Instance != null)
            {
                Vector2Int pos = GridSystem.Instance.WorldToGrid(transform.position);
                GridSystem.Instance.PlaceUnit(this, pos);
            }
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        public virtual void TakeDamage(int damage)
        {
            if (isDead) return;

            currentHealth -= damage;
            OnDamaged?.Invoke(this, damage);

            Debug.Log($"{unitName} 受到 {damage} 点伤害，剩余血量: {currentHealth}");

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 治疗
        /// </summary>
        public virtual void Heal(int amount)
        {
            if (isDead) return;

            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            OnHealed?.Invoke(this);

            Debug.Log($"{unitName} 恢复 {amount} 点生命值，当前血量: {currentHealth}");
        }

        /// <summary>
        /// 死亡
        /// </summary>
        protected virtual void Die()
        {
            isDead = true;
            OnDeath?.Invoke(this);

            Debug.Log($"{unitName} 已死亡！");

            // 从网格移除
            if (GridSystem.Instance != null)
            {
                GridSystem.Instance.RemoveUnit(GridPosition);
            }

            // 可以在这里播放死亡动画，然后销毁
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 攻击目标单位
        /// </summary>
        public virtual void Attack(Unit target)
        {
            if (target == null || hasActed || isDead) return;

            int distance = GridSystem.Instance.GetManhattanDistance(GridPosition, target.GridPosition);
            if (distance > attackRange)
            {
                Debug.Log($"目标超出攻击范围！");
                return;
            }

            Debug.Log($"{unitName} 攻击 {target.unitName}！");
            target.TakeDamage(attackPower);
            hasActed = true;
        }

        /// <summary>
        /// 移动到目标格子
        /// </summary>
        public virtual bool MoveTo(Vector2Int targetPos)
        {
            if (hasMoved || isDead) return false;

            if (!GridSystem.Instance.IsWalkable(targetPos))
            {
                Debug.Log($"目标位置不可行走！");
                return false;
            }

            int distance = GridSystem.Instance.GetManhattanDistance(GridPosition, targetPos);
            if (distance > moveRange)
            {
                Debug.Log($"目标超出移动范围！");
                return false;
            }

            GridSystem.Instance.PlaceUnit(this, targetPos);
            hasMoved = true;

            Debug.Log($"{unitName} 移动到 ({targetPos.x}, {targetPos.y})");
            return true;
        }

        /// <summary>
        /// 沿路径移动（带移动范围限制）
        /// </summary>
        public virtual bool MoveAlongPath(System.Collections.Generic.List<Vector2Int> path)
        {
            if (path == null || path.Count == 0 || hasMoved || isDead) return false;

            // 只移动移动范围内的步数
            int steps = Mathf.Min(path.Count, moveRange);
            Vector2Int targetPos = path[steps - 1];

            return MoveTo(targetPos);
        }

        /// <summary>
        /// 重置回合状态
        /// </summary>
        public virtual void ResetTurn()
        {
            hasMoved = false;
            hasActed = false;
        }

        /// <summary>
        /// 是否可以行动
        /// </summary>
        public bool CanAct()
        {
            return !isDead && (!hasMoved || !hasActed);
        }

        /// <summary>
        /// 获取生命值百分比
        /// </summary>
        public float GetHealthPercent()
        {
            return (float)currentHealth / maxHealth;
        }

        /// <summary>
        /// 是否低血量（低于30%）
        /// </summary>
        public bool IsLowHealth(float threshold = 0.3f)
        {
            return GetHealthPercent() <= threshold;
        }

        /// <summary>
        /// 检查目标是否在攻击范围内
        /// </summary>
        public bool IsInAttackRange(Unit target)
        {
            if (target == null || GridSystem.Instance == null) return false;
            return GridSystem.Instance.GetManhattanDistance(GridPosition, target.GridPosition) <= attackRange;
        }

        /// <summary>
        /// 检查是否被敌人近身（距离<=1）
        /// </summary>
        public bool IsEngagedByEnemy()
        {
            Unit[] allUnits = FindObjectsOfType<Unit>();
            foreach (var unit in allUnits)
            {
                if (unit.team != team && !unit.isDead)
                {
                    int dist = GridSystem.Instance.GetManhattanDistance(GridPosition, unit.GridPosition);
                    if (dist <= 1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
